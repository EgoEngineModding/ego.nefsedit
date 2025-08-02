// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

internal class NefsWriterStrategy151 : NefsWriterStrategy<NefsHeader151>
{
	/// <inheritdoc />
	protected override async Task WriteHeaderAsync(EndianBinaryWriter writer, NefsHeader151 header, long primaryOffset,
		NefsProgress p)
	{
		// Calc weight of each task (5 parts + intro)
		const float weight = 1.0f / 6.0f;

		// Get intro
		var intro = header.Intro;

		var stream = writer.BaseStream;
		using (p.BeginTask(weight, "Writing header intro"))
		{
			var offset = primaryOffset;
			await WriteTocEntryAsync(writer, offset, header.Intro, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing entry table"))
		{
			var offset = primaryOffset + intro.EntryTableStart;
			await WriteTocTableAsync(writer, offset, header.EntryTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing shared entry info table"))
		{
			var offset = primaryOffset + intro.SharedEntryInfoTableStart;
			await WriteTocTableAsync(writer, offset, header.SharedEntryInfoTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing name table"))
		{
			var offset = primaryOffset + intro.NameTableStart;
			await WriteHeaderPart3Async(stream, offset, header.NameTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing block table"))
		{
			var offset = primaryOffset + intro.BlockTableStart;
			await WriteTocTableAsync(writer, offset, header.BlockTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing volume info table"))
		{
			var offset = primaryOffset + intro.VolumeInfoTableStart;
			await WriteTocTableAsync(writer, offset, header.VolumeInfoTable, p).ConfigureAwait(false);
		}
	}
}
