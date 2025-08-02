// See LICENSE.txt for license information.

using Microsoft.Extensions.Logging;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version010;
using VictorBush.Ego.NefsLib.Header.Version130;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

internal class NefsReaderStrategy130 : NefsReaderStrategy
{
	private static readonly ILogger Log = NefsLog.GetLogger();

	protected override NefsVersion Version => NefsVersion.Version130;

	/// <inheritdoc />
	public override async Task<(AesKeyHexBuffer, uint, uint)> GetAesKeyHeaderSizeAndOffset(EndianBinaryReader reader, long offset,
		CancellationToken token = default)
	{
		var header = await ReadTocDataAsync<NefsTocHeader130>(reader, offset, token).ConfigureAwait(false);
		return (header.AesKey, header.TocSize, 126);
	}

	/// <inheritdoc />
	public override async Task<INefsHeader> ReadHeaderAsync(
		EndianBinaryReader reader,
		long primaryOffset,
		NefsWriterSettings detectedSettings,
		NefsProgress p)
	{
		// Calc weight of each task (7 parts plus header)
		var weight = 1.0f / 8.0f;

		NefsTocHeader130 header;
		using (p.BeginTask(weight, "Reading header"))
		{
			header = await ReadTocDataAsync<NefsTocHeader130>(reader, primaryOffset, p.CancellationToken);
		}

		NefsHeaderEntryTable010 entryTable;
		using (p.BeginTask(weight, "Reading entry table"))
		{
			var size = Convert.ToInt32(header.LinkTableStart - header.EntryTableStart);
			entryTable = await ReadTocTableZeroStopAsync<NefsHeaderEntryTable010, NefsTocEntry010>(reader,
				primaryOffset + header.EntryTableStart, size, p);
		}

		NefsHeaderLinkTable010 linkTable;
		using (p.BeginTask(weight, "Reading link table"))
		{
			var size = Convert.ToInt32(header.NameTableStart - header.LinkTableStart);
			linkTable = await ReadTocTableAsync<NefsHeaderLinkTable010, NefsTocLink010>(reader,
				primaryOffset + header.LinkTableStart, size, p);
		}

		NefsHeaderNameTable nameTable;
		var stream = reader.BaseStream;
		using (p.BeginTask(weight, "Reading name table"))
		{
			var size = Convert.ToInt32(header.BlockTableStart - header.NameTableStart);
			nameTable = await ReadHeaderPart3Async(stream, primaryOffset + header.NameTableStart, size, p);
		}

		NefsHeaderBlockTable010 blockTable;
		using (p.BeginTask(weight, "Reading block table"))
		{
			var size = Convert.ToInt32(header.VolumeSizeTableStart - header.BlockTableStart);
			blockTable = await ReadTocTableAsync<NefsHeaderBlockTable010, NefsTocBlock010>(reader,
				primaryOffset + header.BlockTableStart, size, p);
		}

		NefsHeaderVolumeSizeTable010 volumeSizeTable;
		using (p.BeginTask(weight, "Reading volume size table"))
		{
			var size = Convert.ToInt32(header.NumVolumes * NefsTocVolumeSize010.ByteCount);
			volumeSizeTable = await ReadTocTableAsync<NefsHeaderVolumeSizeTable010, NefsTocVolumeSize010>(reader,
				primaryOffset + header.VolumeSizeTableStart, size, p);
		}

		NefsHeaderVolumeNameStartTable130 volumeNameStartTable;
		using (p.BeginTask(weight, "Reading volume name start table"))
		{
			var size = Convert.ToInt32(header.NumVolumes * NefsTocVolumeNameStart130.ByteCount);
			volumeNameStartTable =
				await ReadTocTableAsync<NefsHeaderVolumeNameStartTable130, NefsTocVolumeNameStart130>(reader,
					primaryOffset + header.VolumeNameStartTableStart, size, p);
		}

		NefsHeaderNameTable volumeNameTable;
		using (p.BeginTask(weight, "Reading volume name table"))
		{
			var size = Convert.ToInt32(stream.Length - header.VolumeNameTableStart);
			volumeNameTable = await ReadHeaderPart3Async(stream, primaryOffset + header.VolumeNameTableStart, size, p,
				count: (int)header.NumVolumes);
		}

		return new NefsHeader130(detectedSettings, header, entryTable, linkTable, nameTable, blockTable,
			volumeSizeTable, volumeNameStartTable, volumeNameTable);
	}

	public override Task<INefsHeader> ReadHeaderAsync(EndianBinaryReader reader, long primaryOffset, int? primarySize,
		long secondaryOffset, int? secondarySize, NefsProgress p)
	{
		throw new InvalidOperationException("NeFS version 1.3.0 does not support separated headers.");
	}
}
