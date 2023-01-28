using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Shapes;
using linii_graph.View;

namespace WpfApplication1.Forms.EditorWindows.GraphCore
{
    public class Node : Window
    {
        public enum NodeType
        {
            White,
            Yellow,
            Green,
            Gray,
            Black
        }

        /// <summary>
        /// Идентификатор.
        /// </summary>
        public Int32 Id { get; private set; }

        /// <summary>
        /// Граф, которому принадлежит данная вершина.
        /// </summary>
        public Graph graph;

        /// <summary>
        /// Тип вершины
        /// </summary>
        public NodeType Type;

        /// <summary>
        /// Список ребер, инцидентных данной вершине
        /// </summary>
        public List<Edge> Edges = new List<Edge>(0);
        
        /// <summary>
        /// Графическое представление данной вершины.
        /// </summary>
        public NodeView View { get; private set; }


        
        public Node(Point center, Int32 id, Graph graph = null)
        {
            Id = id;

            View = new NodeView(this, center);

            this.graph = graph;
        }

        public Node(Int32 id)
        {
            Id = id;
        }

        /// <summary>
        /// Создание ребра в графе
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void AddEdge(object sender, RoutedEventArgs e)
        {
            var senderEllipse = sender as Ellipse;
            if (senderEllipse == null)
                return;

            if (!graph.editing || !graph.enable_conect) 
                return;

            if (graph.SourceNode == null)
            {
                graph.SourceNode = this;
                View.Select();
                
            }
            else if (graph.TargetNode == null)
            {
                graph.TargetNode = this;
                View.Select();
                graph.AddEdge();
                graph.enable_conect = false;
                }
        }
    }
}
