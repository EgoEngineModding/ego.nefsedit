// See LICENSE.txt for license information.

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VictorBush.Ego.NefsLib.Header;

namespace VictorBush.Ego.NefsLib.IO;

public class EndianBinaryReader(Stream stream, bool littleEndian, ResizableBuffer buffer) : IDisposable
{
	private ResizableBuffer buffer = buffer;

	public Stream BaseStream { get; } = stream;

	public bool IsLittleEndian { get; } = littleEndian;

	public EndianBinaryReader(Stream stream) : this(stream, BitConverter.IsLittleEndian)
	{
	}

	public EndianBinaryReader(Stream stream, bool littleEndian) : this(stream, littleEndian,
		new ResizableBuffer(ArrayPool<byte>.Shared, 16))
	{
	}

	public async ValueTask<uint> ReadUInt32Async(CancellationToken cancellationToken = default)
	{
		await BaseStream.ReadExactlyAsync(this.buffer.Memory[..4], cancellationToken).ConfigureAwait(false);
		return IsLittleEndian
			? BinaryPrimitives.ReadUInt32LittleEndian(this.buffer.Memory.Span)
			: BinaryPrimitives.ReadUInt32BigEndian(this.buffer.Memory.Span);
	}

	public async ValueTask<T> ReadTocDataAsync<T>(CancellationToken cancellationToken = default)
		where T : unmanaged, INefsTocData<T>
	{
		var size = T.ByteCount;
		this.buffer.EnsureLength(size);
		var buff = this.buffer.Memory[..size];

		await BaseStream.ReadExactlyAsync(buff, cancellationToken).ConfigureAwait(false);
		var data = Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(this.buffer.Memory.Span));
		if (IsLittleEndian != BitConverter.IsLittleEndian)
		{
			data.ReverseEndianness();
		}

		return data;
	}

	public void Dispose()
	{
		this.buffer.Dispose();
	}
}
