// See LICENSE.txt for license information.

using System.Security.Cryptography;
using VictorBush.Ego.NefsLib.Header.Version160;
using VictorBush.Ego.NefsLib.Header.Version200;
using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Source.Utility;

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
		using (p.BeginTask(weight, "Writing header intro"))
		{
			var offset = primaryOffset;
			await WriteTocEntryAsync(writer, offset, header.Intro, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing header intro table of contents"))
		{
			var offset = primaryOffset + NefsTocHeaderA160.ByteCount;
			await WriteTocEntryAsync(writer, offset, toc, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing header part 1"))
		{
			var offset = primaryOffset + toc.EntryTableStart;
			await WriteTocTableAsync(writer, offset, header.EntryTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing header part 2"))
		{
			var offset = primaryOffset + toc.SharedEntryInfoTableStart;
			await WriteTocTableAsync(writer, offset, header.SharedEntryInfoTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing header part 3"))
		{
			var offset = primaryOffset + toc.NameTableStart;
			await WriteHeaderPart3Async(stream, offset, header.Part3, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing header part 4"))
		{
			var offset = primaryOffset + toc.BlockTableStart;
			await WriteTocTableAsync(writer, offset, header.BlockTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing header part 5"))
		{
			var offset = primaryOffset + toc.VolumeInfoTableStart;
			await WriteTocEntryAsync(writer, offset, header.Part5.Data, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing header part 6"))
		{
			var offset = primaryOffset + toc.WritableEntryTableStart;
			await WriteTocTableAsync(writer, offset, header.WriteableEntryTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing header part 7"))
		{
			var offset = primaryOffset + toc.WritableSharedEntryInfoTableStart;
			await WriteTocTableAsync(writer, offset, header.WriteableSharedEntryInfoTable, p).ConfigureAwait(false);
		}

		using (p.BeginTask(weight, "Writing header part 8"))
		{
			var offset = primaryOffset + toc.HashDigestTableStart;
			await WriteTocTableAsync(writer, offset, header.HashDigestTable, p).ConfigureAwait(false);
		}

		await UpdateHashAsync(writer, header, primaryOffset, p).ConfigureAwait(false);
	}

	/// <summary>
	/// Gets the new expected hash and writes it to the header.
	/// </summary>
	/// <param name="writer">The writer with the stream containing the header.</param>
	/// <param name="header">The header.</param>
	/// <param name="primaryOffset">The offset to the header in the stream.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The async task.</returns>
	private async Task UpdateHashAsync(
		EndianBinaryWriter writer,
		NefsHeader200 header,
		long primaryOffset,
		NefsProgress p)
	{
		// The hash is of the entire header except for the expected hash
		var secondOffset = primaryOffset + 0x24;
		var headerSize = (int)header.Intro.TocSize;

		// Seek to beginning of header
		var stream = writer.BaseStream;
		stream.Seek(primaryOffset, SeekOrigin.Begin);

		// Read magic num
		var dataToHash = new byte[headerSize - 0x20];
		await stream.ReadExactlyAsync(dataToHash, 0, 4).ConfigureAwait(false);

		// Skip expected hash and read rest of header
		stream.Seek(secondOffset, SeekOrigin.Begin);
		await stream.ReadExactlyAsync(dataToHash, 4, headerSize - 0x24).ConfigureAwait(false);

		// Compute the new expected hash
		var hashOut = SHA256.HashData(dataToHash);

		// Write the intro with the expected hash
		header.Hash = new Sha256Hash(hashOut);
		await WriteTocEntryAsync(writer, primaryOffset, header.Intro, p).ConfigureAwait(false);
	}
}
