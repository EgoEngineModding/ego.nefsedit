// See LICENSE.txt for license information.

using System.Text;
using VictorBush.Ego.NefsLib.Header;
using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.IO;

internal abstract class NefsWriterStrategy
{
	private static readonly Dictionary<NefsVersion, NefsWriterStrategy> Instances = new();

	public static NefsWriterStrategy Get(NefsVersion version)
	{
		if (Instances.TryGetValue(version, out var inst))
		{
			return inst;
		}

		inst = version switch
		{
			NefsVersion.Version200 => new NefsWriterStrategy200(),
			_ => throw new NotImplementedException($"Support for {version.ToPrettyString()} is not implemented.")
		};

		Instances.Add(version, inst);
		return inst;
	}

	public abstract Task WriteHeaderAsync(
		EndianBinaryWriter writer,
		INefsHeader header,
		long primaryOffset,
		NefsProgress p);

	internal async Task WriteTocEntryAsync<T>(EndianBinaryWriter writer, long offset, T entry, NefsProgress p)
		where T : unmanaged, INefsTocData<T>
	{
		using var t = p.BeginTask(1.0f);
		writer.BaseStream.Seek(offset, SeekOrigin.Begin);
		await writer.WriteTocDataAsync(entry, p.CancellationToken).ConfigureAwait(false);
	}

	internal async Task WriteTocTableAsync<T>(EndianBinaryWriter writer, long offset, INefsTocTable<T> table,
		NefsProgress p)
		where T :  unmanaged, INefsTocData<T>
	{
		if (table.Entries.Count == 0)
		{
			return;
		}

		var weight = 1.0f / table.Entries.Count;
		writer.BaseStream.Seek(offset, SeekOrigin.Begin);
		foreach (var entry in table.Entries)
		{
			using (p.BeginSubTask(weight))
			{
				await writer.WriteTocDataAsync(entry, p.CancellationToken).ConfigureAwait(false);
			}
		}
	}

	/// <summary>
	/// Writes the header part to an output stream.
	/// </summary>
	/// <param name="stream">The stream to write to.</param>
	/// <param name="offset">The absolute offset in the stream to write at.</param>
	/// <param name="nameTable">The data to write.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>An async task.</returns>
	internal async Task WriteHeaderPart3Async(Stream stream, long offset, NefsHeaderNameTable nameTable, NefsProgress p)
	{
		stream.Seek(offset, SeekOrigin.Begin);

		foreach (var entry in nameTable.FileNames)
		{
			var fileNameBytes = Encoding.ASCII.GetBytes(entry);
			await stream.WriteAsync(fileNameBytes, p.CancellationToken);

			// Write null terminator
			await stream.WriteAsync(new byte[] { 0 }, p.CancellationToken);
		}
	}
}

/// <inheritdoc />
internal abstract class NefsWriterStrategy<T> : NefsWriterStrategy
	where T : INefsHeader
{
	/// <inheritdoc />
	public override Task WriteHeaderAsync(EndianBinaryWriter writer, INefsHeader header, long primaryOffset, NefsProgress p)
	{
		return WriteHeaderAsync(writer, header.As<T>(), primaryOffset, p);
	}

	protected abstract Task WriteHeaderAsync(EndianBinaryWriter writer, T header, long primaryOffset, NefsProgress p);
}
