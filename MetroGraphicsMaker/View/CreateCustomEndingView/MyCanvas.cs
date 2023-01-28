using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls;

namespace linii_graph
{
    public class MyCanvas : Canvas
    {
        protected Double LargeStep = 100;

        protected Double StandardStep = 20;

        protected Pen LargePen = new Pen(Brushes.Black, 1.5);

        protected Pen StandardPen = new Pen(Brushes.Gray, 1);

        protected Point StartPoint = new Point();

  
        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Background, null, new Rect(RenderSize));

            for (var x = StartPoint.X; x <= Width; x += StandardStep)
                dc.DrawLine((x % LargeStep == 0 ? LargePen : StandardPen), new Point(x, StartPoint.Y), new Point(x, Height));

            for (var y = StartPoint.Y; y <= Height; y += StandardStep)
                dc.DrawLine((y % LargeStep == 0 ? LargePen : StandardPen), new Point(StartPoint.X, y), new Point(Width, y));
        }
    }
}
