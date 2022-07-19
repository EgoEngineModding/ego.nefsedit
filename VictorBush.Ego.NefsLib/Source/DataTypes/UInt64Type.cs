// See LICENSE.txt for license information.

using VictorBush.Ego.NefsLib.Progress;

namespace VictorBush.Ego.NefsLib.DataTypes;

/// <summary>
/// 64-bit unsigned integer.
/// </summary>
public class UInt64Type : DataType
{
	private const int UInt64Size = 8;

	/// <summary>
	/// Initializes a new instance of the <see cref="UInt64Type"/> class.
	/// </summary>
	/// <param name="offset">See <see cref="DataType.Offset"/>.</param>
	public UInt64Type(int offset)
		: base(offset)
	{
	}

	/// <inheritdoc/>
	public override int Size => UInt64Size;

	/// <summary>
	/// The current data value.
	/// </summary>
	public ulong Value { get; set; }

	/// <inheritdoc/>
	public override byte[] GetBytes() => BitConverter.GetBytes(Value);

	/// <inheritdoc/>
	public override async Task ReadAsync(Stream file, long baseOffset, NefsProgress p)
	{
		var temp = await DoReadAsync(file, baseOffset, p);
		Value = BitConverter.ToUInt64(temp, 0);
	}

	/// <inheritdoc/>
	public override string ToString()
	{
		return "0x" + Value.ToString("X");
	}
}
