using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace View
{
    //Описывает расположение линии станций на рабочей области.
    public class StationLineForWorkArea : Control
    {
        protected override void OnRender(DrawingContext DC)
        {
            SolidColorBrush ColorPhantom = new SolidColorBrush();
            ColorPhantom.Color = Color.FromArgb(0, 0, 0, 255);//(255, 0, 255, 0);
            Pen PenPhantom = new Pen(ColorPhantom, 5);
            DC.DrawLine(PenPhantom, new Point(0, 0), new Point(Width, 0));

            Pen PenLineStation = new Pen(Brushes.Black, 1);
            DC.DrawLine(PenLineStation, new Point(0, 0), new Point(Width, 0));
        }

        /// <summary>
        /// Конструктор линии для рабочей области диаграммы
        /// </summary>
        public StationLineForWorkArea()
        {
        }

    }
}
