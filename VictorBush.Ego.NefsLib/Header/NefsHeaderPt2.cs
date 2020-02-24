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
    public class NefsHeaderPt2
    {
        private static readonly ILog log = LogHelper.GetLogger();

        List<NefsHeaderPt2Entry> _entries = new List<NefsHeaderPt2Entry>();
        UInt32 _offset;
        UInt32 _size;

        /// <summary>
        /// Loads header part 2 from a file stream.
        /// </summary>
        /// <param name="file">The file stream to load from.</param>
        /// <param name="offset">The absolute offset into the file where part 2 begins.</param>
        /// <param name="size">The size of part 2 in bytes.</param>
        /// <param name="p">Progress info.</param>
        public NefsHeaderPt2(Stream file, UInt32 offset, UInt32 size, NefsProgressInfo p)
        {
            _offset = offset;
            _size = size;

            /* Validate inputs */
            if (size == 0)
            {
                log.Warn("Header part 2 has a size of 0.");
                return;
            }

            UInt32 next_entry = offset;
            UInt32 last_entry = offset + size - NefsHeaderPt2Entry.SIZE;

            while (next_entry <= last_entry)
            {
                var entry = new NefsHeaderPt2Entry(file, next_entry);
                _entries.Add(entry);
                next_entry += NefsHeaderPt2Entry.SIZE;
            }
        }

        /// <summary>
        /// Absolute offset into the archive where part 2 begins.
        /// </summary>
        public UInt32 Offset
        {
            get { return _offset; }
            internal set { _offset = value; }
        }

        /// <summary>
        /// Size of part 2 in bytes.
        /// </summary>
        public UInt32 Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets a Part 2 entry corresponding to the specified Part 1 entry.
        /// </summary>
        /// <param name="pt1Entry">The Part 1 entry to get the matching Part 2 entry for.</param>
        public NefsHeaderPt2Entry GetEntry(NefsHeaderPt1Entry pt1Entry)
        {
            return GetEntry(pt1Entry.Id, pt1Entry.OffsetIntoPt2);
        }

        /// <summary>
        /// Gets a part 2 header entry by the item's id number.
        /// </summary>
        /// <param name="id">The id of the entry to get.</param>
        /// <param name="offsetIntoPt2">The relative offset into Part 2 where this entry begins.</param>
        public NefsHeaderPt2Entry GetEntry(UInt32 id, UInt32 offsetIntoPt2)
        {
            /* First try to get an entry based on Id */
            var entry = from e in _entries
                        where e.Id == id
                        select e;

            if (entry.Count() > 0)
            {
                return entry.First();
            }         

            /* If that fails, try to get the entry based on offset.
             *  It is possible that some items share Part 2 entries. */
            entry = from e in _entries
                    where e.Offset == this.Offset + offsetIntoPt2
                    select e;

            if (entry.Count() > 0)
            {
                return entry.First();
            }

            /* Could not find a Part 2 entry */
            throw new ArgumentException("Couldn't find a part 2 entry for id " + id);
        }

        /// <summary>
        /// Writes header part 2 to a file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        /// <param name="p">Progress info.</param>
        internal void Write(FileStream file, NefsProgressInfo p)
        {
            /* Write the data for each entry. Entries must be written in order. */
            foreach (var e in _entries)
            {
                p.BeginTask(1.0f / (float)_entries.Count);
                e.Write(file);
                p.EndTask();
            }

            /* Update size */
            _size = (uint)_entries.Count * NefsHeaderPt2Entry.SIZE;
        }
    }
}
