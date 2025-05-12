// See LICENSE.txt for license information.

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using VictorBush.Ego.NefsLib.Header;

namespace VictorBush.Ego.NefsLib.IO;

public class EndianBinaryWriter(Stream stream, bool littleEndian, ResizableBuffer buffer) : IDisposable
{
	private ResizableBuffer buffer = buffer;

	public Stream BaseStream { get; } = stream;

	public bool IsLittleEndian { get; } = littleEndian;

	public EndianBinaryWriter(Stream stream) : this(stream, BitConverter.IsLittleEndian)
	{
	}

	public EndianBinaryWriter(Stream stream, bool littleEndian) : this(stream, littleEndian,
		new ResizableBuffer(ArrayPool<byte>.Shared, 16))
	{
	}

	public ValueTask WriteAsync(uint value, CancellationToken cancellationToken = default)
	{
		if (IsLittleEndian)
		{
			BinaryPrimitives.WriteUInt32LittleEndian(this.buffer.Memory.Span, value);
		}
		else
		{
			BinaryPrimitives.WriteUInt32BigEndian(this.buffer.Memory.Span, value);
		}

		return BaseStream.WriteAsync(this.buffer.Memory[..4], cancellationToken);
	}

	public ValueTask WriteTocDataAsync<T>(T data, CancellationToken cancellationToken = default)
		where T : unmanaged, INefsTocData<T>
	{
		if (IsLittleEndian != BitConverter.IsLittleEndian)
		{
			data.ReverseEndianness();
		}

		var size = T.ByteCount;
		this.buffer.EnsureLength(size);
		var buff = this.buffer.Memory[..size];
		Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(buff.Span)) = data;
		return BaseStream.WriteAsync(buff, cancellationToken);
	}

	public void Dispose()
	{
		this.buffer.Dispose();
	}
}
