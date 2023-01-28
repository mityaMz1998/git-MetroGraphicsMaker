using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Globalization;

namespace Converters
{
    public static class TypeDataConverter
    {
        public static Double StringToDouble(String value)
        {
            var culture = CultureInfo.CurrentCulture;
            if (value.Contains(","))
            {
                culture = CultureInfo.GetCultureInfo("ru-ru");
            }
            else if (value.Contains("."))
            {
                culture = CultureInfo.GetCultureInfo("en-US");
            }
            return Convert.ToDouble(value, culture);
        }
    }
}
