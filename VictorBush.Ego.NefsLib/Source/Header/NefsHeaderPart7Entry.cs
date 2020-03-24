// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// An entry in header part 7 for an item in an archive.
    /// </summary>
    public class NefsHeaderPart7Entry
    {
        /// <summary>
        /// The size of a part 7 entry.
        /// </summary>
        public const uint Size = 0x8;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart7Entry"/> class.
        /// </summary>
        internal NefsHeaderPart7Entry()
        {
        }

        /// <summary>
        /// Unknown data.
        /// </summary>
        [FileData]
        public UInt32Type Unknown0x00 { get; } = new UInt32Type(0x00);

        /// <summary>
        /// Unknown data.
        /// </summary>
        [FileData]
        public UInt32Type Unknown0x04 { get; } = new UInt32Type(0x04);
    }
}
