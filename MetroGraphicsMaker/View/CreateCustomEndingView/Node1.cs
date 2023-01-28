using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Collections.Generic;
using View;
using View.CreateCustomEndingView;
using System.Globalization;

namespace linii_graph
{
    public class NodeEventArgs : EventArgs
    {
        public Boolean IsSelected { get; set; }
        public Boolean IsMoved { get; set; }

        bool captured = false;

        public NodeEventArgs(Boolean flag)
        {
            IsSelected = flag;
            IsMoved = flag;
        }
    }

    public class Node1 : FrameworkElement
    {
        public enum NodeType : byte
        {
            White,
            Yellow,
            Green,
            Gray,
            Black
        }

        public List<Edge1> InsertEdge1s = new List<Edge1>(0);

        protected Int32 _id;

        public static Int32 MaxId { get; set; }

        public Int32 Id
        {
            get { return _id; }
            set { _id = value == 0 ? ++MaxId : value; }
        }

        //public Point CenterPoint { get; protected set; }

        public Point CenterPoint { get; set; }
        //public String stationName { get; set {stationName= }; }
        protected Double radius = 7;
        protected Boolean isSelected;
        protected Boolean isMoved;

        public Boolean IsMoved
        {
            get { return isMoved; }
            set
            {
                isMoved = value;
                //OnSelect(this, new NodeEventArgs(value));
            }
        }

        public Boolean IsSelected
        {
            get { return isSelected; }
            set
            {
                isSelected = value;
            }
        }

        protected Brush defaultFillBrush = Brushes.Green;

        protected Brush selectedFillBrush = Brushes.Orange;
        protected Brush selected1FillBrush = Brushes.Red;

        protected Pen drawPen = new Pen(Brushes.Black, 1);
        protected Brush BossBrush = Brushes.Red;
        protected Pen PenBossNode = PenGiver.CreatePen(PenGiver.ColorPenForTrainPath.Red);

        public delegate void SelectMethod(object sender, NodeEventArgs args);

        public event SelectMethod OnSelect;

        protected Point currentPoint;

        protected Point HPoint;

        protected UInt32 stationCode;

        protected MyCanvas1 parentCanvas;
        protected Core.Station station;

        public Boolean isBossNode = false;

        /// <summary>
        /// Вершина графа, от которой порождена вершина дерева
        /// </summary>
        public Node1 ParentNode = null;

        /// <summary>
        /// Тип вершины
        /// </summary>
        public NodeType Type = NodeType.Yellow;

        private string pointerName; // =MouseMenuForNode1.getPointerName();

        public string PointerName
        {
            get { return pointerName; }
            set
            {
                pointerName = value;
                InvalidateVisual();
            }
        }

        public Node1()
        {
        }

        public Node1(Point center, MyCanvas1 parent)
        {
            CenterPoint = center;
            // MessageBox.Show(CenterPoint+"");
            parentCanvas = parent;
            MouseDown += OnMouseDown;
            //MouseUp += OnMouseUp;

            //MouseMove += OnMouseMove;
            //MouseRightButtonDown += shape_MouseRightButtonDown;
            MouseRightButtonDown += shape_MouseRightButtonDown;
            MouseMove += shape_MouseMove;
            MouseRightButtonUp += shape_MouseRightButtonUp;
        }

        public Node1(Int32 id, Node1 parentNode)
        {
            Id = id;
            ParentNode = parentNode;
            CenterPoint = ParentNode.CenterPoint;
        }

        bool captured = false;
        bool kap = false;
        double x_shape, x_canvas, y_shape, y_canvas;

        // <local:MyCanvas1 x:Name="GraphCanvas"

        private void shape_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            #region Old code
            //MessageBox.Show(currentPoint+"d");
            /*source = (UIElement)sender;
            Mouse.Capture(source);
            captured = true;
            //x_shape = MyCanvas1.GetLeft(source);
            x_shape = MyCanvas1.GetLeft(source);
            x_canvas = e.GetPosition(parentCanvas).X;
            y_shape = MyCanvas1.GetTop(source);
           // y_canvas = e.GetPosition(GraphCanvas).Y;
            y_canvas = e.GetPosition(parentCanvas).Y;*/
            #endregion
            Mouse.Capture((Node1)sender);
            captured = true;
            //InvalidateVisual();
            #region Old code            
            //x_shape = MyCanvas1.GetLeft(Point);
            //x_shape = MyCanvas1.GetLeft();
            //x_canvas = e.GetPosition(parentCanvas).X;
            //y_shape = MyCanvas1.GetTop(source);
            // y_canvas = e.GetPosition(GraphCanvas).Y;
            //y_canvas = e.GetPosition(parentCanvas).Y;
            //currentPoint = e.GetPosition(parentCanvas);
            // MessageBox.Show(currentPoint + "");
            //MessageBox.Show(captured+"");
            //CenterPoint = new Point(x_canvas, y_canvas);
            //MessageBox.Show(CenterPoint + "GFGGF");
            /*x_shape = MyCanvas1.GetLeft(CenterPoint);
            MessageBox.Show("1"+x_shape);
            x_canvas = e.GetPosition(parentCanvas).X;
            y_shape = MyCanvas1.GetTop(source);
           // y_canvas = e.GetPosition(GraphCanvas).Y;
            y_canvas = e.GetPosition(parentCanvas).Y;*/
            #endregion

        }

