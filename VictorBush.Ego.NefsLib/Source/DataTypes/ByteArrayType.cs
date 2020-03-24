// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataTypes
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.Progress;
    using VictorBush.Ego.NefsLib.Utility;

    /// <summary>
    /// An array of bytes.
    /// </summary>
    public class ByteArrayType : DataType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayType"/> class.
        /// </summary>
        /// <param name="offset">See <see cref="DataType.Offset"/>.</param>
        /// <param name="size">The size of the array in bytes.</param>
        public ByteArrayType(int offset, uint size)
            : base(offset)
        {
            if (size == 0)
            {
                throw new ArgumentOutOfRangeException("ByteArrayType must have size greater than 0 bytes.");
            }

            this.Size = size;
            this.Value = new byte[size];
        }

        /// <summary>
        /// The size of the array in bytes.
        /// </summary>
        public override uint Size { get; }

        /// <summary>
        /// The current data value.
        /// </summary>
        public byte[] Value { get; set; }

        /// <inheritdoc/>
        public override byte[] GetBytes()
        {
            return this.Value;
        }

        /// <summary>
        /// Gets a 32-bit unsigned integer from the array.
        /// </summary>
        /// <param name="offset">
        /// The offset from the beginning of the array to get the integer from.
        /// </param>
        /// <returns>A <see cref="UInt32"/>.</returns>
        public UInt32 GetUInt32(UInt64 offset)
        {
            if (offset >= this.Size)
            {
                throw new ArgumentOutOfRangeException("Offset outside of byte array.");
            }

            if (this.Value.Length - (int)offset < 4)
            {
                throw new ArgumentOutOfRangeException("Offset must be at least 4 bytes from the end of the array.");
            }

            return BitConverter.ToUInt32(this.Value, (int)offset);
        }

        /// <inheritdoc/>
        public override async Task ReadAsync(Stream file, UInt64 baseOffset, NefsProgress p)
        {
            this.Value = await this.ReadFileAsync(file, baseOffset, p);
        }

        /// <inheritdoc/>
        public override String ToString()
        {
            return HexHelper.ByteArrayToString(this.Value);
        }
    }
}
