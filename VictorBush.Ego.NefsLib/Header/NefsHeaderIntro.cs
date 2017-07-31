using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using VictorBush.Ego.NefsLib.DataTypes;

namespace VictorBush.Ego.NefsLib.Header
{
    public class NefsHeaderIntro
    {
        const int OFFSET = 0x0;

        /// <summary>File magic number; "NeFS" or 0x5346654E</summary>
        [FileData]
        UInt32Type _hdr_0000_magic_number = new UInt32Type(0x0000);

        /// <summary>Expected hash of header.</summary>
        [FileData]
        ByteArrayType _hdr_0004_expected_hash = new ByteArrayType(0x0004, 0x20);

        /// <summary>Unknown string of ASCII characters that represent hex values. Seems constant throughoutnefs files.</summary>
        [FileData]
        ByteArrayType _hdr_0024_ascii_hex_str = new ByteArrayType(0x0024, 0x40);

        /// <summary>Size of header in bytes.</summary>
        [FileData]
        UInt32Type _hdr_0064_header_size = new UInt32Type(0x0064);

        /// <summary>Appears to be a constant number expected to be 0x20000</summary>
        [FileData]
        UInt32Type _hdr_0068 = new UInt32Type(0x0068);

        /// <summary>Unknown value</summary>
        [FileData]
        UInt32Type _hdr_006c = new UInt32Type(0x006c);
        
        /// <summary>8 bytes; Another constant; the last four bytes are "zlib" in ASCII.</summary>
        [FileData]
        UInt64Type _hdr_0070_zlib = new UInt64Type(0x0070);

        /// <summary>Unknown value</summary>
        [FileData]
        UInt64Type _hdr_0078 = new UInt64Type(0x0078);

        /// <summary>Unknown, maybe constant (01 00 00 01)</summary>
        [FileData]
        UInt32Type _hdr_0080 = new UInt32Type(0x0080);

        /// <summary>Offset the header part 1.</summary>
        [FileData]
        UInt32Type _hdr_0084_offset_to_part_1 = new UInt32Type(0x0084);

        /// <summary>??? Offset into some part of header part 5.</summary>
        [FileData]
        UInt32Type _hdr_0088_offset_into_part_5 = new UInt32Type(0x0088);

        /// <summary>Offset to header part 2.</summary>
        [FileData]
        UInt32Type _hdr_008c_offset_to_part_2 = new UInt32Type(0x008c);

        /// <summary>Offset to header part 6.</summary>
        [FileData]
        UInt32Type _hdr_0090_offset_to_part_6 = new UInt32Type(0x0090);

        /// <summary>Offset to header part 3 (the filename/directory strings list).</summary>
        [FileData]
        UInt32Type _hdr_0094_offset_to_part_3_strings = new UInt32Type(0x0094);

        /// <summary>Offset to header part 4.</summary>
        [FileData]
        UInt32Type _hdr_0098_offset_to_part_4 = new UInt32Type(0x0098);

        /// <summary>Offset to header part 5. First four bytes in header part 5 contain the size of the nefs file.</summary>
        [FileData]
        UInt32Type _hdr_009c_offset_to_part_5 = new UInt32Type(0x009c);

        /// <summary>??? Offset to data??????.</summary>
        [FileData]
        UInt32Type _hdr_00a0_offset_to_data = new UInt32Type(0x00a0);

        /// <summary>Unknown chunk of data.</summary>
        [FileData]
        ByteArrayType _hdr_00a4 = new ByteArrayType(0x00a4, 0x5c);

        UInt32 _part1_size;
        UInt32 _part2_size;
        UInt32 _part3_size;
        UInt32 _part4_size;
        UInt32 _part5_size;
        UInt32 _part6_size;

        /// <summary>
        /// Parses the introductory section of the NeFS header.
        /// </summary>
        /// <param name="file">NeFS file to parse.</param>
        /// <param name="p">Progress reporting info.</param>
        public NefsHeaderIntro(FileStream file, NefsProgressInfo p)
        {
            /* Read the file data as defined by [FileData] fields */
            FileData.ReadData(file, OFFSET, this);

            /* Calculate the sizes of the different header parts */
            _part1_size = _hdr_008c_offset_to_part_2.Value - _hdr_0084_offset_to_part_1.Value;
            _part2_size = _hdr_0094_offset_to_part_3_strings.Value - _hdr_008c_offset_to_part_2.Value;
            _part3_size = _hdr_0098_offset_to_part_4.Value - _hdr_0094_offset_to_part_3_strings.Value;
            _part4_size = _hdr_009c_offset_to_part_5.Value - _hdr_0098_offset_to_part_4.Value;
            _part5_size = _hdr_0090_offset_to_part_6.Value - _hdr_009c_offset_to_part_5.Value;
            _part6_size = _hdr_00a0_offset_to_data.Value - _hdr_0090_offset_to_part_6.Value;

            // TODO : Verify hash??
        }

