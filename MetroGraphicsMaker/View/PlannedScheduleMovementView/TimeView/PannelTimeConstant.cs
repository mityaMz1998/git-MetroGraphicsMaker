using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace View
{
    public static class PannelTimeConstant
    {
        /// <summary>
        /// Граница ПГД.
        /// </summary>
        public static int BorderPannelTime = 40;
        /// <summary>
        /// Высота панели времени.
        /// </summary>
        public static int HeightScalePannelTime = 24;
        /// <summary>
        /// Расстояние между шкалой времени и панели станций.
        /// </summary>
        public static int PannelStationTopSubScalePannelTime = 36;
        /// <summary>
        /// Высота панели станций. (Одной)
        /// </summary>
        public static int HeightPannelTime = BorderPannelTime + HeightScalePannelTime + PannelStationTopSubScalePannelTime;
        /// <summary>
        /// Сдвиг текста (Label) часового времени относительно позиции линии времени. Для времени из одного символа. (Позиция линии (X,Y); Сдвиг (X1,Y1) => Позиция текста (X-X1,Y-Y1).
        /// </summary>
        public static Point ShiftPositionLabelHourForOneSymbol = new Point(11, 36);
        /// <summary>
        /// Сдвиг текста (Label) часового времени относительно позиции линии времени. Для времени из двух символов. (Позиция линии (X,Y); Сдвиг (X1,Y1) => Позиция текста (X-X1,Y-Y1).
        /// </summary> 
        public static Point ShiftPositionLabelHourForTwoSymbols = new Point(16, 36);
        /// <summary>
        /// Сдвиг текста (Label) часового времени относительно позиции линии времени. (Позиция линии (X,Y); Сдвиг (X1,Y1) => Позиция текста (X-X1,Y-Y1).
        /// </summary>
        public static Point ShiftPositionLabelMinute = new Point(13, 4.5);
        /// <summary>
        /// Высота одной отметки на панели времени.
        /// </summary>
        public static int HeightTimeMinuteLineOnPannel = 8;
        /// <summary>
        /// Высота одной отметки на панели времени.
        /// </summary>
        public static int HeightTimeMinuteLineOnStation = 10;
    }
}
