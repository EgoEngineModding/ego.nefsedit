// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Utility
{
    using System;

    /// <summary>
    /// Some hex string utilities.
    /// </summary>
    public static class HexHelper
    {
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
        public static string ToHexString(this UInt32 value, bool prefix = true)
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
    }
}
