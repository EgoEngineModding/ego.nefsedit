// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Utility
{
    using System.Security.Cryptography;
    using System.Text;

    /// <summary>
    /// Hashing utilities.
    /// </summary>
    internal static class HashHelper
    {
        /// <summary>
        /// Hashes a string and returns the resulting hash as a string.
        /// </summary>
        /// <param name="stringToHash">The string to hash.</param>
        /// <returns>The hashed string.</returns>
        public static string HashStringMD5(string stringToHash)
        {
            var hash = MD5.Create();
            var bytesToHash = Encoding.ASCII.GetBytes(stringToHash);

            var hashBytes = hash.ComputeHash(bytesToHash);
            var hashStringBuilder = new StringBuilder();

            foreach (var b in hashBytes)
            {
                hashStringBuilder.Append(b.ToString("x2"));
            }

            return hashStringBuilder.ToString();
        }
    }
}
