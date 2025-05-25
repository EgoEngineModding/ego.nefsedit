// See LICENSE.txt for license information.

using System.Diagnostics;
using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

internal class Nefs150ReaderStrategy : NefsReaderStrategy
{
	private static readonly ILogger Log = NefsLog.GetLogger();

	protected override NefsVersion Version => NefsVersion.Version150;

	/// <inheritdoc />
	public override async Task<(AesKeyBuffer, uint, uint)> GetAesKeyHeaderSizeAndOffset(EndianBinaryReader reader, long offset,
		CancellationToken token = default)
	{
		var header = await ReadHeaderIntroV150Async(reader, offset, token).ConfigureAwait(false);

		// This version may have a header of 126 bytes, and 2 were added for the sake of RSA encryption
		// we'll back up here to overwrite those last 2 bytes with the bytes contained in next AES section
		// Side-note: last two bytes could be padding and not real data
		return (header.AesKeyBuffer, header.TocSize, 126);
	}

	/// <inheritdoc />
	public override async Task<INefsHeader> ReadHeaderAsync(
		EndianBinaryReader reader,
		long primaryOffset,
		NefsWriterSettings detectedSettings,
		NefsProgress p)
	{
		Log.LogInformation("Detected NeFS version 1.5.0.");

		// Calc weight of each task (6 parts)
		var weight = 1.0f / 6.0f;

		Nefs150TocHeader header;
		using (p.BeginTask(weight, "Reading header"))
		{
			header = await ReadHeaderIntroV150Async(reader, primaryOffset, p.CancellationToken);
		}

		Nefs150HeaderEntryTable entryTable;
		using (p.BeginTask(weight, "Reading entry table"))
		{
			var size = Convert.ToInt32(header.SharedEntryInfoTableStart - header.EntryTableStart);
			entryTable = await Read150HeaderPart1Async(reader, primaryOffset + header.EntryTableStart, size, p);
		}

		Nefs150HeaderSharedEntryInfoTable sharedEntryInfoTable;
		using (p.BeginTask(weight, "Reading shared entry info table"))
		{
			var size = Convert.ToInt32(header.NameTableStart - header.SharedEntryInfoTableStart);
			sharedEntryInfoTable = await Read150HeaderPart2Async(reader, primaryOffset + header.SharedEntryInfoTableStart, size, p);
		}

		NefsHeaderPart3 part3;
		var stream = reader.BaseStream;
		using (p.BeginTask(weight, "Reading name table"))
		{
			var size = Convert.ToInt32(header.BlockTableStart - header.NameTableStart);
			part3 = await ReadHeaderPart3Async(stream, primaryOffset + header.NameTableStart, size, p);
		}

		Nefs150HeaderBlockTable blockTable;
		using (p.BeginTask(weight, "Reading block table"))
		{
			var size = Convert.ToInt32(header.VolumeInfoTableStart - header.BlockTableStart);
			blockTable = await Read150HeaderPart4Async(reader, primaryOffset + header.BlockTableStart, size, entryTable, p);
		}

		NefsHeaderPart5 part5;
		using (p.BeginTask(weight, "Reading volume info table"))
		{
			var size = Convert.ToInt32(header.NumVolumes * Nefs150TocVolumeInfo.ByteCount);
			part5 = await ReadHeaderPart5Async(reader, primaryOffset + header.VolumeInfoTableStart, size, p);
		}

		return new Nefs150Header(detectedSettings, header, entryTable, sharedEntryInfoTable, part3, blockTable, part5);
	}

	public override Task<INefsHeader> ReadHeaderAsync(EndianBinaryReader reader, long primaryOffset, int? primarySize,
		long secondaryOffset, int? secondarySize, NefsProgress p)
	{
		throw new InvalidOperationException("NeFS version 1.5.0 does not support separated headers.");
	}

	/// <summary>
	/// Reads the 1.5.0 header from an input stream.
	/// </summary>
	/// <summary>
	/// Reads the header intro from an input stream. This is for non-encrypted headers only.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header intro from the beginning of the stream.</param>
	/// <param name="token">The cancellation token.</param>
	/// <returns>The loaded header intro.</returns>
	private static async Task<Nefs150TocHeader> ReadHeaderIntroV150Async(
		EndianBinaryReader reader,
		long offset,
		CancellationToken token = default)
	{
		reader.BaseStream.Seek(offset, SeekOrigin.Begin);
		var header = await reader.ReadTocDataAsync<Nefs150TocHeader>(token).ConfigureAwait(false);
		return header;
	}

	/// <summary>
	/// Reads 1.5.0 header part 1 from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal static async ValueTask<Nefs150HeaderEntryTable> Read150HeaderPart1Async(EndianBinaryReader reader, long offset,
		int size, NefsProgress p)
	{
		var entries = await ReadTocEntriesAsync<Nefs150TocEntry>(reader, offset, size, p).ConfigureAwait(false);
		return new Nefs150HeaderEntryTable(entries);
	}

	/// <summary>
	/// Reads 1.5.0 header part 2 from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	protected async Task<Nefs150HeaderSharedEntryInfoTable> Read150HeaderPart2Async(EndianBinaryReader reader, long offset, int size,
		NefsProgress p)
	{
		var entries = await ReadTocEntriesAsync<Nefs150TocSharedEntryInfo>(reader, offset, size, p).ConfigureAwait(false);
		Debug.Assert(entries.All(x => x.FirstDuplicate == x.PatchedEntry));
		return new Nefs150HeaderSharedEntryInfoTable(entries);
	}

	/// <summary>
	/// Reads 1.5.0 header part 4 from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="entryTable">Header part 1.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	protected async Task<Nefs150HeaderBlockTable> Read150HeaderPart4Async(EndianBinaryReader reader, long offset, int size,
		Nefs150HeaderEntryTable entryTable, NefsProgress p)
	{
		var entries = await ReadTocEntriesAsync<Nefs150TocBlock>(reader, offset, size, p).ConfigureAwait(false);

		// TODO: I believe this is padding to reach multiple of EntrySize boundary
		// Get the unknown last value at the end of part 4
		var endValue = await reader.ReadUInt32Async(p.CancellationToken).ConfigureAwait(false);

		return new Nefs150HeaderBlockTable(entries, endValue);
	}

	/// <summary>
	/// Reads header part 5 from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	internal static async Task<NefsHeaderPart5> ReadHeaderPart5Async(EndianBinaryReader reader, long offset, int size,
		NefsProgress p)
	{
		// TODO: fix reading multiple volumes
		var entries = await ReadTocEntriesAsync<Nefs150TocVolumeInfo>(reader, offset, size, p).ConfigureAwait(false);
		return entries.Select(x => new NefsHeaderPart5(x)).First();
	}
}
