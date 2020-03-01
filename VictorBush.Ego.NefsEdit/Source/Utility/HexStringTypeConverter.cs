using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VictorBush.Ego.NefsEdit.Utility
{
    class HexStringTypeConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string) &&
                (value.GetType() == typeof(int) || value.GetType() == typeof(uint)))
            {
                return string.Format("0x{0:X8}", value);
            }
            else
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}
