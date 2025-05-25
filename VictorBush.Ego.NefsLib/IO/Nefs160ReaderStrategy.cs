// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Source.Utility;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.IO;

internal class Nefs160ReaderStrategy : Nefs151ReaderStrategy
{
	private static readonly ILogger Log = NefsLog.GetLogger();

	protected override NefsVersion Version => NefsVersion.Version160;

	/// <inheritdoc />
	public override async Task<(AesKeyBuffer, uint, uint)> GetAesKeyHeaderSizeAndOffset(EndianBinaryReader reader,
		long offset, CancellationToken token = default)
	{
		var header = await ReadHeaderIntroV160Async(reader, offset, token).ConfigureAwait(false);

		return (header.AesKey, header.TocSize, 128);
	}

	/// <inheritdoc />
	public override async Task<INefsHeader> ReadHeaderAsync(EndianBinaryReader reader, long primaryOffset,
		NefsWriterSettings detectedSettings, NefsProgress p)
	{
		Log.LogInformation("Detected NeFS {Version}.", Version.ToPrettyString());

		var header = await ReadHeaderCoreAsync(reader, primaryOffset, primaryOffset, detectedSettings, p)
			.ConfigureAwait(false);
		if (header.IsEncrypted)
		{
			await ValidateEncryptedHeaderAsync(reader.BaseStream, primaryOffset, header.Size, header.Hash);
		}
		else
		{
			await ValidateHeaderHashAsync(reader.BaseStream, primaryOffset, header.Size, header.Hash);
		}

		return header;
	}

	/// <inheritdoc />
	public override async Task<INefsHeader> ReadHeaderAsync(EndianBinaryReader reader, long primaryOffset,
		int? primarySize, long secondaryOffset, int? secondarySize, NefsProgress p)
	{
		var writerSettings = new NefsWriterSettings
		{
			IsLittleEndian = reader.IsLittleEndian
		};
		var header = await ReadHeaderCoreAsync(reader, primaryOffset, secondaryOffset, writerSettings, p)
			.ConfigureAwait(false);
		await ValidateSplitHeaderHashAsync(reader.BaseStream, primaryOffset, primarySize, secondaryOffset,
			secondarySize, header.Hash).ConfigureAwait(false);
		return header;
	}

