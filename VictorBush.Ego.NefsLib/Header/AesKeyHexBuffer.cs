// See LICENSE.txt for license information.

using System.Runtime.CompilerServices;
using System.Text;

namespace VictorBush.Ego.NefsLib.Header;

[InlineArray(Size)]
public struct AesKeyHexBuffer
{
	/// <summary>
	/// The size of the buffer in bytes.
	/// </summary>
	public const int Size = 0x40;

	private byte element;

	/// <summary>
	/// Initializes a new instance of the <see cref="AesKeyHexBuffer"/> struct.
	/// </summary>
	/// <param name="hexString">A string of hex data.</param>
	public AesKeyHexBuffer(string hexString)
	{
		const string msg = "The hex string must have exactly 64 characters representing hex digits.";
		if (hexString.Length != Size)
		{
			throw new ArgumentException(msg);
		}

		var bytesEncoded = Encoding.ASCII.GetBytes(hexString, this);
		if (hexString.Length != bytesEncoded)
		{
			throw new ArgumentException(msg);
		}
	}

	/// <summary>
	/// Gets the AES-256 key for this header.
	/// </summary>
	/// <returns>A byte array with the AES key.</returns>
	public byte[] GetAesKey()
	{
		return Convert.FromHexString(ToString());
	}

	public override string ToString()
	{
		return Encoding.ASCII.GetString(this);
	}
}
