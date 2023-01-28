using System;
using System.Windows;

namespace linii_graph
{
    [Serializable]
   public class Decoder
    {
        /// <summary>
        /// Колличество вершин графа.
        /// </summary>
        
        public Int32 NodesCount = 0;
        
        /// <summary>
        /// Колличество рёбер графа.
        /// </summary>
        public Int32 EdgesCount = 0;

        /// <summary>
        /// Массив координат вершин.
        /// </summary>
        public Point[] NodeCoordinates;

        public Int32[] SourceNodeIdentifiers;

        public Int32[] TargetNodeIdentifiers;



        /// <summary>
        /// динамическое выделение памяти под хранение графа в файл
        /// </summary>
        public void Initialize()
        {
            NodeCoordinates = new Point[NodesCount];

            SourceNodeIdentifiers = new Int32[EdgesCount];

            TargetNodeIdentifiers = new Int32[EdgesCount];

            
        }
        
    }
}
