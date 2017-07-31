using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VictorBush.Ego.NefsLib.Utility;

namespace VictorBush.Ego.NefsLib.Header
{
    public class NefsHeaderPt1
    {
        private static readonly ILog log = LogHelper.GetLogger();

        List<NefsHeaderPt1Entry> _entries = new List<NefsHeaderPt1Entry>();
        UInt32 _offset;
        UInt32 _size;

        /// <summary>
        /// Loads header part 1 from a file stream.
        /// </summary>
        /// <param name="file">The file stream to load from.</param>
        /// <param name="offset">The absolute offset into the file where part 1 starts.</param>
        /// <param name="size">The size of part 1 in bytes.</param>
        /// <param name="p">Progress info.</param>
        internal NefsHeaderPt1(FileStream file, UInt32 offset, UInt32 size, NefsProgressInfo p)
        {
            _offset = offset;
            _size = size;

            /* Validate inputs */
            if (size == 0)
            {
                log.Warn("Header part 1 has a size of 0.");
                return;
            }

            UInt32 nextEntry = offset;
            UInt32 lastEntry = offset + size - NefsHeaderPt1Entry.SIZE;
            UInt32 numEntries = size / NefsHeaderPt1Entry.SIZE;

            /* Load each entry */
            while (nextEntry <= lastEntry)
            {
                p.BeginTask(1.0f / (float)numEntries);
                
                var entry = new NefsHeaderPt1Entry(file, nextEntry);
                _entries.Add(entry);
                nextEntry += NefsHeaderPt1Entry.SIZE;

                p.EndTask();
            }
        }

        /// <summary>
        /// Gets list of part 1 entries.
        /// </summary>
        public List<NefsHeaderPt1Entry> Entries
        {
            get { return _entries; }
        }

        /// <summary>
        /// Absolute offset to first compressed data in the archive.
        /// </summary>
        public UInt32 FirstItemDataOffset
        {
            get
            {
                UInt32 firstItemOffset = (from e in _entries
                                          where e.OffsetToData != 0
                                          select e.OffsetToData).FirstOrDefault();
                return firstItemOffset;
            }
        }

        /// <summary>
        /// Absolute offset into the archive where part 1 begins.
        /// </summary>
        public UInt32 Offset
        {
            get { return _offset; }
        }

        /// <summary>
        /// Size of part 1 in bytes.
        /// </summary>
        public UInt32 Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets a part 1 header entry by the item's id number.
        /// </summary>
        /// <param name="id">The id of the entry to get.</param>
        public NefsHeaderPt1Entry GetEntry(UInt32 id)
        {
            var entry = from e in _entries
                        where e.Id == id
                        select e;

            if (entry.Count() == 0)
            {
                throw new ArgumentException("Couldn't find a part 1 entry for id " + id);
            }

            return entry.First();
        }

        /// <summary>
        /// Writes header part 1 to a file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        /// <param name="p">Progress info.</param>
        public void Write(FileStream file, NefsProgressInfo p)
        {
            /* Write the data for each entry. Entries must be written in order. */
            foreach (var e in _entries)
            {
                p.BeginTask(1.0f / (float)_entries.Count);
                e.Write(file);
                p.EndTask();
            }

            /* Update size (offset of part 1 doesn't change) */
            _size = (uint)_entries.Count * NefsHeaderPt1Entry.SIZE;
        }
    }
}
