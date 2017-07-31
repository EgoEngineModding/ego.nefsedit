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
    /// Contains details relating to an item's extracted data.
    /// </summary>
    public class NefsHeaderPt2Entry
    {
        public const UInt32 SIZE = 0x14;

        /// <summary>Absolute offset into the archive where this entry begins.</summary>
        UInt32 _offset;

        [FileData]
        UInt32Type off_0x00_directory_id = new UInt32Type(0x00);
        [FileData]
        UInt32Type off_0x04_first_child_id = new UInt32Type(0x04);
        [FileData]
        UInt32Type off_0x08_off_into_str_table = new UInt32Type(0x08);
        [FileData]
        UInt32Type off_0x0c_extracted_size = new UInt32Type(0x0c);
        [FileData]
        UInt32Type off_0x10_index = new UInt32Type(0x10);

        /// <summary>
        /// Loads an entry from a file stream.
        /// </summary>
        /// <param name="file">The file stream to load from.</param>
        /// <param name="offset">The absolute offset into the file where the entry begins.</param>
        internal NefsHeaderPt2Entry(FileStream file, UInt32 offset)
        {
            _offset = offset;

            /* Read data from file as defined by [FileData] fields. */
            FileData.ReadData(file, offset, this);
        }

        /// <summary>
        /// The id of the directory this item is in.
        /// </summary>
        public UInt32 DirectoryId
        {
            get { return off_0x00_directory_id.Value; }
        }

        /// <summary>
        /// Extracted sisze of this item.
        /// </summary>
        public UInt32 ExtractedSize
        {
            get { return off_0x0c_extracted_size.Value; }
        }

        /// <summary>
        /// Offset into header part 3 (file strings table) for the name of this item.
        /// </summary>
        public UInt32 FilenameOffset
        {
            get { return off_0x08_off_into_str_table.Value; }
        }

        /// <summary>
        /// Id of the first child of this item. 
        /// - If the first child id matches this item's id, then there are no children.
        /// - If this item is a file, there won't be any children (only directories can have children).
        /// </summary>
        public UInt32 FirstChildId
        {
            get { return off_0x04_first_child_id.Value; }
        }

        /// <summary>
        /// The id of the item this entry belongs to.
        /// </summary>
        public UInt32 Id
        {
            get { return off_0x10_index.Value; }
        }

        /// <summary>
        /// The absolute offset to this entry in the archive.
        /// </summary>
        public UInt32 Offset
        {
            get { return _offset; }
        }

        /// <summary>
        /// Updates the data in this header entry for the specified item.
        /// </summary>
        /// <param name="item">The item this entry belongs to.</param>
        public void Update(NefsItem item)
        {
            off_0x00_directory_id.Value = item.DirectoryId;
            off_0x0c_extracted_size.Value = item.ExtractedSize;
            off_0x10_index.Value = item.Id;
        }

        /// <summary>
        /// Writes this entry to a file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        public void Write(FileStream file)
        {
            FileData.WriteData(file, _offset, this);
        }
    }
}
