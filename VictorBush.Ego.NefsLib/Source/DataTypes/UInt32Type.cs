// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataTypes
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.Progress;

    /// <summary>
    /// 32-bit unsigned integer.
    /// </summary>
    public class UInt32Type : DataType
    {
        private const int UInt32TypeSize = 4;

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt32Type"/> class.
        /// </summary>
        /// <param name="offset">See <see cref="DataType.Offset"/>.</param>
        public UInt32Type(int offset)
            : base(offset)
        {
        }

        /// <inheritdoc/>
        public override int Size => UInt32TypeSize;

        /// <summary>
        /// The current data value.
        /// </summary>
        public UInt32 Value { get; set; }

        /// <inheritdoc/>
        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(this.Value);
        }

        /// <inheritdoc/>
        public override async Task ReadAsync(Stream file, long baseOffset, NefsProgress p)
        {
            var temp = await this.DoReadAsync(file, baseOffset, p);
            this.Value = BitConverter.ToUInt32(temp, 0);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            /* Return value in hex */
            return "0x" + this.Value.ToString("X");
        }
    }
}
