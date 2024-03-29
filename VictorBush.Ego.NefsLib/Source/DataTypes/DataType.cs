﻿// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.DataTypes;

/// <summary>
/// A base for different data types that supports reading in data from a data stream.
/// </summary>
public abstract class DataType
{
	protected DataType(int offset)
	{
		Offset = offset;
	}

	/// <summary>
	/// Offset to the data from an arbitrary location, not necessarily from the beginning. Can be a relative offset.
	/// </summary>
	public int Offset { get; private set; }

	/// <summary>
	/// Number of bytes.
	/// </summary>
	public abstract int Size { get; }

	/// <summary>
	/// Gets the data as an array of bytes.
	/// </summary>
	/// <returns>Array of bytes.</returns>
	public abstract byte[] GetBytes();

	/// <summary>
	/// Reads in data from a sream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="baseOffset">
	/// The base offset where to read in the stream. The offset of the data type instance is added to the base offset.
	/// </param>
	/// <param name="p">Progress info.</param>
	/// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
	public abstract Task ReadAsync(Stream stream, long baseOffset, NefsProgress p);

	/// <summary>
	/// Writes the stored data in little endian format.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	public void Write(Stream stream) => Write(stream);

	/// <summary>
	/// Writes the stored data in little endian format.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="baseOffset">Base offset to write at.</param>
	/// <param name="p">Progress info.</param>
	/// <returns>The async task.</returns>
	public async Task WriteAsync(Stream stream, long baseOffset, NefsProgress p)
	{
		var actualOffset = baseOffset + Offset;

		// Validate inputs
		if (stream == null)
		{
			throw new ArgumentNullException("Stream required to read data from.");
		}

		if (actualOffset < 0)
		{
			throw new InvalidOperationException("Invalid offset into stream.");
		}

		stream.Seek(actualOffset, SeekOrigin.Begin);
		await stream.WriteAsync(GetBytes(), 0, Size, p.CancellationToken);
	}

	/// <summary>
	/// Reads the data from the specified stream.
	/// </summary>
	/// <param name="stream">The stream to read from.</param>
	/// <param name="baseOffset">
	/// The base offset where to read in the stream. The offset of the data type instance is added to the base offset.
	/// </param>
	/// <param name="p">Progress info.</param>
	/// <returns>Byte array containing the data read from the stream.</returns>
	protected async Task<byte[]> DoReadAsync(Stream stream, long baseOffset, NefsProgress p)
	{
		var actualOffset = baseOffset + Offset;

		// Validate inputs
		if (stream == null)
		{
			throw new ArgumentNullException("Stream required to read data from.");
		}

		if (actualOffset < 0
		 || actualOffset >= stream.Length)
		{
			var ex = new InvalidOperationException("Invalid offset into stream.");
			throw ex;
		}

		// Read data from stream
		var temp = new byte[Size];
		stream.Seek(actualOffset, SeekOrigin.Begin);
		var bytesRead = await stream.ReadAsync(temp, 0, Size, p.CancellationToken);

		if (bytesRead != Size)
		{
			throw new Exception("Did not read the requested number of bytes.");
		}

		return temp;
	}
}
