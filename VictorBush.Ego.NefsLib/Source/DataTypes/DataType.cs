// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataTypes
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.Progress;

    /// <summary>
    /// A base for different data types that supports reading in data from a file.
    /// </summary>
    public abstract class DataType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DataType"/> class.
        /// </summary>
        /// <param name="offset">See <see cref="Offset"/>.</param>
        protected DataType(int offset)
        {
            this.Offset = offset;
        }

        /// <summary>
        /// Offset to the data from an arbitrary location, not necessarily from the beginning. Can
        /// be a relative offset.
        /// </summary>
        public int Offset { get; private set; }

        /// <summary>
        /// Number of bytes.
        /// </summary>
        public abstract uint Size { get; }

        /// <summary>
        /// Gets the data as an array of bytes.
        /// </summary>
        /// <returns>Array of bytes.</returns>
        public abstract byte[] GetBytes();

        /// <summary>
        /// Reads in data from the file.
        /// </summary>
        /// <param name="file">The file stream to read from.</param>
        /// <param name="baseOffset">
        /// The base offset where to read in the file. The offset of the data type instance is added
        /// to the base offset.
        /// </param>
        /// <param name="p">Progress info.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public abstract Task ReadAsync(Stream file, UInt64 baseOffset, NefsProgress p);

        /// <summary>
        /// Writes the stored data in little endian format.
        /// </summary>
        /// <param name="file">The file stream to read from.</param>
        public void Write(Stream file) => this.Write(file);

        /// <summary>
        /// Writes the stored data in little endian format.
        /// </summary>
        /// <param name="file">The file stream to read from.</param>
        /// <param name="baseOffset">Base offset to write at.</param>
        /// <param name="p">Progress info.</param>
        /// <returns>The async task.</returns>
        public async Task WriteAsync(Stream file, UInt64 baseOffset, NefsProgress p)
        {
            var actualOffset = (long)baseOffset + this.Offset;

            // Validate inputs
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
            await file.WriteAsync(this.GetBytes(), 0, (int)this.Size, p.CancellationToken);
        }

        /// <summary>
        /// Reads the data from the specified filestream.
        /// </summary>
        /// <param name="file">The file stream to read from.</param>
        /// <param name="baseOffset">
        /// The base offset where to read in the file. The offset of the data type instance is added
        /// to the base offset.
        /// </param>
        /// <param name="p">Progress info.</param>
        /// <returns>Byte array containing the data read from the file.</returns>
        protected async Task<byte[]> ReadFileAsync(Stream file, UInt64 baseOffset, NefsProgress p)
        {
            var actualOffset = (long)baseOffset + this.Offset;

            // Validate inputs
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

            // Read data from file
            var temp = new byte[this.Size];
            file.Seek(actualOffset, SeekOrigin.Begin);
            var bytesRead = await file.ReadAsync(temp, 0, (int)this.Size, p.CancellationToken);

            if (bytesRead != this.Size)
            {
                throw new Exception("Did not read the requested number of bytes.");
            }

            return temp;
        }
    }
}
