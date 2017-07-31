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
    /// Purpose of part 5 entries are unknown.
    /// </summary>
    public class NefsHeaderPt5Entry
    {
        public const UInt32 SIZE = 4;

        NefsHeaderPt5 _parent;
        /// <summary>The relative offset into part 5 where the entry begins.</summary>
        UInt32 _relOffset;

        [FileData]
        ByteArrayType byte1 = new ByteArrayType(0x00, 0x01);
        [FileData]
        ByteArrayType byte2 = new ByteArrayType(0x01, 0x01);
        [FileData]
        ByteArrayType byte3 = new ByteArrayType(0x02, 0x01);
        [FileData]
        ByteArrayType byte4 = new ByteArrayType(0x03, 0x01);

        /// <summary>
        /// Loads an entry from a file stream.
        /// </summary>
        /// <param name="file">The file stream to load from.</param>
        /// <param name="parent">The part 5 obj this entry belongs to.</param>
        /// <param name="relOffset">The relative offset into part 5 where the entry begins.</param>
        public NefsHeaderPt5Entry(FileStream file, NefsHeaderPt5 parent, UInt32 relOffset)
        {
            _parent = parent;
            _relOffset = relOffset;

            /* Read data from file as defined by [FileData] fields. */
            FileData.ReadData(file, _parent.Offset + relOffset, this);
        }

        public byte Byte1
        {
            get { return byte1.Value[0]; }
        }

        public byte Byte2
        {
            get { return byte2.Value[0]; }
        }

        public byte Byte3
        {
            get { return byte3.Value[0]; }
        }

        public byte Byte4
        {
            get { return byte4.Value[0]; }
        }

        /// <summary>
        /// The relative offset into part 5 where the entry begins.
        /// </summary>
        public UInt32 RelOffset
        {
            get { return _relOffset; }
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
