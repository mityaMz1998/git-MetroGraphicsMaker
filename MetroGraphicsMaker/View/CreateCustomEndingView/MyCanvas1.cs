using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using View.CreateCustomEndingView;

namespace linii_graph
{
    public class MyCanvas1 : Canvas
    {
        protected Boolean flag = false;

        protected Double LargeStep = 100;

        protected Double StandardStep = 20;

        protected Pen LargePen = new Pen(Brushes.Black, 1.5);

        protected Pen StandardPen = new Pen(Brushes.Gray, 1);

        protected Point StartPoint = new Point();

        public List<Node1> SelectedNodes = new List<Node1>();

        public List<Node1> Nodes1 = new List<Node1>();

       // public List<Edge1> Edges1 = new List<Edge1>();
        


        public MyCanvas1()
        {
            MouseDown += OnMouseDown;
            MouseUp += OnMouseUp;
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            /*if (mouseButtonEventArgs.ClickCount == 2)
                MessageBox.Show("Hello");*/

            
        }

        public void AddEdge()
        {
            var nodes = Children.OfType<Node1>().Where(node => node.IsSelected).ToList();
            if (nodes.Count == 2)
            {
                Children.Add(new Edge1(nodes[0], nodes[1]));
                flag = true;

                if (flag == true)
                {
                    foreach (var node in nodes)
                    {
                        node.IsSelected = false;
                        node.InvalidateVisual();
                    }
                }
            }
        }

        public void AddNode(Node1 node)
        {
            node.Id = 0;
            SetZIndex(node, 1);
            //node.OnSelect += NodeOnSelect;
            Children.Add(node);
           // node.MouseRightButtonDown += Node_MouseRightButtonDown;
            node.InvalidateVisual();
        }

        private void Node_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            //ContextMenu = new MouseMenuForNode1 {PlacementTarget = this};
            //this.InvalidateVisual();         
        }

        /*
private void NodeOnSelect(object sender, NodeEventArgs args)
{
   var senderNode = sender as Node;
   if (senderNode == null)
       return;

   if (args.IsSelected)
       SelectedNodes.Add(senderNode);
   else
       SelectedNodes.Remove(senderNode);

}
*/
        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && e.ClickCount == 2)
            {
                AddNode(new Node1(e.GetPosition(this), this));
            }

          if (e.ChangedButton == MouseButton.Right && e.ClickCount == 1)
            {
                var nodes = Children.OfType<Node1>().Where(node => node.IsSelected);
                foreach (var node in nodes)
                {
                    node.IsSelected = false;
                    node.InvalidateVisual();
                }

                //SelectedNodes.ForEach(node => node.IsSelected = false);
                //SelectedNodes.Clear();
            }

     
            InvalidateVisual();
        }


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
