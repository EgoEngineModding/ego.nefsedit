// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.DataTypes;

/// <summary>
/// 8-bit unsigned integer.
/// </summary>
public class UInt8Type : DataType
{
	private const int UInt8TypeSize = 1;

	/// <summary>
	/// Initializes a new instance of the <see cref="UInt8Type"/> class.
	/// </summary>
	/// <param name="offset">See <see cref="DataType.Offset"/>.</param>
	public UInt8Type(int offset)
		: base(offset)
	{
	}

	/// <inheritdoc/>
	public override int Size => UInt8TypeSize;

	/// <summary>
	/// The current data value.
	/// </summary>
	public byte Value { get; set; }

	/// <inheritdoc/>
	public override byte[] GetBytes() => BitConverter.GetBytes(Value);

	/// <inheritdoc/>
	public override async Task ReadAsync(Stream file, long baseOffset, NefsProgress p)
	{
		var temp = await DoReadAsync(file, baseOffset, p);
		Value = temp[0];
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		/* Return value in hex */
		return "0x" + Value.ToString("X");
	}
}