	protected virtual async Task<INefsHeader> ReadHeaderCoreAsync(EndianBinaryReader reader, long primaryOffset,
		long secondaryOffset, NefsWriterSettings detectedSettings, NefsProgress p)
	{
		// Calc weight of each task (8 parts + header + table of contents)
		var weight = 1.0f / 10.0f;

		Nefs160TocHeaderA header;
		using (p.BeginTask(weight, "Reading header"))
		{
			header = await ReadHeaderIntroV160Async(reader, primaryOffset, p.CancellationToken).ConfigureAwait(false);
		}

		Nefs160TocHeaderB toc;
		using (p.BeginTask(weight, "Reading header table of contents"))
		{
			toc = await ReadHeaderIntroTocVersion16Async(reader, primaryOffset + Nefs160TocHeaderA.ByteCount, p);
		}

		Nefs160HeaderEntryTable entryTable;
		using (p.BeginTask(weight, "Reading entry table"))
		{
			var size = Convert.ToInt32(toc.SharedEntryInfoTableStart - toc.EntryTableStart);
			entryTable = await ReadHeaderPart1Async(reader, primaryOffset + toc.EntryTableStart, size, p);
		}

		Nefs160HeaderSharedEntryInfoTable sharedEntryInfoTable;
		using (p.BeginTask(weight, "Reading shared entry info table"))
		{
			var size = Convert.ToInt32(toc.NameTableStart - toc.SharedEntryInfoTableStart);
			sharedEntryInfoTable = await ReadHeaderPart2Async(reader, primaryOffset + toc.SharedEntryInfoTableStart, size, p);
		}

		NefsHeaderPart3 part3;
		using (p.BeginTask(weight, "Reading name table"))
		{
			var size = Convert.ToInt32(toc.BlockTableStart - toc.NameTableStart);
			part3 = await ReadHeaderPart3Async(reader.BaseStream, primaryOffset + toc.NameTableStart, size, p);
		}

		Nefs151HeaderBlockTable blockTable;
		using (p.BeginTask(weight, "Reading block table"))
		{
			var size = Convert.ToInt32(toc.VolumeInfoTableStart - toc.BlockTableStart);
			blockTable = await Read151HeaderPart4Async(reader, primaryOffset + toc.BlockTableStart, size, p);
		}

		NefsHeaderPart5 part5;
		using (p.BeginTask(weight, "Reading volume info table"))
		{
			var size = Convert.ToInt32(toc.NumVolumes * Nefs150TocVolumeInfo.ByteCount);
			part5 = await ReadHeaderPart5Async(reader, primaryOffset + toc.VolumeInfoTableStart, size, p);
		}

		Nefs160HeaderWriteableEntryTable writeableEntryTable;
		using (p.BeginTask(weight, "Reading entry writable table"))
		{
			var numEntries = entryTable.Entries.Count;
			writeableEntryTable = await Read160HeaderPart6Async(reader, secondaryOffset + toc.WritableEntryTableStart, numEntries, p);
		}

		Nefs160HeaderWriteableSharedEntryInfo writeableSharedEntryInfo;
		using (p.BeginTask(weight, "Reading shared entry info writable table"))
		{
			var numEntries = sharedEntryInfoTable.Entries.Count;
			writeableSharedEntryInfo = await Read160HeaderPart7Async(reader, secondaryOffset + toc.WritableSharedEntryInfoTableStart, numEntries, p);
		}

		Nefs160HeaderHashDigestTable hashDigestTable;
		using (p.BeginTask(weight, "Reading hash digest table"))
		{
			// Part 8 must use primary offset because, for split headers, it is contained in the primary header section
			// (after part 5)
			hashDigestTable = await Read160HeaderPart8Async(reader, primaryOffset + toc.HashDigestTableStart, toc.HashBlockSize, part5, p);
		}

		return new Nefs160Header(detectedSettings, header, toc, entryTable, sharedEntryInfoTable, part3, blockTable,
			part5, writeableEntryTable, writeableSharedEntryInfo, hashDigestTable);
	}

	private static async Task ValidateEncryptedHeaderAsync(Stream stream, long offset, uint headerSize,
		Sha256Hash expectedHash)
	{
		var hash = await HashHelper.HashEncryptedHeaderAsync(stream, offset, Convert.ToInt32(headerSize));
		if (hash != expectedHash)
		{
			Log.LogWarning("Header hash does not match expected value.");
		}
	}

	private static async Task ValidateHeaderHashAsync(Stream stream, long offset, uint headerSize,
		Sha256Hash expectedHash)
	{
		var hash = await HashHelper.HashStandardHeaderAsync(stream, offset, Convert.ToInt32(headerSize));
		if (hash != expectedHash)
		{
			Log.LogWarning("Header hash does not match expected value.");
		}
	}

	private static async Task ValidateSplitHeaderHashAsync(Stream stream, long primaryOffset, int? primarySize,
		long secondaryOffset, int? secondarySize, Sha256Hash expectedHash)
	{
		if (!primarySize.HasValue)
		{
			Log.LogWarning("Split-header primary size not specified, cannot validate hash.");
			return;
		}

		if (!secondarySize.HasValue)
		{
			Log.LogWarning("Split-header secondary size not specified, cannot validate hash.");
			return;
		}

		var hash = await HashHelper.HashSplitHeaderAsync(stream, primaryOffset, primarySize.Value, secondaryOffset,
			secondarySize.Value);
		if (hash != expectedHash)
		{
			Log.LogWarning("Header hash does not match expected value.");
		}
	}

