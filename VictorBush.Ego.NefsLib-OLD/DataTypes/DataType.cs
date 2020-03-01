using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsLib.DataTypes
{
    abstract class DataType
    {
        int _offset;

        public DataType(int offset)
        {
            _offset = offset;
        }

        /// <summary>
        /// Number of bytes.
        /// </summary>
        abstract public UInt32 Size { get; }
        
        /// <summary>
        /// Offset to the data from an arbitrary location, not necessarily from the
        /// beginning. Can be a relative offset.
        /// </summary>
        public int Offset
        {
            get { return _offset; }
        }

        abstract public byte[] GetBytes();
        abstract public void Read(Stream file, UInt32 baseOffset);

        /// <summary>
        /// Writes the stored data in little endian format.
        /// </summary>
        /// <param name="baseOffset">Base offset to write at.</param>
        /// <param name="file">The file stream to write to.</param>
        public void Write(Stream file, UInt32 baseOffset)
        {
            int actualOffset = (int)baseOffset + Offset;

            /*
             * Validate inputs
             */
            if (file == null)
            {
                throw new ArgumentNullException("File stream required to read data from.");
            }

            if (actualOffset < 0)
            {
                var ex = new InvalidOperationException("Invalid offset into file.");
                throw ex;
            }

            file.Seek(actualOffset, SeekOrigin.Begin);
            file.Write(GetBytes(), 0, (int)Size);
        }

        /// <summary>
        /// Reads the data from the specified filestream.
        /// </summary>
        /// <param name="file">The file stream to read from.</param>
        /// <param name="baseOffset">The base offset where to read in the file. The 
        ///     offset of the data type instance is added to the base offset.</param>
        /// <returns>Byte array containing the data read from the file.</returns>
        protected byte[] readFile(Stream file, UInt32 baseOffset)
        {
            int actualOffset = (int)baseOffset + Offset;
            int bytesRead = 0;

            /*
             * Validate inputs
             */
            if (file == null)
            {
                throw new ArgumentNullException("File stream required to read data from.");
            }

            if (actualOffset < 0
             || actualOffset >= file.Length)
            {
                var ex = new InvalidOperationException("Invalid offset into file.");
                throw ex;
            }

            /*
             * Read data from file
             */
            var temp = new byte[Size];
            file.Seek(actualOffset, SeekOrigin.Begin);
            bytesRead = file.Read(temp, 0, (int)Size);

            if (bytesRead != Size)
            {
                var ex = new Exception("Did not read the requested number of bytes.");
            }

            return (temp);
        }
    }
}
