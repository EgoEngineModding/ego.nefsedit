using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsLib.Utility
{
    class FilePathHelper
    {
        /// <summary>
        /// The directory where the library DLL resides.
        /// It is expected that this directory also contains
        /// packzip.exe and offzip.exe.
        /// </summary>
        public static string ExeDirectory
        {
            get
            {
                return Path.GetDirectoryName(typeof(FilePathHelper).Assembly.Location);
            }
        }

        /// <summary>
        /// Path to offzip.exe.
        /// </summary>
        public static string OffzipPath
        {
            get
            {
                return Path.Combine(ExeDirectory, "offzip.exe");
            }
        }

        /// <summary>
        /// Path to packzip.exe.
        /// </summary>
        public static string PackzipPath
        {
            get
            {
                return Path.Combine(ExeDirectory, "packzip.exe");
            }
        }

        /// <summary>
        /// Path to a temporary working directory.
        /// </summary>
        public static string TempDirectory
        {
            get
            {
                return Path.Combine(ExeDirectory, "temp");
            }
        }

        /// <summary>
        /// Hashes a string and returns the resulting hash as a string.
        /// </summary>
        /// <param name="stringToHash">The string to hash.</param>
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