	/// <summary>
	/// Reads the 1.5.1 header from an input stream.
	/// </summary>
	/// <summary>
	/// Reads the header intro from an input stream. This is for non-encrypted headers only.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header intro from the beginning of the stream.</param>
	/// <param name="token">The cancellation token.</param>
	/// <returns>The loaded header intro.</returns>
	protected static async Task<Nefs160TocHeaderA> ReadHeaderIntroV160Async(
		EndianBinaryReader reader,
		long offset,
		CancellationToken token = default)
	{
		reader.BaseStream.Seek(offset, SeekOrigin.Begin);
		var header = await reader.ReadTocDataAsync<Nefs160TocHeaderA>(token).ConfigureAwait(false);
		return header;
	}

	/// <summary>
	/// Reads the header intro table of contents from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header intro table of contents from the beginning of the stream.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header intro offsets data.</returns>
	internal static async Task<Nefs160TocHeaderB> ReadHeaderIntroTocVersion16Async(EndianBinaryReader reader,
		long offset, NefsProgress p)
	{
		reader.BaseStream.Seek(offset, SeekOrigin.Begin);
		var header = await reader.ReadTocDataAsync<Nefs160TocHeaderB>(p.CancellationToken).ConfigureAwait(false);
		return header;
	}

	/// <summary>
	/// Reads header part 1 from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal static async Task<Nefs160HeaderEntryTable> ReadHeaderPart1Async(EndianBinaryReader reader, long offset, int size,
		NefsProgress p)
	{
		var entries = await ReadTocEntriesAsync<Nefs160TocEntry>(reader, offset, size, p).ConfigureAwait(false);
		return new Nefs160HeaderEntryTable(entries);
	}

	/// <summary>
	/// Reads header part 2 from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal static async Task<Nefs160HeaderSharedEntryInfoTable> ReadHeaderPart2Async(EndianBinaryReader reader, long offset, int size,
		NefsProgress p)
	{
		var entries = await ReadTocEntriesAsync<Nefs160TocSharedEntryInfo>(reader, offset, size, p).ConfigureAwait(false);
		return new Nefs160HeaderSharedEntryInfoTable(entries);
	}

	/// <summary>
	/// Reads header part 6 from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="numEntries">Number of entries.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal static async Task<Nefs160HeaderWriteableEntryTable> Read160HeaderPart6Async(EndianBinaryReader reader, long offset,
		int numEntries, NefsProgress p)
	{
		var size = numEntries * Nefs160TocEntryWriteable.ByteCount;
		var entries = await ReadTocEntriesAsync<Nefs160TocEntryWriteable>(reader, offset, size, p)
			.ConfigureAwait(false);
		return new Nefs160HeaderWriteableEntryTable(entries);
	}

	/// <summary>
	/// Reads header part 7 from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="numEntries">Number of entries.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal static async Task<Nefs160HeaderWriteableSharedEntryInfo> Read160HeaderPart7Async(EndianBinaryReader reader, long offset,
		int numEntries, NefsProgress p)
	{
		var size = numEntries * Nefs160TocSharedEntryInfoWriteable.ByteCount;
		var entries = await ReadTocEntriesAsync<Nefs160TocSharedEntryInfoWriteable>(reader, offset, size, p)
			.ConfigureAwait(false);
		return new Nefs160HeaderWriteableSharedEntryInfo(entries);
	}

	/// <summary>
	/// Reads header part 8 from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="hashBlockSize">The block size specified by the header used to split up the file data for hashing.</param>
	/// <param name="part5">Header part 5.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal static async Task<Nefs160HeaderHashDigestTable> Read160HeaderPart8Async(EndianBinaryReader reader, long offset,
		uint hashBlockSize, NefsHeaderPart5 part5, NefsProgress p)
	{
		// Archives can specify a 0 hash block size (use default in this case)
		if (hashBlockSize == 0)
		{
			hashBlockSize = NefsWriter.DefaultHashBlockSize;
		}

		var totalCompressedDataSize = part5.DataSize - part5.FirstDataOffset;
		var numHashes = (int)((totalCompressedDataSize + hashBlockSize - 1) / hashBlockSize);
		var size = numHashes * Nefs160TocHashDigest.ByteCount;
		var entries = await ReadTocEntriesAsync<Nefs160TocHashDigest>(reader, offset, size, p)
			.ConfigureAwait(false);
		return new Nefs160HeaderHashDigestTable(entries);
	}
}
