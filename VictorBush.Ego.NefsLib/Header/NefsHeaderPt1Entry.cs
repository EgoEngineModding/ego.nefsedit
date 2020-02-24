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
    /// A record for an item in an archive. Contains details relating to the item's
    /// data within the archive.
    /// </summary>
    public class NefsHeaderPt1Entry
    {
        public const UInt32 SIZE = 0x14;

        /// <summary>Absolute offset into the archive where this entry begins.</summary>
        UInt32 _offset;

        [FileData]
        UInt64Type off_0x00_offset_to_data = new UInt64Type(0x00);
        [FileData]
        UInt32Type off_0x08_offset_in_hdr_pt2 = new UInt32Type(0x08);
        [FileData]
        UInt32Type off_0x0c_offset_in_hdr_pt4 = new UInt32Type(0x0c);
        [FileData]
        UInt32Type off_0x10_index = new UInt32Type(0x10);

        /// <summary>
        /// Loads a part 1 entry from a file stream.
        /// </summary>
        /// <param name="file">The file stream to load from.</param>
        /// <param name="offset">The absolute offset into the file where the entry begins.</param>
        internal NefsHeaderPt1Entry(Stream file, UInt32 offset)
        {
            _offset = offset;

            /* Read data from file as defined by [FileData] fields. */
            FileData.ReadData(file, offset, this);
        }

        /// <summary>
        /// The id of the item this entry belongs to.
        /// </summary>
        public UInt32 Id
        {
            get { return off_0x10_index.Value; }
        }

        /// <summary>
        /// Absolute offset to the compressed data for this item in the archive.
        /// </summary>
        public UInt64 OffsetToData
        {
            get { return off_0x00_offset_to_data.Value; }
        }

        /// <summary>
        /// Scaled offset into header part 2 where the part 2 entry for this item begins.
        /// </summary>
        public UInt32 OffsetIntoPt2
        {
            get { return off_0x08_offset_in_hdr_pt2.Value * 20; }
        }

        /// <summary>
        /// Raw value for the offset into header part 2 for this item. This is the value 
        /// that is stored in the header. The actual offset is scaled by 20.
        /// </summary>
        public UInt32 OffsetIntoPt2Raw
        {
            get { return off_0x08_offset_in_hdr_pt2.Value; }
        }

        /// <summary>
        /// Scaled offset into header part 2 where the part 2 entry for this item begins.
        /// </summary>
        public UInt32 OffsetIntoPt4
        {
            get { return off_0x0c_offset_in_hdr_pt4.Value * 4; }
        }

        /// <summary>
        /// Raw value for the offset into header part 4 for this item. This is the value 
        /// that is stored in the header. The actual offset is scaled by 4.
        /// </summary>
        public UInt32 OffsetIntoPt4Raw
        {
            get { return off_0x0c_offset_in_hdr_pt4.Value; }
        }

        /// <summary>
        /// Updates the data in this header entry for the specified item.
        /// </summary>
        /// <param name="item">The item this entry belongs to.</param>
        internal void Update(NefsItem item)
        {
            off_0x00_offset_to_data.Value = item.DataOffset;
            off_0x08_offset_in_hdr_pt2.Value = item.OffsetIntoPt2Raw;
            off_0x0c_offset_in_hdr_pt4.Value = item.OffsetIntoPt4Raw;
            off_0x10_index.Value = item.Id;
        }

        /// <summary>
        /// Writes this entry to a file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        internal void Write(FileStream file)
        {
            FileData.WriteData(file, _offset, this);
        }
    }
}
