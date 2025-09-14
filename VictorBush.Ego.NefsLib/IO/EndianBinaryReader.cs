// See LICENSE.txt for license information.

using System.Buffers;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using VictorBush.Ego.NefsLib.Header;

namespace VictorBush.Ego.NefsLib.IO;

public sealed class EndianBinaryReader(Stream stream, bool littleEndian, ResizableBuffer buffer) : IDisposable
{
	public Stream BaseStream { get; } = stream;

	public bool IsLittleEndian { get; } = littleEndian;

	public EndianBinaryReader(Stream stream) : this(stream, BitConverter.IsLittleEndian)
	{
	}

	public EndianBinaryReader(Stream stream, bool littleEndian) : this(stream, littleEndian,
		new ResizableBuffer(ArrayPool<byte>.Shared, 16))
	{
	}

	public string ReadNullTerminatedAscii()
	{
		var sb = new StringBuilder();
		do
		{
			var b = BaseStream.ReadByte();
			if (b == 0)
			{
				break;
			}

			sb.Append((char)b);
		} while (true);

		return sb.ToString();
	}

	public ushort ReadUInt16()
	{
		Span<byte> buff = stackalloc byte[sizeof(ushort)];
		BaseStream.ReadExactly(buff);
		return IsLittleEndian
			? BinaryPrimitives.ReadUInt16LittleEndian(buff)
			: BinaryPrimitives.ReadUInt16BigEndian(buff);
	}

	public uint ReadUInt32()
	{
		Span<byte> buff = stackalloc byte[sizeof(uint)];
		BaseStream.ReadExactly(buff);
		return IsLittleEndian
			? BinaryPrimitives.ReadUInt32LittleEndian(buff)
			: BinaryPrimitives.ReadUInt32BigEndian(buff);
	}

	public ulong ReadUInt64()
	{
		Span<byte> buff = stackalloc byte[sizeof(ulong)];
		BaseStream.ReadExactly(buff);
		return IsLittleEndian
			? BinaryPrimitives.ReadUInt64LittleEndian(buff)
			: BinaryPrimitives.ReadUInt64BigEndian(buff);
	}

	public async ValueTask<uint> ReadUInt32Async(CancellationToken cancellationToken = default)
	{
		await BaseStream.ReadExactlyAsync(buffer.Memory[..4], cancellationToken).ConfigureAwait(false);
		return IsLittleEndian
			? BinaryPrimitives.ReadUInt32LittleEndian(buffer.Memory.Span)
			: BinaryPrimitives.ReadUInt32BigEndian(buffer.Memory.Span);
	}

	public ValueTask<T> ReadTocDataAsync<T>(CancellationToken cancellationToken = default)
		where T : unmanaged, INefsTocData<T>
	{
		var size = T.ByteCount;
		buffer.EnsureLength(size);
		var buff = buffer.Memory[..size];

		var readTask = BaseStream.ReadExactlyAsync(buff, cancellationToken);
		return readTask.IsCompletedSuccessfully
			? new ValueTask<T>(ProcessData())
			: Awaited(readTask);

		async ValueTask<T> Awaited(ValueTask task)
		{
			await task.ConfigureAwait(false);
			return ProcessData();
		}
		T ProcessData()
		{
			var data = Unsafe.As<byte, T>(ref MemoryMarshal.GetReference(buffer.Memory.Span));
			if (IsLittleEndian != BitConverter.IsLittleEndian)
			{
				data.ReverseEndianness();
			}

			return data;
		}
	}

	public ValueTask ReadTocEntriesAsync<T>(Memory<T> entryBuffer, CancellationToken cancellationToken = default)
		where T : unmanaged, INefsTocData<T>
	{
		var readTask =
			BaseStream.ReadExactlyAsync(new CastMemoryManager<T, byte>(entryBuffer).Memory, cancellationToken);
		if (!readTask.IsCompletedSuccessfully)
		{
			return Awaited(readTask);
		}

		ProcessData();
		return default;

		async ValueTask Awaited(ValueTask task)
		{
			await task.ConfigureAwait(false);
			ProcessData();
		}
		void ProcessData()
		{
			if (IsLittleEndian == BitConverter.IsLittleEndian)
			{
				return;
			}

			var entrySpan = entryBuffer.Span;
			for (var i = 0; i < entryBuffer.Length; ++i)
			{
				entrySpan[i].ReverseEndianness();
			}
		}
	}

	public void Dispose()
	{
		buffer.Dispose();
	}
}

file sealed class CastMemoryManager<TFrom, TTo>(Memory<TFrom> from) : MemoryManager<TTo>
	where TFrom : unmanaged
	where TTo : unmanaged
{
	public override Span<TTo> GetSpan()
		=> MemoryMarshal.Cast<TFrom, TTo>(from.Span);

	protected override void Dispose(bool disposing) { }
	public override MemoryHandle Pin(int elementIndex = 0)
		=> throw new NotSupportedException();
	public override void Unpin()
		=> throw new NotSupportedException();
}
