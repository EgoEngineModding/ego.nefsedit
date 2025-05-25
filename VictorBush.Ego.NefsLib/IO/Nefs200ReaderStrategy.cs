// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

internal class Nefs200ReaderStrategy : Nefs160ReaderStrategy
{
	protected override NefsVersion Version => NefsVersion.Version200;

	protected override async Task<INefsHeader> ReadHeaderCoreAsync(EndianBinaryReader reader, long primaryOffset,
		long secondaryOffset, NefsWriterSettings detectedSettings, NefsProgress p)
	{
		// Calc weight of each task (8 parts + header + table of contents)
		var weight = 1.0f / 10.0f;

		Nefs160TocHeaderA header;
		using (p.BeginTask(weight, "Reading header"))
		{
			header = await ReadHeaderIntroV160Async(reader, primaryOffset, p.CancellationToken).ConfigureAwait(false);
		}

		Nefs200TocHeaderB toc;
		using (p.BeginTask(weight, "Reading header table of contents"))
		{
			toc = await ReadHeaderIntroTocVersion20Async(reader, primaryOffset + Nefs160TocHeaderA.ByteCount, p);
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

		Nefs200HeaderBlockTable blockTable;
		using (p.BeginTask(weight, "Reading block table"))
		{
			var size = Convert.ToInt32(toc.VolumeInfoTableStart - toc.BlockTableStart);
			blockTable = await ReadHeaderPart4Version20Async(reader, primaryOffset + toc.BlockTableStart, size, p);
		}

		NefsHeaderPart5 part5;
		using (p.BeginTask(weight, "Reading volume info table"))
		{
			var size = Convert.ToInt32(toc.NumVolumes * Nefs150TocVolumeInfo.ByteCount);
			part5 = await ReadHeaderPart5Async(reader, primaryOffset + toc.VolumeInfoTableStart, size, p);
		}

		Nefs160HeaderWriteableEntryTable part6;
		using (p.BeginTask(weight, "Reading entry writable table"))
		{
			var numEntries = entryTable.Entries.Count;
			part6 = await Read160HeaderPart6Async(reader, secondaryOffset + toc.WritableEntryTableStart, numEntries, p);
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
			var hashBlockSize = NefsWriter.DefaultHashBlockSize;
			hashDigestTable = await Read160HeaderPart8Async(reader, primaryOffset + toc.HashDigestTableStart, hashBlockSize, part5, p);
		}

		return new Nefs200Header(detectedSettings, header, toc, entryTable, sharedEntryInfoTable, part3, blockTable, part5, part6, writeableSharedEntryInfo, hashDigestTable);
	}

	/// <summary>
	/// Reads the header intro table of contents.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the header intro table of contents from the beginning of the stream.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The loaded header intro offsets data.</returns>
	private static async Task<Nefs200TocHeaderB> ReadHeaderIntroTocVersion20Async(EndianBinaryReader reader,
		long offset, NefsProgress p)
	{
		reader.BaseStream.Seek(offset, SeekOrigin.Begin);
		var header = await reader.ReadTocDataAsync<Nefs200TocHeaderB>(p.CancellationToken).ConfigureAwait(false);
		return header;
	}

	/// <summary>
	/// Reads 2.0.0 block table.
	/// </summary>
	/// <param name="reader">The reader to use.</param>
	/// <param name="offset">The offset to the table from the beginning of the stream.</param>
	/// <param name="size">The size of the table.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The table.</returns>
	internal static async Task<Nefs200HeaderBlockTable> ReadHeaderPart4Version20Async(EndianBinaryReader reader,
		long offset, int size, NefsProgress p)
	{
		var entries = await ReadTocEntriesAsync<Nefs200TocBlock>(reader, offset, size, p).ConfigureAwait(false);
		return new Nefs200HeaderBlockTable(entries);
	}
}
