// See LICENSE.txt for license information.

using System.Runtime.CompilerServices;
using System.Text;

namespace VictorBush.Ego.NefsLib.Header;

[InlineArray(64)]
public struct AesKeyBuffer
{
	private byte element;

	/// <summary>
	/// Gets the AES-256 key for this header.
	/// </summary>
	/// <returns>A byte array with the AES key.</returns>
	public byte[] GetAesKey()
	{
		var asciiKey = Encoding.ASCII.GetString(this);
		return Convert.FromHexString(asciiKey);
	}
}
