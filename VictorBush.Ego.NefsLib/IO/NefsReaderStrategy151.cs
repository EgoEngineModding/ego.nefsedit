using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

internal class NefsReaderStrategy151 : NefsReaderStrategy150
{
	private static readonly ILogger Log = NefsLog.GetLogger();

	protected override NefsVersion Version => NefsVersion.Version151;

	/// <inheritdoc />
	public override async Task<(AesKeyHexBuffer, uint, uint)> GetAesKeyHeaderSizeAndOffset(EndianBinaryReader reader,
		long offset, CancellationToken token = default)
	{
		var header = await ReadHeaderIntroV151Async(reader, offset, token).ConfigureAwait(false);

		// This version may have a header of 126 bytes, and 2 were added for the sake of RSA encryption
		// we'll back up here to overwrite those last 2 bytes with the bytes contained in next AES section
		// Side-note: last two bytes could be padding and not real data
		return (header.AesKey, header.TocSize, 126);
	}

	/// <inheritdoc />
	public override async Task<INefsHeader> ReadHeaderAsync(EndianBinaryReader reader, long primaryOffset,
		NefsWriterSettings detectedSettings, NefsProgress p)
	{
		Log.LogInformation("Detected NeFS version 1.5.1.");

		// Calc weight of each task (5 parts + header)
		var weight = 1.0f / 6.0f;

		NefsTocHeader151 header;
		using (p.BeginTask(weight, "Reading header"))
		{
			header = await ReadHeaderIntroV151Async(reader, primaryOffset, p.CancellationToken);
		}

		NefsHeaderEntryTable150 entryTable;
		using (p.BeginTask(weight, "Reading entry table"))
		{
			var size = Convert.ToInt32(header.SharedEntryInfoTableStart - header.EntryTableStart);
			entryTable = await Read150HeaderPart1Async(reader, primaryOffset + header.EntryTableStart, size, p);
		}

		NefsHeaderSharedEntryInfoTable150 sharedEntryInfoTable;
		using (p.BeginTask(weight, "Reading shared entry info table"))
		{
			var size = Convert.ToInt32(header.NameTableStart - header.SharedEntryInfoTableStart);
			sharedEntryInfoTable = await Read150HeaderPart2Async(reader, primaryOffset + header.SharedEntryInfoTableStart, size, p);
		}

		NefsHeaderNameTable nameTable;
		var stream = reader.BaseStream;
		using (p.BeginTask(weight, "Reading name table"))
		{
			var size = Convert.ToInt32(header.BlockTableStart - header.NameTableStart);
			nameTable = await ReadHeaderPart3Async(stream, primaryOffset + header.NameTableStart, size, p);
		}

		NefsHeaderBlockTable151 blockTable;
		using (p.BeginTask(weight, "Reading block table"))
		{
			var size = Convert.ToInt32(header.VolumeInfoTableStart - header.BlockTableStart);
			blockTable = await Read151HeaderPart4Async(reader, primaryOffset + header.BlockTableStart, size, p);
		}

		NefsHeaderVolumeInfoTable150 volumeInfoTable;
		using (p.BeginTask(weight, "Reading volume info table"))
		{
			var size = Convert.ToInt32(header.NumVolumes * NefsTocVolumeInfo150.ByteCount);
			volumeInfoTable = await ReadHeaderPart5Async(reader, primaryOffset + header.VolumeInfoTableStart, size, p);
		}

		return new NefsHeader151(detectedSettings, header, entryTable, sharedEntryInfoTable, nameTable, blockTable, volumeInfoTable);
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
	private static async Task<NefsTocHeader151> ReadHeaderIntroV151Async(
		EndianBinaryReader reader,
		long offset,
		CancellationToken token = default)
	{
		reader.BaseStream.Seek(offset, SeekOrigin.Begin);
		var header = await reader.ReadTocDataAsync<NefsTocHeader151>(token).ConfigureAwait(false);
		return header;
	}

	/// <summary>
	/// Reads 1.5.1 header part 4 from an input stream.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header part from the beginning of the stream.</param>
	/// <param name="size">The size of the header part.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header part.</returns>
	protected static async Task<NefsHeaderBlockTable151> Read151HeaderPart4Async(EndianBinaryReader reader, long offset,
		int size, NefsProgress p)
	{
		var entries = await ReadTocEntriesAsync<NefsTocBlock151>(reader, offset, size, p).ConfigureAwait(false);
		return new NefsHeaderBlockTable151(entries);
	}
}
