using System;
using System.Data;
using Messages;
using System.Windows.Media;
using windows = System.Windows;

using Converters;

namespace Core
{
    /// <summary>
    /// Класс содержит данные о надписях, добавлемых пользователем.
    /// </summary>
    public class Legend : ICloneable
    {
        static Int32 counter = 0;

        /// <summary>
        /// Идентификатор, для удобства поиска и работы с коллекцией объектов.
        /// </summary>
        Int32 _id;

        void setId(Int32 id = -1)
        {
            _id = id != -1 ? id : ++counter;
        }

        public Int32 getId()
        {
            return _id;
        }

        /// <summary>
        /// Угол в градусах.
        /// </summary>
        public Double AngleInDegree;

        /// <summary>
        /// Координата (X,Y).
        /// </summary>
        public windows.Point Coordinate;

        /// <summary>
        /// Координата (Х,Y) при 100% масштабе.
        /// </summary>
        public windows.Point NormalCoordinate;

        /// <summary>
        /// Текст надписи.
        /// </summary>      
        public String Text;

        /// <summary>
        /// Размер шрифта.
        /// </summary>
        public Double FontSize;

        /// <summary>
        /// Размер шрифта при 100% масштабе.
        /// </summary>
        public Int32 NormalFontSize;

        /// <summary>
        /// Цвет надписи.
        /// </summary>
        public String FontColorStr;


        /// <summary>
        /// Цвет надписи.
        /// </summary>
        public Color FontColor;

        public Legend()
        {
            setId();
            Coordinate = new windows.Point();
            NormalCoordinate = Coordinate;
        }

        public Legend(DataRow row)
        {
            // @TODO: Стоит озаботиться безопасностью -- например, try-catch секциями.
            try
            {
                setId(Convert.ToInt32(row["Код"].ToString()));
            }
            catch
            {
                setId();
            }

            NormalCoordinate = new windows.Point(
                Convert.ToDouble(row["Координата_X"].ToString()),
                Convert.ToDouble(row["Координата_Y"].ToString())
                );
            Coordinate = NormalCoordinate;
            AngleInDegree = Convert.ToDouble(row["Наклон"].ToString());
            Text = row["Текст"].ToString();
            NormalFontSize = Convert.ToInt32(row["Шрифт"].ToString());
            FontSize = NormalFontSize;
            FontColorStr = Converters.ColorConverter.decToHex(Convert.ToInt32(row["Цвет"].ToString()));
            FontColor = Converters.ColorConverter.StringToColor(row["Цвет"].ToString());
            //Logger.output(FontColor + " <--> " + FontColorStr + " <--> " + row["Текст"] + " -- " + row["Цвет"]);
        }

        public object Clone()
        {
            return new object();
        }

        /*
        public Legend Clone()
        {
            return (MemberwiseClone() as Legend);
        }
         */
    }
}
