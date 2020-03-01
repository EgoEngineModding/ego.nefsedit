using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Header
{
    /// <summary>
    /// Purpose of part 6 entries unknown.
    /// </summary>
    public class NefsHeaderPt6Entry
    {
        public const UInt32 SIZE = 8;

        NefsHeaderPt6 _parent;
        UInt32 _relOffset;

        [FileData]
        UInt32Type off_0x00 = new UInt32Type(0x00);
        [FileData]
        UInt32Type off_0x04 = new UInt32Type(0x04);

        /// <summary>
        /// Loads an entry from a file stream.
        /// </summary>
        /// <param name="file">The file stream to load from.</param>
        /// <param name="parent">The part 6 obj this entry belongs to.</param>
        /// <param name="relOffset">The relative offset into part 5 where the entry begins.</param>
        public NefsHeaderPt6Entry(Stream file, NefsHeaderPt6 parent, UInt32 relOffset)
        {
            _parent = parent;
            _relOffset = relOffset;

            /* Read data from file as defined by [FileData] fields. */
            FileData.ReadData(file, _parent.Offset + relOffset, this);
        }

        public UInt32 Off_0x00
        {
            get { return off_0x00.Value; }
        }

        public UInt32 Off_0x04
        {
            get { return off_0x04.Value; }
        }

        /// <summary>
        /// Writes this entry to a file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        public void Write(FileStream file)
        {
            FileData.WriteData(file, _parent.Offset + _relOffset, this);
        }
    }
}
