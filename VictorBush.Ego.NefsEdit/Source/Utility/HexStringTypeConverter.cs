// See LICENSE.txt for license information.

using System.ComponentModel;
using System.Globalization;

namespace VictorBush.Ego.NefsEdit.Utility;

/// <summary>
/// Hex string formatting utility.
/// </summary>
internal class HexStringTypeConverter : TypeConverter
{
	/// <inheritdoc/>
	public override object ConvertTo(
		ITypeDescriptorContext context,
		CultureInfo culture,
		object value,
		Type destinationType)
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
