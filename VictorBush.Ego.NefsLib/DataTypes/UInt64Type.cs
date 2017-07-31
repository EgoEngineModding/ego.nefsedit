using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsLib.DataTypes
{
    class UInt64Type : DataType
    {
        private const UInt32 SIZE = 8;

        private UInt64 _value;

        public UInt64Type(int offset) : base(offset)
        { }

        public override UInt32 Size
        {
            get { return SIZE; }
        }

        public UInt64 Value
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
            _value = BitConverter.ToUInt64(temp, 0);
        }

        public override string ToString()
        {
            return this.Value.ToString("X");
        }
    }
}
