// See LICENSE.txt for license information.

namespace VictorBush.Ego.NefsLib.Header
{
    using VictorBush.Ego.NefsLib.DataTypes;

    /// <summary>
    /// Not sure what this data is. Between header part 7 and the start of the first
    /// item's compressed data, there is an unknown chunk of data. It straddles the "header size"
    /// offset that is used when performing a hash check of the header. So I'm not sure how much
    /// of this is actually part of the header or not. Needs investigated.
    /// </summary>
    public class NefsHeaderPart8
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NefsHeaderPart8"/> class.
        /// </summary>
        internal NefsHeaderPart8()
        {
        }

        /// <summary>
        /// All the data in part 8.
        /// </summary>
        [FileData]
        public ByteArrayType AllTheData { get; internal set; }
    }
}
