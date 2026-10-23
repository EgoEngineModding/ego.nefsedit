// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Header.Version150;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;

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
		using (p.BeginTask(weight, "Writing header"))
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

		// The header checksum is a CRC32 over the decoded table of contents, excluding the checksum
		// field itself and the final two bytes of the TOC. It is validated by the game on mount.
		if (header.WriterSettings.IsXorEncoded)
		{
			var checksum = await ComputeChecksumAsync(stream, primaryOffset, intro.TocSize, p).ConfigureAwait(false);
			stream.Seek(primaryOffset + 0x38, SeekOrigin.Begin);
			await writer.WriteAsync(checksum, p.CancellationToken).ConfigureAwait(false);
		}
	}

	private static async Task<uint> ComputeChecksumAsync(Stream stream, long offset, uint tocSize, NefsProgress p)
	{
		var buffer = new byte[tocSize];
		stream.Seek(offset, SeekOrigin.Begin);

		var totalRead = 0;
		while (totalRead < buffer.Length)
		{
			var read = await stream.ReadAsync(buffer.AsMemory(totalRead), p.CancellationToken).ConfigureAwait(false);
			if (read == 0)
			{
				throw new EndOfStreamException("Unexpected end of header while computing checksum.");
			}

			totalRead += read;
		}

		var checksum = ChecksumCrc32.ComputeCrc32(0, buffer.AsSpan(0, 0x38));
		return ChecksumCrc32.ComputeCrc32(checksum, buffer.AsSpan(0x3C, (int)(tocSize - 0x3E)));
	}
}
