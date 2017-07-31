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
    /// Not sure exactly what this data is. Between header part 6 and the start of the first
    /// item's compressed data, there is an unknown chunk of data. It straddles the "header size"
    /// offset that is used when performing a hash check of the header. So I'm not sure how much
    /// of this is actually part of the header or not. Needs investigated.
    /// </summary>
    public class NefsHeaderPt7
    {
        private static readonly ILog log = LogHelper.GetLogger();

        UInt32 _offset;
        UInt32 _size;

        [FileData]
        ByteArrayType all_the_data;

        /// <summary>
        /// Loads header part 7 from a file stream.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="offset"></param>
        /// <param name="size"></param>
        public NefsHeaderPt7(FileStream file, UInt32 offset, UInt32 size)
        {
            _offset = offset;
            _size = size;

            /* Validate inputs */
            if (size == 0)
            {
                log.Warn("Header part 7 has a size of 0.");
                return;
            }

            all_the_data = new ByteArrayType(0x0, size);

            FileData.ReadData(file, offset, this);
        }

        /// <summary>
        /// Writes header part 7 to a file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        public void Write(FileStream file)
        {
            if (_size == 0)
            {
                log.Debug("Can't write header part 7 (size is 0).");
                return;
            }

            FileData.WriteData(file, _offset, this);

            /* Update size */
            _size = all_the_data.Size;
        }
    }
}
