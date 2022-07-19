// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Progress;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.DataTypes;

/// <summary>
/// An array of bytes.
/// </summary>
public sealed class ByteArrayType : DataType
{
	private byte[] value;

	/// <summary>
	/// Initializes a new instance of the <see cref="ByteArrayType"/> class.
	/// </summary>
	/// <param name="offset">See <see cref="DataType.Offset"/>.</param>
	/// <param name="size">The size of the array in bytes.</param>
	public ByteArrayType(int offset, int size)
		: base(offset)
	{
		if (size == 0)
		{
			throw new ArgumentOutOfRangeException("ByteArrayType must have size greater than 0 bytes.");
		}

		Size = size;
		this.value = new byte[size];
	}

	/// <summary>
	/// The size of the array in bytes.
	/// </summary>
	public override int Size { get; }

	/// <summary>
	/// The current data value.
	/// </summary>
	public byte[] Value
	{
		get => this.value;
		set
		{
			if (value.Length != Size)
			{
				throw new ArgumentException($"Byte array had length of {value.Length}, but expected {Size}. Value of {nameof(ByteArrayType)} must not change length.");
			}

			this.value = value;
		}
	}

	/// <inheritdoc/>
	public override byte[] GetBytes() => Value;

	/// <summary>
	/// Gets a 32-bit unsigned integer from the array.
	/// </summary>
	/// <param name="offset">The offset from the beginning of the array to get the integer from.</param>
	public uint GetUInt32(long offset)
	{
		if (offset >= Size)
		{
			throw new ArgumentOutOfRangeException("Offset outside of byte array.");
		}

		if (Value.Length - (int)offset < 4)
		{
			throw new ArgumentOutOfRangeException("Offset must be at least 4 bytes from the end of the array.");
		}

		return BitConverter.ToUInt32(Value, (int)offset);
	}

	/// <inheritdoc/>
	public override async Task ReadAsync(Stream file, long baseOffset, NefsProgress p)
	{
		Value = await DoReadAsync(file, baseOffset, p);
	}

	/// <inheritdoc/>
	public override string ToString() => StringHelper.ByteArrayToString(Value);
}
