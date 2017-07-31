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
    public class NefsHeaderPt6
    {
        private static readonly ILog log = LogHelper.GetLogger();

        List<NefsHeaderPt6Entry> _entries = new List<NefsHeaderPt6Entry>();
        UInt32 _offset;
        UInt32 _size;

        /// <summary>
        /// Loads header part 6 from a file stream.
        /// </summary>
        /// <param name="file">The file stream to load from.</param>
        /// <param name="offset">The absolute offset into the file where part 6 starts.</param>
        /// <param name="size">The size of part 6 in bytes.</param>
        /// <param name="p">Progress info.</param>
        public NefsHeaderPt6(FileStream file, UInt32 offset, UInt32 size, NefsProgressInfo p)
        {
            _offset = offset;
            _size = size;

            /* Validate inputs */
            if (size == 0)
            {
                log.Warn("Header part 6 has a size of 0.");
                return;
            }

            /* Load part 6 entries */
            uint entrySize = NefsHeaderPt6Entry.SIZE;
            uint numEntries = size / entrySize;

            for (uint i =0; i< numEntries; i++)
            {
                _entries.Add(new NefsHeaderPt6Entry(file, this, (i * 8)));
            }
        }

        /// <summary>
        /// Absolute offset into the archive where part 6 begins.
        /// </summary>
        public UInt32 Offset
        {
            get { return _offset; }
            internal set { _offset = value; }
        }

        /// <summary>
        /// Size of part 6 in bytes.
        /// </summary>
        public UInt32 Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets a part 6 header entry by the item's id number.
        /// </summary>
        /// <param name="id">The id of the entry to get.</param>
        public NefsHeaderPt6Entry GetEntry(UInt32 id)
        {
            if (id >= _entries.Count)
            {
                throw new Exception("Couldn't find a part 6 entry for id " + id);
            }

            return _entries[(int)id];
        }

        /// <summary>
        /// Writes header part 6 to a file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        /// <param name="p">Progress info.</param>
        public void Write(FileStream file, NefsProgressInfo p)
        {
            foreach( var entry in _entries)
            {
                p.BeginTask(1.0f / (float)_entries.Count);
                entry.Write(file);
                p.EndTask();
            }

            /* Update size */
            _size = (uint)_entries.Count * NefsHeaderPt6Entry.SIZE;
        }
    }
}
