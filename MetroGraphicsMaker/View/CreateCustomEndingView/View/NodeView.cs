using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WpfApplication1.Forms.EditorWindows.GraphCore;

namespace linii_graph.View
{
    public class NodeView : Control
    {
        /// <summary>
        /// Коррдинаты центра.
        /// </summary>
        public Point Center { get; private set; }

        public Ellipse Circle { get; private set; }

        public Label Label { get; private set; }

        private Node node;

        int a = 0;

        public Boolean IsSelected { get; private set; }
    

        public NodeView(Node parentNode, Point center)
        {
            if (parentNode == null)
                throw new ArgumentNullException("parentNode");

            node = parentNode;

            Center = center;

           
            
            IsSelected = false;

            var name = "n" + node.Id;
            Circle = new Ellipse
            {   
                
                Height = 10,
                Width = 10,
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Stroke = new SolidColorBrush(Colors.Black),
                StrokeThickness = 0,
                Fill = Brushes.LimeGreen,
                Margin = new Thickness(center.X - 5, center.Y - 5, 0, 0),
                Name = name,
                ToolTip = name
            };


            
            Circle.MouseDown += CircleOnMouseDown;
            Circle.MouseRightButtonDown += CircleOnMouseRightButtonDown;


            Label = new Label
            {
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Left,
                Width = 32,
                Height = 28,
                Name = "l" + node.Id,
                Uid = node.Id.ToString(),
                Margin = new Thickness(center.X - 5, center.Y - 5, 0, 0)                
            };

            
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
        }

        private void CircleOnMouseRightButtonDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {

            //if (IsSelected = true)
            {
                node.AddEdge(sender, mouseButtonEventArgs);
                Select();
                
            }

        }

        private void CircleOnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            //throw new NotImplementedException();
            //здесь идет некая идея по реализации отмены выделения
            if (IsSelected = true && Keyboard.IsKeyDown(Key.LeftShift))
            {
                Circle.Fill = Brushes.Red;
            }
            
        }

       
        public void Select()
        {
            IsSelected = true;
            Circle.InvalidateVisual();
            Circle.Fill = Brushes.Blue;            
        }
        

        public void Unselect()
        {
            IsSelected = false;
            Circle.InvalidateVisual();
            Circle.Fill = Brushes.LightGreen;
        }       
    }
}
