using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace View
{
    //Генерирует ручку для рисования.
    public static class PenGiver
    {
        /// <summary>
        /// Ширина ручки для отображения нитки.
        /// </summary>
        public static int ThicknessPen = 3;
        /// <summary>
        /// Ширина ручки для фантома нитки в пассивном состоянии. (Для актуальности фантома необходимо что бы данный параметр был больше ThicknessPen)
        /// </summary>
        public static int ThicknessPhantomPen = 4;
        /// <summary>
        /// Ширина ручки для фантома нитки в активном состоянии. (Для актуальности фантома необходимо что бы данный параметр был больше ThicknessPen и ThicknessPhantomPen)
        /// </summary>
        public static int WorkThicknessPhantomPen = 100;

        public enum ColorPenForTrainPath
        {
            /// <summary>
            /// "Темно-зеленый" цвет RGB= dec(255, 0, 128, 0) or hex(FF, 00, 80, 00)
            /// </summary>
            DarkGreen,

            /// <summary>
            /// "Голубой королевский" цвет RGB= dec(255, 65, 105, 225) or hex(FF, 41, 69, E1)
            /// </summary>
            RoyalBlue,

            /// <summary>
            /// "Красный" цвет RGB= dec(255, 255, 0, 0) or hex(FF, FF, 00, 00)
            /// </summary>
            Red,

            /// <summary>
            /// "Невидимый" цвет RGB= dec(255, 0, 0, 0) or hex(FF, 00, 00, 00)
            /// </summary>
            Phantom,

            /// <summary>
            /// "Невидимый" цвет для отображения всех контуров нитки RGB= dec(255, 0, 0, 0) or hex(FF, 00, 00, 00)
            /// </summary>
            TestPhantom
        }

        public static Pen CreatePen(ColorPenForTrainPath ColorPen)
        {
            switch (ColorPen)
            {
                case ColorPenForTrainPath.DarkGreen:
                    return new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 128, 0)), ThicknessPen);
                case ColorPenForTrainPath.RoyalBlue:
                    return new Pen(new SolidColorBrush(Color.FromArgb(255, 65, 105, 225)), ThicknessPen);
                case ColorPenForTrainPath.Red:
                    return new Pen(new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)), ThicknessPen);
                case ColorPenForTrainPath.Phantom:
                    return new Pen(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), ThicknessPhantomPen);
                case ColorPenForTrainPath.TestPhantom:
                    return new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)), ThicknessPhantomPen);
                default:
                    throw new System.ArgumentException("Unknown Color (Неизвестный цвет) in PenGiver class (CreatePen1)", "original");
            }
        }

        public static Pen CreatePen(ColorPenForTrainPath ColorPen, double Thickness)
        {
            switch (ColorPen)
            {
                case ColorPenForTrainPath.DarkGreen:
                    return new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 128, 0)), Thickness);
                case ColorPenForTrainPath.RoyalBlue:
                    return new Pen(new SolidColorBrush(Color.FromArgb(255, 65, 105, 225)), Thickness);
                case ColorPenForTrainPath.Red:
                    return new Pen(new SolidColorBrush(Color.FromArgb(255, 255, 0, 0)), Thickness);
                case ColorPenForTrainPath.Phantom:
                    return new Pen(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), Thickness);
                case ColorPenForTrainPath.TestPhantom:
                    return new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 0, 0)), Thickness);
                default:
                    throw new System.ArgumentException("Unknown Color (Неизвестный цвет) in PenGiver class (CreatePen2)", "original");
            }
        }
    }
}
