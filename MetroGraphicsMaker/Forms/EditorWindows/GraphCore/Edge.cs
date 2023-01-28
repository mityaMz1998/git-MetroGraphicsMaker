using System;
using System.Windows;
using linii_graph.View;

namespace WpfApplication1.Forms.EditorWindows.GraphCore
{
    public class Edge : Window
    {
        /// <summary>
        /// Вершина, из которой выходит ребро.
        /// </summary>
        public Node Source { get; private set; }

        /// <summary>
        /// Вершина, в которую заходит ребро.
        /// </summary>
        public Node Target { get; private set; }

        /// <summary>
        /// Граф, которому принадлежит ребро.
        /// </summary>
        public Graph Graph { get; private set; }

        /// <summary>
        /// Признак направленности ребра
        /// </summary>
        public Boolean IsNavigated;

        /// <summary>
        /// Графическое представление данного ребра.
        /// </summary>
        public EdgeView View { get; private set; }

        public Edge(Node source, Node target, Graph graph = null, Boolean isNavigated=false)
        {
            if (source == null)
                throw new ArgumentNullException("source");


            if (target == null)
                throw new ArgumentNullException("target");

            Source = source;
            Source.Edges.Add(this);

            Target = target;
            Target.Edges.Add(this);

            if (graph != null)
                Graph = graph;

            View = new EdgeView(this);

            IsNavigated = isNavigated;
        }

        public Double Weight { get; private set; }
    }
}
