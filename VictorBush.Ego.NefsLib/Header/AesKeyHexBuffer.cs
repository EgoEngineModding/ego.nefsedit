// See LICENSE.txt for license information.

using System.Runtime.CompilerServices;
using System.Text;

namespace VictorBush.Ego.NefsLib.Header;

[InlineArray(64)]
public struct AesKeyHexBuffer
{
	private byte element;

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
