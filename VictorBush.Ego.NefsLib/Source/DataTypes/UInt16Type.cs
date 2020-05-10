// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.DataTypes
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using VictorBush.Ego.NefsLib.Progress;

    /// <summary>
    /// 16-bit unsigned integer.
    /// </summary>
    public class UInt16Type : DataType
    {
        private const int UInt16TypeSize = 2;

        /// <summary>
        /// Initializes a new instance of the <see cref="UInt16Type"/> class.
        /// </summary>
        /// <param name="offset">See <see cref="DataType.Offset"/>.</param>
        public UInt16Type(int offset)
            : base(offset)
        {
        }

        /// <inheritdoc/>
        public override uint Size => UInt16TypeSize;

        /// <summary>
        /// The current data value.
        /// </summary>
        public UInt16 Value { get; set; }

        /// <inheritdoc/>
        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(this.Value);
        }

        /// <inheritdoc/>
        public override async Task ReadAsync(Stream file, UInt64 baseOffset, NefsProgress p)
        {
            var temp = await this.ReadFileAsync(file, baseOffset, p);
            this.Value = BitConverter.ToUInt16(temp, 0);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            /* Return value in hex */
            return "0x" + this.Value.ToString("X");
        }
    }
}
