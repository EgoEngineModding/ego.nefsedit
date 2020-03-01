using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header
{
    public class NefsHeaderPt5
    {
        private static readonly ILog log = LogHelper.GetLogger();

        List<NefsHeaderPt5Entry> _entries = new List<NefsHeaderPt5Entry>();
        UInt32 _offset;
        UInt32 _size;

        [FileData]
        UInt64Type off_0x00_archive_size = new UInt64Type(0x00);

        /// <summary>
        /// Loads header part 5 from a file stream.
        /// </summary>
        /// <param name="file">The file stream to load from.</param>
        /// <param name="offset">The absolute offset into the file where part 5 starts.</param>
        /// <param name="size">The size of part 5 in bytes.</param>
        /// <param name="p">Progress info.</param>
        public NefsHeaderPt5(Stream file, UInt32 offset, UInt32 size, NefsProgressInfo p)
        {
            _offset = offset;
            _size = size;

            /* Validate inputs */
            if (size == 0)
            {
                log.Warn("Header part 5 has a size of 0.");
                return;
            }

            /* Read the archive size */
            FileData.ReadData(file, offset, this);

            /* Item entries begin after the archive size entry */
            UInt32 firstEntryOffset = off_0x00_archive_size.Size;
            UInt32 entrySize = NefsHeaderPt5Entry.SIZE;
            UInt32 numEntries = (size - off_0x00_archive_size.Size) / entrySize;

            for (UInt32 i = 0; i < numEntries; i++)
            {
                _entries.Add(new NefsHeaderPt5Entry(file, this, firstEntryOffset + (i * entrySize)));
            }
        }

        /// <summary>
        /// The size of the NeFS archive in bytes.
        /// </summary>
        public UInt64 ArchiveSize
        {
            get { return off_0x00_archive_size.Value; }
            internal set { off_0x00_archive_size.Value = value; }
        }

        /// <summary>
        /// Absolute offset into the archive where part 5 begins.
        /// </summary>
        public UInt32 Offset
        {
            get { return _offset; }
            internal set { _offset = value; }
        }

        /// <summary>
        /// Size of part 5 in bytes.
        /// </summary>
        public UInt32 Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets a part 5 header entry by the item's id number.
        /// </summary>
        /// <param name="id">The id of the entry to get.</param>
        public NefsHeaderPt5Entry GetEntry(UInt32 id)
        {
            if (id >= _entries.Count)
            {
                throw new Exception("Couldn't find a part 5 entry for id " + id);
            }

            return _entries[(int)id];
        }

        /// <summary>
        /// Writes header part 5 to a file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        /// <param name="p">Progress info.</param>
        public void Write(FileStream file, NefsProgressInfo p)
        {
            /* Write the file size entry first */
            FileData.WriteData(file, _offset, this);

            /* Write the item entries */
            foreach (var entry in _entries)
            {
                p.BeginTask(1.0f / (float)_entries.Count);
                entry.Write(file);
                p.EndTask();
            }

            /* Update size */
            _size = off_0x00_archive_size.Size;
            _size += (uint)_entries.Count * NefsHeaderPt5Entry.SIZE;
        }
    }
}
