// See LICENSE.txt for license information.

using System.Runtime.InteropServices;
using System.Text;

namespace VictorBush.Ego.NefsLib.Utility;

/// <summary>
/// Some string utilities.
/// </summary>
public static class StringHelper
{
	/// <summary>
	/// Prints a byte array to a string in hex format.
	/// </summary>
	/// <param name="bytes">The bytes to print.</param>
	/// <returns>The string.</returns>
	public static string ByteArrayToString(ReadOnlySpan<byte> bytes)
	{
		if (bytes == null)
		{
			return "";
		}

		var sb = new StringBuilder();
		foreach (var b in bytes)
		{
			sb.Append("0x" + b.ToString("X") + ", ");
		}

		return sb.ToString();
	}

	/// <summary>
	/// Takes a string representation of a hexademical value and converts it to a byte array.
	/// </summary>
	/// <param name="hex">The hex string.</param>
	/// <returns>The byte array.</returns>
	public static byte[] FromHexString(string hex)
	{
		byte[] raw = new byte[hex.Length / 2];
		for (int i = 0; i < raw.Length; i++)
		{
			raw[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
		}

		return raw;
	}

	/// <summary>
	/// Takes an integer and converts it to a string representation in hexadecimal format.
	/// </summary>
	/// <param name="value">The value to convert.</param>
	/// <param name="prefix">Whether to prefix with '0x'.</param>
	/// <returns>The hex string.</returns>
	public static string ToHexString(this uint value, bool prefix = true)
	{
		if (prefix)
		{
			return string.Format("0x{0:X}", value);
		}
		else
		{
			return string.Format("{0:X}", value);
		}
	}

	/// <summary>
	/// Tries to read a null terminated ASCII string. If no null-terminator is found, an empty string is returned.
	/// </summary>
	/// <param name="bytes">The input bytes.</param>
	/// <returns>The string.</returns>
	public static string TryReadNullTerminatedAscii(ReadOnlySpan<byte> bytes)
	{
		// Find the next null terminator
		var nullOffset = bytes.IndexOf((byte)0);
		return nullOffset == -1 ?
			// No null terminator found
			"" :
			// Get the string
			Encoding.ASCII.GetString(bytes[..nullOffset]);
	}
}
