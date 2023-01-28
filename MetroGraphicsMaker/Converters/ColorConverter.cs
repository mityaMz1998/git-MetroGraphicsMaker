using System;
using System.Collections.Generic;
using System.Text;

using System.Windows.Media;
using Messages;

namespace Converters
{
    public static class ColorConverter
    {
        const String prefix = "0xFF";

        /// <summary>
        /// Convert decimal number to hexadecimal.
        /// </summary>
        /// <param name="decNumber">Decimal number.</param>
        /// <returns>String contain hex number in format 0xFFZZZZZZ, where Z may be 0-9, A-F, a-divisor.</returns>
        public static String decToHex(Int32 decNumber)
        {
            var str = Convert.ToString(decNumber, 16).ToUpperInvariant();
            var sb = new StringBuilder("000000");
            var sbLength = sb.Length;

            if (str.Length <= sbLength)
            {
                sb.Insert(sbLength - str.Length, str);
                sb.Remove(sbLength, str.Length);
            }
            sb.Insert(0, prefix);
            return sb.ToString();
        }

        public static String DecToHex(Int32 decNumber)
        {
            return prefix + decNumber.ToString("X");

        }


        private static readonly Dictionary<String, Color> colors = new Dictionary<String, Color>
        {
            {"0xFF000000", Colors.Black},
            {"0xFF0000FF", Colors.Blue},
            {"0xFF00FF00", Colors.Green},
            {"0xFFFF0000", Colors.Red},
            {"0xFFFFFFFF", Colors.White},
            {"0xFF006400", Colors.DarkGreen}

        }; 

        public static Color StringToColor(String colorStr)
        {
            var colorCode = Convert.ToInt32(colorStr);

            var color = Colors.Transparent;
            try
            {
                color = colors[decToHex(colorCode)];
            }
            catch (Exception)
            {
                Logger.Output("цвет не распознан " + decToHex(colorCode));
            }
            return color;
            
            //return Color.FromArgb(0xFF, (colorCode & 0xFF0000) >> 0x10 , (colorCode & 0x00FF00) >> 0x8, colorCode & 0x0000FF);
        }

        /// <summary>
        /// Convert hexadecimal number to decimal.
        /// </summary>
        /// <param name="hexNumber">String contain hexadecimal number in format 0xFFZZZZZZ, where Z may be 0-9, A-F, a-divisor.</param>
        /// <returns>Decimal number.</returns>
        public static Int32 hexToDec(String hexNumber)
        {
            return Convert.ToInt32(hexNumber.Remove(0, 4), 16);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="hexColorCode"></param>
        /// <returns></returns>
        public static Color hexToColor(String hexColorCode)
        {
            var argb = hexTrim(hexColorCode);
            return Color.FromArgb(argb[0], argb[1], argb[2], argb[3]);
            //return Color.FromArgb(Convert.ToInt32(hexColorCode, 16));
        }

        public static Byte[] hexTrim(String hexColorCode)
        {
            var byteLength = 2;
            var bytes = new Byte[4];
            var tmp = hexColorCode.ToUpperInvariant().Remove(0, hexColorCode.Length - bytes.Length * byteLength);
           
            for (var i = 0; i < bytes.Length; ++i)
                bytes[i] = Convert.ToByte(tmp.Substring(i * byteLength, (i + 1) * byteLength));

            return bytes;
        }
    }
}