        /// <summary>
        /// The expected hash of the archive's header.
        /// </summary>
        public byte[] ExpectedHash
        {
            get { return _hdr_0004_expected_hash.Value; }
        }

        /// <summary>
        /// Appears to be the offset to compressed data, but does not seem to point
        /// to the actual compressed file data. It points to right after header part 6.
        /// </summary>
        public UInt32 DataOffset
        {
            get { return _hdr_00a0_offset_to_data.Value; }
            internal set { _hdr_00a0_offset_to_data.Value = value; }
        }

        /// <summary>
        /// Size of the header. Used to calculate the header's hash.
        /// </summary>
        public UInt32 HeaderSize
        {
            get { return _hdr_0064_header_size.Value; }
        }

        /// <summary>
        /// The archive file's magic number. Must be "NeFS" (0x5346654E).
        /// </summary>
        public UInt32 MagicNumber
        {
            get { return _hdr_0000_magic_number.Value; }
        }

        /// <summary>
        /// Unkonwn, some offset into part 5.
        /// </summary>
        public UInt32 OffsetIntoPart5
        {
            get { return _hdr_0088_offset_into_part_5.Value; }
            internal set { _hdr_0088_offset_into_part_5.Value = value; }
        }

        /// <summary>
        /// Offset to header part 1.
        /// </summary>
        public UInt32 Part1Offset
        {
            get { return _hdr_0084_offset_to_part_1.Value; }
        }

        /// <summary>
        /// Length in bytes of header part 1.
        /// </summary>
        public UInt32 Part1Size
        {
            get { return _part1_size; }
            internal set { _part1_size = value; }
        }

        /// <summary>
        /// Offset to header part 2.
        /// </summary>
        public UInt32 Part2Offset
        {
            get { return _hdr_008c_offset_to_part_2.Value; }
            internal set { _hdr_008c_offset_to_part_2.Value = value; }
        }

        /// <summary>
        /// Length in bytes of header part 2.
        /// </summary>
        public UInt32 Part2Size
        {
            get { return _part2_size; }
            internal set { _part2_size = value; }
        }

        /// <summary>
        /// Offset to header part 3 (strings table).
        /// </summary>
        public UInt32 Part3Offset
        {
            get { return _hdr_0094_offset_to_part_3_strings.Value; }
            internal set { _hdr_0094_offset_to_part_3_strings.Value = value; }
        }

        /// <summary>
        /// Length in bytes of header part 3.
        /// </summary>
        public UInt32 Part3Size
        {
            get { return _part3_size; }
            internal set { _part3_size = value; }
        }

        /// <summary>
        /// Offset to header part 4 (data chunk sizes).
        /// </summary>
        public UInt32 Part4Offset
        {
            get { return _hdr_0098_offset_to_part_4.Value; }
            internal set { _hdr_0098_offset_to_part_4.Value = value; }
        }

        /// <summary>
        /// Length in bytes of header part 4.
        /// </summary>
        public UInt32 Part4Size
        {
            get { return _part4_size; }
            internal set { _part4_size = value; }
        }

        /// <summary>
        /// Offset to header part 5.
        /// </summary>
        public UInt32 Part5Offset
        {
            get { return _hdr_009c_offset_to_part_5.Value; }
            internal set { _hdr_009c_offset_to_part_5.Value = value; }
        }

        /// <summary>
        /// Length in bytes of header part 5.
        /// </summary>
        public UInt32 Part5Size
        {
            get { return _part5_size; }
            internal set { _part5_size = value; }
        }

        /// <summary>
        /// Offset to header part 6.
        /// </summary>
        public UInt32 Part6Offset
        {
            get { return _hdr_0090_offset_to_part_6.Value; }
            internal set { _hdr_0090_offset_to_part_6.Value = value; }
        }

        /// <summary>
        /// Length in bytes of header part 6.
        /// </summary>
        public UInt32 Part6Size
        {
            get { return _part6_size; }
            internal set { _part6_size = value; }
        }

        /// <summary>
        /// Writes the header intro to the file stream.
        /// </summary>
        /// <param name="file">The file stream to write to.</param>
        public void Write(FileStream file)
        {
            /* Write data as defined by the [FileData] fields. */
            FileData.WriteData(file, OFFSET, this);
        }
    }
}
