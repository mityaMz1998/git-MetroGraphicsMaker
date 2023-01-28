using System;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfApplication1.Forms.EditorWindows.GraphCore;

namespace linii_graph.View
{
    public class EdgeView
    {
        private Edge edge;

        public Line Line { get; private set; }

        public Label Label { get; private set; }

        public EdgeView(Edge parentEdge)
        {
            if (parentEdge == null)
                throw new ArgumentNullException("parentEdge");

            edge = parentEdge;

            Line = new Line
            {
                X1 = edge.Source.View.Circle.Margin.Left + 5,
                Y1 = edge.Source.View.Circle.Margin.Top + 5,
                X2 = edge.Target.View.Circle.Margin.Left + 5,
                Y2 = edge.Target.View.Circle.Margin.Top + 5,
                Stroke = new SolidColorBrush { Color = Colors.Green },
                StrokeThickness = 4,
            };

            Label = new Label
            {
                Content = edge.Weight,
            };
        }
    }
}
