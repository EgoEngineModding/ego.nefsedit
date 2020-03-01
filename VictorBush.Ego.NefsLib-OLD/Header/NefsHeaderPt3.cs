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
    /// <summary>
    /// Part 3 of the NeFS header contains the table of filename/directory strings.
    /// </summary>
    public class NefsHeaderPt3
    {
        private static readonly ILog log = LogHelper.GetLogger();

        UInt32 _offset;
        UInt32 _size;

        [FileData]
        ByteArrayType data;

        /// <summary>
        /// Loads header part 3 from a file stream.
        /// </summary>
        /// <param name="file">The file stream to load from.</param>
        /// <param name="offset">The absolute offset into the file where part 3 starts.</param>
        /// <param name="size">The size of part 3 in bytes.</param>
        /// <param name="p">Progress info.</param>
        internal NefsHeaderPt3(Stream file, UInt32 offset, UInt32 size, NefsProgressInfo p)
        {
            _offset = offset;
            _size = size;

            /* Validate inputs */
            if (size == 0)
            {
                log.Warn("Header part 3 has a size of 0.");
                return;
            }

            // Currently we are just storing all the strings from the file as-is and
            // not manipulating them.
            data = new ByteArrayType(0x0, size);

            FileData.ReadData(file, offset, this);
        }

        /// <summary>
        /// Absolute offset into the archive where part 3 begins.
        /// </summary>
        public UInt32 Offset
        {
            get { return _offset; }
            internal set { _offset = value; }
        }

        /// <summary>
        /// Size of part 3 in bytes.
        /// </summary>
        public UInt32 Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets the filename at the specified offset.
        /// </summary>
        /// <param name="offset"></param>
        /// <returns></returns>
        public string GetFilename(UInt32 offset)
        {
            var i = offset;
            var output = "";

            // Read one byte at a time until null terminator is reached
            while (i < data.Value.Length
                && data.Value[i] != 0)
            {
                output += Encoding.ASCII.GetString(data.Value, (int)i, 1);
                i++;
            }

            return output;
        }

        /// <summary>
        /// Writes header part 3 to a file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        public void Write(FileStream file, NefsProgressInfo p)
        {
            FileData.WriteData(file, _offset, this);

            /* Update size */
            _size = (uint)data.Size;
        }
    }
}
