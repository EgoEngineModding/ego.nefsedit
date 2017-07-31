using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsLib.DataTypes
{
    class ByteArrayType : DataType
    {
        UInt32 _size;
        byte[] _value;

        public ByteArrayType(int offset, UInt32 size) : base(offset)
        {
            if (size == 0)
            {
                throw new ArgumentOutOfRangeException("ByteArrayType must have size greater than 0 bytes.");
            }

            _size = size;
            _value = new byte[size];
        }

        public override UInt32 Size
        {
            get { return _size; }
        }

        public byte[] Value
        {
            get { return _value; }
        }

        public override byte[] GetBytes()
        {
            return _value;
        }

        public UInt32 GetUInt32(UInt32 offset)
        {
            if (offset >= _size)
            {
                throw new ArgumentOutOfRangeException("Offset outside of byte array.");
            }

            if ((int)_value.Length - (int)offset < 4)
            {
                throw new ArgumentOutOfRangeException("Offset must be at least 4 bytes from the end of the array.");
            }

            return BitConverter.ToUInt32(_value, (int)offset);
        }

        public override void Read(FileStream file, UInt32 baseOffset)
        {
            _value = this.readFile(file, baseOffset);
        }
    }
}
