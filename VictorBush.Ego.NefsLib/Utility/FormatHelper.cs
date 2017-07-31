using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsLib.Utility
{
    public static class FormatHelper
    {
        public static string ToHexString(this UInt32 value, bool prefix = true)
        {
            if (prefix)
            {
                return string.Format("0x{0:X}", value);
            }
            else
            {
                return string.Format("{0:X}", value);
            }
        }
    }
}
