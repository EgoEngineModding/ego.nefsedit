using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VictorBush.Ego.NefsLib.Utility;
using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Header
{
    /// <summary>
    /// Part 4 contains a list of compressed chunk sizes (cumulative)
    /// for each item in the archive.
    /// </summary>
    public class NefsHeaderPt4
    {
        private static readonly ILog log = LogHelper.GetLogger();

        UInt32 _offset;
        UInt32 _size;

        [FileData]
        UInt32Type last_four_bytes;

        /// <summary>
        /// Loads header part 4 from a file stream.
        /// </summary>
        /// <param name="file">The file stream to load from.</param>
        /// <param name="offset">The absolute offset into the file where part 4 starts.</param>
        /// <param name="size">The size of part 4 in bytes.</param>
        /// <param name="p">Progress info.</param>
        internal NefsHeaderPt4(Stream file, UInt32 offset, UInt32 size, NefsProgressInfo p)
        {
            _offset = offset;
            _size = size;

            /* Validate inputs */
            if (size == 0)
            {
                log.Warn("Header part 4 has a size of 0.");
                return;
            }

            // The entries for each item in chunk 4 are read when creating the NefsItem
            // since the offset into part 4 are stored in part 1.

            // The last four bytes of header 4 is some unknown value.
            // For now, just copy the data that was already there.
            last_four_bytes = new UInt32Type((int)size - 0x4);
            FileData.ReadData(file, offset, this);
        }

        /// <summary>
        /// Absolute offset into the archive where part 4 begins.
        /// </summary>
        public UInt32 Offset
        {
            get { return _offset; }
            internal set { _offset = value; }
        }

        /// <summary>
        /// Size of part 4 in bytes.
        /// </summary>
        public UInt32 Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Writes header part 4 to a file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        /// <param name="items">The list of NefsItems in this archive.</param>
        /// <param name="p">Progress info.</param>
        public void Write(FileStream file, List<NefsItem> items, NefsProgressInfo p)
        {
            int numChunkSizes = 0;

            /*
             * An item entry in part 4 is a list of compressed chunk sizes. These
             * sizes are cumulative:
             * - The first entry is the size of the first chunk.
             * - The second entry is the size of the second chunk + the first chunk.
             * - The last entry is the size of all compressed chunks together.
             */
            foreach (var item in items)
            {
                file.Seek(item.Archive.Header.Part4.Offset, SeekOrigin.Begin);
                file.Seek(item.OffsetIntoPt4, SeekOrigin.Current);

                foreach(var cs in item.ChunkSizes)
                {
                    file.Write(BitConverter.GetBytes(cs), 0, 4);
                    numChunkSizes++;
                }
            }

            /* Update size */
            _size = (uint)numChunkSizes * 4;
            _size += last_four_bytes.Size;

            /* Write the unknown section of this header part */
            //FileData.WriteData(file, _offset, this);
            if (items.Count > 0)
            {
                file.Seek(items[0].Archive.Header.Part4.Offset, SeekOrigin.Begin);
                file.Seek(_size - 4, SeekOrigin.Current);
                file.Write(BitConverter.GetBytes(last_four_bytes.Value), 0, 4);
            }
        }
    }
}
