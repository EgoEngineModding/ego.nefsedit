// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.DataTypes;

/// <summary>
/// 16-bit unsigned integer.
/// </summary>
public class UInt16Type : DataType
{
	private const int UInt16TypeSize = 2;

	/// <summary>
	/// Initializes a new instance of the <see cref="UInt16Type"/> class.
	/// </summary>
	/// <param name="offset">See <see cref="DataType.Offset"/>.</param>
	public UInt16Type(int offset)
		: base(offset)
	{
	}

	/// <inheritdoc/>
	public override int Size => UInt16TypeSize;

	/// <summary>
	/// The current data value.
	/// </summary>
	public ushort Value { get; set; }

	/// <inheritdoc/>
	public override byte[] GetBytes() => BitConverter.GetBytes(Value);

	/// <inheritdoc/>
	public override async Task ReadAsync(Stream file, long baseOffset, NefsProgress p)
	{
		var temp = await DoReadAsync(file, baseOffset, p);
		Value = BitConverter.ToUInt16(temp, 0);
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		/* Return value in hex */
		return "0x" + Value.ToString("X");
	}
}
