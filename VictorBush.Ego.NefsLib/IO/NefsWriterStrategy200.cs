// See LICENSE.txt for license information.

using System.Security.Cryptography;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.IO;

internal class NefsWriterStrategy200 : NefsWriterStrategy160Base<NefsHeader200>
{
	/// <inheritdoc />
	protected override async Task WriteHeaderAsync(EndianBinaryWriter writer, NefsHeader200 header, long primaryOffset,
		NefsProgress p)
	{
		// Calc weight of each task (8 parts + intro + table of contents)
		const float weight = 1.0f / 10.0f;

		// Get table of contents
		var toc = header.TableOfContents;

		var stream = writer.BaseStream;
		using (p.BeginTask(weight, "Writing header"))
		{
			var offset = primaryOffset;
			await WriteTocEntryAsync(writer, offset, header.Intro, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing header table of contents"))
		{
			var offset = primaryOffset + NefsTocHeaderA160.ByteCount;
			await WriteTocEntryAsync(writer, offset, toc, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing entry table"))
		{
			var offset = primaryOffset + toc.EntryTableStart;
			await WriteTocTableAsync(writer, offset, header.EntryTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing shared entry info table"))
		{
			var offset = primaryOffset + toc.SharedEntryInfoTableStart;
			await WriteTocTableAsync(writer, offset, header.SharedEntryInfoTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing name table"))
		{
			var offset = primaryOffset + toc.NameTableStart;
			await WriteHeaderPart3Async(stream, offset, header.NameTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing block table"))
		{
			var offset = primaryOffset + toc.BlockTableStart;
			await WriteTocTableAsync(writer, offset, header.BlockTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing volume info table"))
		{
			var offset = primaryOffset + toc.VolumeInfoTableStart;
			await WriteTocTableAsync(writer, offset, header.VolumeInfoTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing writeable entry table"))
		{
			var offset = primaryOffset + toc.WritableEntryTableStart;
			await WriteTocTableAsync(writer, offset, header.WriteableEntryTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing writeable shared entry info table"))
		{
			var offset = primaryOffset + toc.WritableSharedEntryInfoTableStart;
			await WriteTocTableAsync(writer, offset, header.WriteableSharedEntryInfoTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing hash digest table"))
		{
			var offset = primaryOffset + toc.HashDigestTableStart;
			await WriteTocTableAsync(writer, offset, header.HashDigestTable, p).ConfigureAwait(false);
		}

		// Write the intro with the updated hash
		header.Hash = await ComputeHashAsync(writer, primaryOffset, header.Intro.TocSize, p).ConfigureAwait(false);
		await WriteTocEntryAsync(writer, primaryOffset, header.Intro, p).ConfigureAwait(false);
	}
}
