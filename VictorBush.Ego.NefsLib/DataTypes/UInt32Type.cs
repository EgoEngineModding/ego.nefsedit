using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsLib.DataTypes
{
    class UInt32Type : DataType
    {
        private const int SIZE = 4;

        UInt32 _value = 0;

        public UInt32Type(int offset) : base(offset)
        { }

        public override UInt32 Size
        {
            get { return SIZE; }
        }

        public UInt32 Value
        {
            get { return _value; }
            set { _value = value; }
        }

        public override byte[] GetBytes()
        {
            return BitConverter.GetBytes(_value);
        }

        public override void Read(FileStream file, UInt32 baseOffset)
        {
            var temp = this.readFile(file, baseOffset);
            _value = BitConverter.ToUInt32(temp, 0);
        }

        public override string ToString()
        {
            /* Return value in hex */
            return Value.ToString("X");
        }
    }
}
