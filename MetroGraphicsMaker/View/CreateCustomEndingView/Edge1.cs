using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using linii_graph.View;
using WpfApplication1.Forms.EditorWindows.GraphCore;

namespace linii_graph
{
    public class Edge1 : UIElement
    {
        public Node1 SourceNode;

        public Node1 TargetNode;

        protected Pen drawPen = new Pen(Brushes.Green, 5);

        /// <summary>
        /// Граф, которому принадлежит ребро.
        /// </summary>
        public Graph Graph;

        /// <summary>
        /// Признак направленности ребра
        /// </summary>
        public Boolean IsNavigated;
 
        public Edge1(Node1 source, Node1 target)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            if (target == null)
                throw new ArgumentNullException("target");

            SourceNode = source;
            SourceNode.InsertEdge1s.Add(this);

            TargetNode = target;
            TargetNode.InsertEdge1s.Add(this);
        }

        public Edge1(Node1 source, Node1 target, Graph graph = null, Boolean isNavigated=false)
        {
            if (source == null)
                throw new ArgumentNullException("source");


            if (target == null)
                throw new ArgumentNullException("target");

            SourceNode = source;
            SourceNode.InsertEdge1s.Add(this);

            TargetNode = target;
            TargetNode.InsertEdge1s.Add(this);

            if (graph != null)
                Graph = graph;
            Graph.Edges1.Add(this);

 //           View = new EdgeView(this);

            IsNavigated = isNavigated;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            drawingContext.DrawLine(drawPen, SourceNode.CenterPoint, TargetNode.CenterPoint);
        }
    }
}
