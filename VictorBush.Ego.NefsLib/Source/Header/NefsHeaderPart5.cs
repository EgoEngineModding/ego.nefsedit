// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// Header part 5.
    /// </summary>
    public class NefsHeaderPart5
    {
        /// <summary>
        /// The size of header part 5.
        /// </summary>
        public const uint Size = 0x10;

        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart5"/> class.
        /// </summary>
        internal NefsHeaderPart5()
        {
        }

        /// <summary>
        /// The size of the archive file.
        /// </summary>
        [FileData]
        public UInt64Type ArchiveSize { get; } = new UInt64Type(0x00);

        /// <summary>
        /// Unknown data.
        /// </summary>
        [FileData]
        public UInt64Type UnknownData { get; } = new UInt64Type(0x08);
    }
}