        private void shape_MouseMove(object sender, MouseEventArgs e)
        {
            if (captured)
            {
                #region Old code
                //MessageBox.Show("Move!");
                //currentPoint.X = currentPoint.X;
                //currentPoint.Y = currentPoint.Y;
                //x_shape += x - currentPoint.X;
                //MessageBox.Show(x_shape + "");
                //MyCanvas1.SetLeft(parentCanvas, x_shape);
                //x_canvas = x-100;
                // y_shape += y - currentPoint.Y;
                //MyCanvas1.SetTop(parentCanvas, y_shape);
                //y_canvas = y-100;
                //CenterPoint = new Point(CenterPoint.X-currentPoint.X, CenterPoint.Y  - currentPoint.Y);
#endregion
                x_canvas = e.GetPosition(parentCanvas).X;
                //y_shape = MyCanvas1.GetTop(source);
                // y_canvas = e.GetPosition(GraphCanvas).Y;
                y_canvas = e.GetPosition(parentCanvas).Y;
#region
                //currentPoint = e.GetPosition(parentCanvas);
                // MessageBox.Show(currentPoint + "");
                //MessageBox.Show(captured+"");
#endregion
            }
        }

        private void shape_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            Mouse.Capture(null);
            captured = false;
            kap = true;
            //MessageBox.Show("not");
            // CenterPoint = new Point(CenterPoint.X - 100, CenterPoint.Y);
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {

                if (x_canvas < 0)
                {
                    x_canvas = 0;
                }
                if (x_canvas > parentCanvas.Height)
                {
                    x_canvas = parentCanvas.Height;
                }
                if (y_canvas < 0)
                {
                    y_canvas = 0;
                }
                if (y_canvas > parentCanvas.Height)
                {
                    y_canvas = parentCanvas.Height;
                }

                CenterPoint = new Point(x_canvas, y_canvas);
                //MessageBox.Show(CenterPoint + "GFGGF");
                IsSelected = !IsSelected;
                InvalidateVisual();
                if (InsertEdge1s.Count > 0)
                {
                    InsertEdge1s.ForEach(thEdge1 => thEdge1.InvalidateVisual());
                }
            }
            else
            {
                ContextMenu = new MouseMenuForNode1 { PlacementTarget = this };
            }
        }
        #region Old code
        /* private void OnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
         {

             if (mouseButtonEventArgs.LeftButton == MouseButtonState.Released)
             {
                 //currentPoint = mouseButtonEventArgs.GetPosition(parentCanvas);
                 HPoint = mouseButtonEventArgs.GetPosition(parentCanvas);
                 IsMoved = !IsMoved;
                 InvalidateVisual();

                 mouseButtonEventArgs.Handled = true;
                 if (mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed)
                 {
                     isMoved = true;
                     currentPoint = mouseButtonEventArgs.GetPosition(parentCanvas);
                     IsSelected = !IsSelected;
                     InvalidateVisual();
                 }

             }

         }

         */

        /*private void OnMouseDown(object sender, MouseButtonEventArgs e)
         {
             currentPoint = e.GetPosition(parentCanvas);
             IsSelected = !IsSelected;
             InvalidateVisual();

             /*if (e.LeftButton == MouseButtonState.Pressed)
             {
                 Mouse.Capture((Node1) sender);
                 HPoint = e.GetPosition(parentCanvas);
                 //MessageBox.Show("=" + CenterPoint);
                 MessageBox.Show("=" + isMoved);
             }
             */

        //}

        /* private void OnMouseMove(object sender, MouseButtonEventArgs e)
         {
             HPoint = e.GetPosition(parentCanvas);
             IsMoved = !IsMoved;
             InvalidateVisual();

             if (e.RightButton == MouseButtonState.Pressed)
             {
                 Mouse.Capture((Node1) sender);
                 HPoint = e.GetPosition(parentCanvas);
                 //MessageBox.Show("=" + CenterPoint);
                 MessageBox.Show("=" + isMoved);
             }


         }
         */

        //перемещение вершины вместе с кругом

        /* private void OnMouseMove(object sender, MouseButtonEventArgs mouseButtonEventArgs)
         {
             //if (e.RightButton == MouseButtonState.Pressed)
             if (mouseButtonEventArgs.RightButton == MouseButtonState.Pressed)
             {

                 //currentPoint = mouseButtonEventArgs.GetPosition(parentCanvas);
                 HPoint = mouseButtonEventArgs.GetPosition(parentCanvas);
                 IsMoved = !IsMoved;
                 InvalidateVisual();

                 mouseButtonEventArgs.Handled = true;
                 if (mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed)
                 {
                     isMoved = true;
                     currentPoint = mouseButtonEventArgs.GetPosition(parentCanvas);
                     IsSelected = !IsSelected;
                     InvalidateVisual();
                 }


             }
                 /*var myNode = (Node1) sender;
                 double mouseX = e.GetPosition(parentCanvas).X, mouseY = e.GetPosition(parentCanvas).Y;
                 Point currentPoint = e.GetPosition(parentCanvas);*/
        //if ( //&& (mouseX < this.) && (mouseY > 0) && (mouseY < Image.ActualHeight)
        //Math.Abs(currentPoint.X - HPoint.X) > SystemParameters.MinimumHorizontalDragDistance &&
        //Math.Abs(currentPoint.Y - HPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
        //если мышка находится внутри области для рисования
        //и переместилась больше мин значения
        // {
        //var x = myNode.
        //var y = myEllipse.Margin.Top;

        //пересоздаём вершину в новом месте
        //пересоздаём круг в новом месте

        // var x = myEllipse.Margin.Left;
        //var y = myEllipse.Margin.Top;

        //currentPoint = e.GetPosition(parentCanvas);

        //isMoved = true;
        // MessageBox.Show("Drag");
        //MessageBox.Show("!" + isMoved);

        // }
        //пересоздаём вершину в новом месте
        //пересоздаём круг в новом месте
        // myEllipse.Margin = new Thickness(e.GetPosition(Image).X, e.GetPosition(Image).Y, 0, 0);


        //}
        // }
        //если отпустили кнопку - отменяем захват мышки
        //else
        //Mouse.Capture(null);

        //}
        //}

        /*
            var realizedPoint = mouseButtonEventArgs.GetPosition(parentCanvas);
            if (realizedPoint.Equals(currentPoint))
                return;

            CenterPoint = new Point(CenterPoint.X + realizedPoint.X - currentPoint.X, CenterPoint.Y + realizedPoint.Y - currentPoint.Y);
            InvalidateVisual();
            mouseButtonEventArgs.Handled = true;
           */

        /*
                private void shape_MouseRightButtonDown (object sender, MouseButtonEventArgs mouseButtonEventArgs)
                {
                    currentPoint = mouseButtonEventArgs.GetPosition(parentCanvas);
                    IsSelected = !IsSelected;
                    InvalidateVisual();




                    mouseButtonEventArgs.Handled = true;
                    if (mouseButtonEventArgs.RightButton == MouseButtonState.Pressed)
                    {

                        currentPoint = mouseButtonEventArgs.GetPosition(parentCanvas);
                        IsSelected = !IsSelected;
                        InvalidateVisual();
                    }


                }


                 */

        /* private void OnMouseUp(object sender, MouseButtonEventArgs mouseButtonEventArgs)
         {

             UIElement currUIEl = sender as UIElement;
             Point currPos = mouseButtonEventArgs.GetPosition(currUIEl);

             //MessageBox.Show("cursorPosition" + currPos.X);
             //Xlabel.Content = "X= " + currPos.X.ToString();
             //Ylabel.Content = "Y= " + currPos.Y.ToString();
                
             //HPoint = mouseButtonEventArgs.GetPosition(currUIEl);

             IsMoved = !IsMoved;
             InvalidateVisual();


            // MessageBox.Show("BA!");

             mouseButtonEventArgs.Handled = true;
             if (mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed                                        )
             {

                 if (mouseButtonEventArgs.ClickCount == 2)
                 {
                     MessageBox.Show("you double-clicked");
                 }
                 HPoint.X=currPos.X+10; 
                 HPoint.Y = currPos.Y + 10;
                 IsMoved = !IsMoved;
                 InvalidateVisual();
             }
         }
        */
        #endregion
        private void OnMouseDown(object sender, MouseButtonEventArgs mouseButtonEventArgs)
        {
            currentPoint = mouseButtonEventArgs.GetPosition(parentCanvas);
            IsSelected = !IsSelected;
            InvalidateVisual();

            #region Old code
            /* //mouseButtonEventArgs.Handled = true;
            if (mouseButtonEventArgs.LeftButton == MouseButtonState.Pressed)
            {
                isMoved = true;
                currentPoint = mouseButtonEventArgs.GetPosition(this);
            }*/
            #endregion
        }
        #region Old code
        /*public void OnMouseMove(object sender, MouseEventArgs e)
         {            
         }
         */
        #endregion
        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawEllipse(
                isSelected ? selectedFillBrush : defaultFillBrush, 
                drawPen, 
                CenterPoint, 
                radius, 
                radius);

            drawingContext.DrawText(
                MakeLabel(), 
                new Point(CenterPoint.X - 45, CenterPoint.Y - 25));

            if (isBossNode)
            {
                drawingContext.DrawEllipse(BossBrush, PenBossNode, CenterPoint, radius / 4, radius / 4);
            }
            //drawingContext.DrawEllipse(isMoved ? selected1FillBrush : defaultFillBrush, drawPen, HPoint, radius, radius);
        }

        protected FormattedText MakeLabel()
        {
            if (PointerName == null)
                PointerName = "";

            return new FormattedText(
                PointerName,
                CultureInfo.GetCultureInfo("en-us"),
                FlowDirection.LeftToRight,
                new Typeface("Times New Roman Bold"),
                12,
                Brushes.Maroon);
        }

    }
}

