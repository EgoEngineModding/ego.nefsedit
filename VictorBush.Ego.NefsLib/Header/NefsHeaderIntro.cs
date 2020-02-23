using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using VictorBush.Ego.NefsLib.DataTypes;
using VictorBush.Ego.NefsLib.Utility;

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

        byte[] _public_key = {
            0xCF, 0x19, 0x63, 0x94, 0x1E, 0x0F, 0x42, 0x16, 0x35, 0xDE, 0x51, 0xD0, 0xB3, 0x3A, 0xB7, 0x67,
            0xC7, 0x1C, 0x8D, 0x3B, 0x27, 0x49, 0x40, 0x9E, 0x58, 0x43, 0xDD, 0x6D, 0xD9, 0xAA, 0xF5, 0x1B,
            0x94, 0x94, 0xC4, 0x30, 0x49, 0xBA, 0xE7, 0x72, 0x3D, 0xFA, 0xDF, 0x80, 0x17, 0x55, 0xF3, 0xAB,
            0xF8, 0x97, 0x42, 0xE6, 0xB2, 0xDF, 0x11, 0xE4, 0x93, 0x0E, 0x92, 0x1D, 0xC5, 0x4E, 0x0F, 0x87,
            0xCD, 0x46, 0x83, 0x06, 0x6B, 0x97, 0xA7, 0x00, 0x42, 0x35, 0xB0, 0x33, 0xEA, 0xEF, 0x68, 0x54,
            0xA0, 0xF9, 0x03, 0x41, 0xF7, 0x5C, 0xFF, 0xC3, 0x75, 0xE1, 0x1B, 0x00, 0x73, 0x5A, 0x7A, 0x81,
            0x68, 0xAF, 0xB4, 0x9F, 0x86, 0x3C, 0xD6, 0x09, 0x3A, 0xC0, 0x94, 0x6F, 0x18, 0xE2, 0x03, 0x38,
            0x14, 0xF7, 0xC5, 0x13, 0x91, 0x4E, 0xD0, 0x4F, 0xAC, 0x46, 0x6C, 0x70, 0x27, 0xED, 0x69, 0x99,
            00
        };

        byte[] _exponent = { 01, 00, 01, 00 };

        bool _is_encrypted;

        MemoryStream _decryptedStream;


        /// <summary>
        /// Parses the introductory section of the NeFS header.
        /// </summary>
        /// <param name="file">NeFS file to parse.</param>
        /// <param name="p">Progress reporting info.</param>
        public NefsHeaderIntro(FileStream file, NefsProgressInfo p)
        {
            /* Read the file data as defined by [FileData] fields */
            FileData.ReadData(file, OFFSET, this);
            _is_encrypted = false;

            if (MagicNumber != 0x5346654E)
            {
                _is_encrypted = true;
                _decryptedStream = new MemoryStream();

                file.Seek(0, SeekOrigin.Begin);
                byte[] encryptedHeader = new byte[0x81];
                file.Read(encryptedHeader, 0, 0x80);
                encryptedHeader[0x80] = 0;


                // Use big integers instead of RSA since the c# implementation forces the
                // use of padding.
                BigInteger n = new BigInteger(_public_key);
                BigInteger e = new BigInteger(_exponent);
                BigInteger m = new BigInteger(encryptedHeader);

                byte[] decrypted = BigInteger.ModPow(m, e, n).ToByteArray();

                _decryptedStream.Write(decrypted, 0, decrypted.Length);

                if (decrypted.Length != 0x80)
                {
                    for (int i = 0; i < (0x80 - decrypted.Length); i++)
                    {
                        _decryptedStream.WriteByte(0);
                    }
                }

                string asciiKey;
                UInt32 headerLen;
                BinaryReader br = new BinaryReader(_decryptedStream);
                // Read hex key
                _decryptedStream.Seek(0x24, SeekOrigin.Begin);
                asciiKey = new string(br.ReadChars(0x40));

                // Read header length
                _decryptedStream.Seek(0x64, SeekOrigin.Begin);
                headerLen = br.ReadUInt32();

                _decryptedStream.Seek(0x0, SeekOrigin.End);
                



                byte[] key = FormatHelper.FromHexString(asciiKey);

                using (RijndaelManaged rijAlg = new RijndaelManaged())
                {
                    rijAlg.KeySize = 256;
                    rijAlg.Key = key;
                    rijAlg.Mode = CipherMode.ECB;
                    rijAlg.BlockSize = 128;
                    rijAlg.Padding = PaddingMode.Zeros;

                    ICryptoTransform decryptor = rijAlg.CreateDecryptor();

                    CryptoStream csDecrypt = new CryptoStream(file, decryptor, CryptoStreamMode.Read);
                    byte[] buffer = new byte[headerLen - 0x80];
                    csDecrypt.Read(buffer, 0, (int)(headerLen - 0x80));
                    _decryptedStream.Write(buffer, 0, (int)(headerLen - 0x80));
                }
                //_decryptedStream.Seek(0, SeekOrigin.End);
                //file.CopyTo(_decryptedStream);
                _decryptedStream.Seek(0, SeekOrigin.Begin);
                FileData.ReadData(_decryptedStream, OFFSET, this);
            }

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
        /// The archive's encryption key
        /// </summary>
        public byte[] EncryptionKey
        {
            get { return _hdr_0024_ascii_hex_str.Value; }
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
        /// Wether the header is encrypted or not
        /// </summary>
        public bool IsEncrypted
        {
            get { return _is_encrypted; }
        }

        public byte[] PublicKey
        {
            get { return _public_key; }
        }

        public Stream DecryptedHeader
        {
            get { return _decryptedStream; }
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
