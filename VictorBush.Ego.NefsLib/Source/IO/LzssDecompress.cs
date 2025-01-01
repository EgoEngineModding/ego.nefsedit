// See LICENSE.txt for license information.

using System.Buffers;
using System.Diagnostics;

namespace VictorBush.Ego.NefsLib.IO;

public class LzssDecompress
{
	private const byte BufferDefaultValue = 32;
	private const int BufferHeadStart = 4078;
	private const int RingBufferSize = 4113;

	private readonly byte[] ringBuffer;
	private int ringBufferHead;
	private bool history;
	private byte historyLsb;
	private int historyLength;
	private int historyOffset;
	private int controlFlags;

	public LzssDecompress()
	{
		this.ringBuffer = new byte[RingBufferSize];
		Reset();
	}

	public void Reset()
	{
		Array.Fill(this.ringBuffer, BufferDefaultValue);
		this.controlFlags = 0;
		this.historyLsb = 0;
		this.ringBufferHead = BufferHeadStart;
		this.history = false;
		this.historyLength = 0;
		this.historyOffset = 0;
	}

	public (int Consumed, int Written) Decompress(Span<byte> input, Span<byte> output)
	{
		var i = 0;
		var io = 0;
		while (true)
		{
			while (this.historyLength > 0)
			{
				if (io >= output.Length)
				{
					break;
				}

				var v14 = this.ringBuffer[this.historyOffset];
				--this.historyLength;
				this.historyOffset = (this.historyOffset + 1) & 0xFFF;
				output[io++] = v14;
				this.ringBuffer[this.ringBufferHead] = v14;
				this.ringBufferHead = (this.ringBufferHead + 1) & 0xFFF;
			}

			if (i >= input.Length || io >= output.Length)
			{
				break;
			}

			var b = input[i++];
			if (this.controlFlags > 255)
			{
				if ((this.controlFlags & 1) != 0)
				{
					output[io++] = b;
					this.controlFlags >>= 1;
					this.ringBuffer[this.ringBufferHead] = b;
					this.ringBufferHead = (this.ringBufferHead + 1) & 0xFFF;
				}
				else if (this.history)
				{
					this.controlFlags >>= 1;
					this.history = false;
					this.historyOffset = this.historyLsb | (16 * (b & 0xF0));
					this.historyLength = (b & 0xF) + 3;
				}
				else
				{
					this.historyLsb = b;
					this.history = true;
				}
			}
			else
			{
				this.controlFlags = b | 0xFF00;
			}
		}

		return (i, io);
	}

	public async Task DecompressAsync(Stream source, Stream destination, CancellationToken cancellationToken)
	{
		var buffer = ArrayPool<byte>.Shared.Rent(8192);
		var outBuffer = ArrayPool<byte>.Shared.Rent(8192);
		try
		{
			int bytesRead;
			while ((bytesRead =
				       await source.ReadAsync(new Memory<byte>(buffer), cancellationToken).ConfigureAwait(false)) != 0)
			{
				var consumed = 0;
				while (consumed < bytesRead)
				{
					// loop in case output buffer wasn't large enough
					var pos = Decompress(buffer.AsSpan(consumed..bytesRead), outBuffer.AsSpan());
					await destination.WriteAsync(new ReadOnlyMemory<byte>(outBuffer, 0, pos.Written), cancellationToken)
						.ConfigureAwait(false);
					consumed += pos.Consumed;
				}
			}

			if (this.historyLength > 0)
			{
				Debug.Fail("LZSS decompression failed.");
				throw new IOException("Failed to decompress data");
			}
		}
		finally
		{
			ArrayPool<byte>.Shared.Return(buffer);
			ArrayPool<byte>.Shared.Return(outBuffer);
		}
	}
}
