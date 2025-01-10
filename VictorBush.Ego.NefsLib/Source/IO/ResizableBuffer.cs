// See LICENSE.txt for license information.

using System.Buffers;

namespace VictorBush.Ego.NefsLib.IO;

public class ResizableBuffer : IDisposable
{
	private readonly ArrayPool<byte> arrayPool;
	private byte[] buffer;

	public Memory<byte> Memory { get; private set; }

	public ResizableBuffer() : this(ArrayPool<byte>.Shared, 16)
	{
	}

	public ResizableBuffer(ArrayPool<byte> arrayPool, int startingMinimumLength)
	{
		this.arrayPool = arrayPool;
		this.buffer = arrayPool.Rent(startingMinimumLength);
		Memory = this.buffer.AsMemory(0, startingMinimumLength);
	}

	public void EnsureLength(int length)
	{
		if (length <= this.buffer.Length)
		{
			return;
		}

		this.arrayPool.Return(this.buffer);
		this.buffer = this.arrayPool.Rent(length);
		Memory = this.buffer.AsMemory(0, length);
	}

	public void Dispose()
	{
		this.arrayPool.Return(this.buffer);
	}
}
