using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Exceptions;
using linii_graph;

namespace WpfApplication1.Forms.EditorWindows.GraphCore
{
    public class Graph : Window
    {
        public Boolean enable_conect = true;
        public Boolean editing = true;

        public List<Node> Nodes = new List<Node>();
        public List<Edge> Edges = new List<Edge>();


        public List<Node1> Nodes1 = new List<Node1>();
        public List<Edge1> Edges1 = new List<Edge1>();

        public Node SourceNode = null;
        public Node TargetNode = null;

        /// <summary>
        /// соединения двух вершин
        /// </summary>
        public void AddEdge()
        {
            Edges.Add(new Edge(SourceNode, TargetNode, this));
        }

        public void AddNode(Point center)
        {
            Nodes.Add(new Node(center, Nodes.Count, this));
        }

        /// <summary>
        /// Процедура добавления узла в дерево путем глубокого копирования узла исходного графа
        /// </summary>
        public Node1 AddNodeFromGraphToTree(Node1 graphNode)
        {
            var node = new Node1(Nodes1.Count * 100 + graphNode.Id, graphNode);
            Nodes1.Add(node);
            return node;
        }

        public Node1 AddNodeForDensedGraph(Node1 graphNode)
        {
            var node = new Node1(graphNode.Id, graphNode);
            Nodes1.Add(node);
            return node;
        }
        public Graph()
        {
        }

        public Graph(List<Node1> nodes1, List<Edge1> edges1)
        {
            Nodes1 = nodes1;
            Edges1 = edges1;
        }
        /// <summary>
        /// конструктор - процедура инициализации построения дерева по графу
        /// </summary>
        public Graph(Graph dataGraph, Boolean flgModigyGraph)
        {
            //Перед построением дерева из исходного графа необходимо удалить зеленые вершины с использованием следующего алгоритма.

            //При первом вызове процедуры CreateTree в качестве текущей вершины рассматривается белая вершина исходного графа, а запомненной вершины не существует.
            var whiteNode = dataGraph.Nodes1.Find(nd => nd.Type == Node1.NodeType.White);
            if (whiteNode == null)
                throw new TheGraphIsNotFeasible(Exceptions.GraphState.WithOutWhite);
            if (flgModigyGraph)
            {
                dataGraph.DenseGraph(whiteNode, null, this);
            }
            else
            {
                dataGraph.CreateTree(whiteNode, null, this);
                var listOfNodes1IncidentOnlyOneEdge = Nodes1.FindAll(n => ((n.InsertEdge1s.Count == 1) && (n.ParentNode.Type != Node1.NodeType.White)));
            }
            //Отображение результата
        }
        /// <summary>
        /// Процедура  построения дерева по графу
        /// currentGraphNode - вершина ГРАФА, которую мы добавляем
        /// rememberedTreeNode - вершина ДЕРЕВА, с которой соединяем вновь создаваемые вершины
        /// </summary>
        public void CreateTree(Node1 currentGraphNode, Node1 rememberedTreeNode, Graph newTree)
        {

            //Проверяется условие работы с текущей вершиной белого цвета. Это условие аналогично тому, что  rememberedNode==Null
            //В случае, если текущая вершина белая, происходит создание истока дерева
            //Иначе происходит добавление в дерево вершины, соответствующей текущей вершине графа, как смежной к запомненной вершине дерева
            var currentTreeNode = newTree.AddNodeFromGraphToTree(currentGraphNode);
            Edge1 edge = null;
            {
                try
                {
                    edge = new Edge1(rememberedTreeNode, currentTreeNode, newTree, true);
                }
                catch (ArgumentNullException ane)
                {

                }
            }
            //Происходит фиксация параметров текущей итерации процесса с целью корректного их восстановления после возврата из рекурсивных процедур после достижения граничного условия построения дерева: сохранение информации о текущей серой вершине и текущей окраске вершин.
            Node1.NodeType oldTypeOfCurrentNode = currentGraphNode.Type;

            //Происходит окрашивание текущей вершины графа в серый цвет.
            currentGraphNode.Type = Node1.NodeType.Gray;

            //Организован цикл по всем серым вершинам графа
            Nodes1.ForEach(nd =>
            {
                if (nd.Type == Node1.NodeType.Gray)
                {
                    //Проверяется условие наличия в графе смежных серой желтых вершин
                    var listOfEdgesIncidentWithYellowNode = nd.InsertEdge1s.FindAll(ed =>
                    ((ReferenceEquals(ed.SourceNode, nd) && (ed.TargetNode.Type == Node1.NodeType.Yellow)) ||
                      (ReferenceEquals(ed.TargetNode, nd) && (ed.SourceNode.Type == Node1.NodeType.Yellow))));

                    //if ((listOfEdgesIncidentWithYellowNode == null) || (listOfEdgesIncidentWithYellowNode.Count == 0))
                    ////Происходит окрашивание серой вершин графа в черный цвет
                    //{
                    //    var oldTypeOfAddedNode = nd.Type;
                    //    nd.Type = Node1.NodeType.Black;
                    //}
                    //else
                    //{
                    //Организован цикл по всем желтым вершинам, соседним с текущей серой вершиной
                    listOfEdgesIncidentWithYellowNode.ForEach(ed =>
                    {
                        //Происходит переход к следующему шагу построения дерева. В качестве новой текущей вершины рассматривается текущая желтая вершина, в качестве запомненной вершины – текущая серая.!!!!!!!!!!!!!!!!!!1 
                        CreateTree(ReferenceEquals(ed.SourceNode, nd) ? ed.TargetNode : ed.SourceNode, currentTreeNode, newTree);
                    });

                    //}
                }
            });
            //Происходит восстановление после возврата из рекурсивных процедур после достижения граничного условия построения дерева
            currentGraphNode.Type = oldTypeOfCurrentNode;
            //if (currentGraphNode != null)
            //{
            //    currentGraphNode.Type = oldTypeOfAddedNode;
            //}
        }

        public void DenseGraph(Node1 currentGraphNode, Node1 rememberedDensedGraphNode, Graph densedGraph)
        {

            //Если в «уплотненном графе» не существует вершина, соответствующая текущей, то добавляем ее
            var currentDensedGraphNode = densedGraph.Nodes1.Find(nd => ((nd.Id == currentGraphNode.Id)));
            if (currentDensedGraphNode == null)
            {
                currentDensedGraphNode = densedGraph.AddNodeForDensedGraph(currentGraphNode);
            }
            //Создание в «уплотненном» графе ребра, направленного от запомненной ранее к текущей
            Edge1 edge = null;
            {
                try
                {
                    var ed = currentDensedGraphNode.InsertEdge1s.Find(ed1 => (ed1.SourceNode.Id == rememberedDensedGraphNode.Id));
                    if (ed == null)
                    {
                        edge = new Edge1(rememberedDensedGraphNode, currentDensedGraphNode, densedGraph, true);
                    }
                }
                catch (ArgumentNullException ane)
                {

                }
            }
            //Происходит фиксация параметров текущей итерации процесса с целью корректного их восстановления после возврата из рекурсивных процедур после достижения граничного условия построения дерева: сохранение информации о текущей серой вершине и текущей окраске вершин.
            Node1.NodeType oldTypeOfCurrentNode = currentGraphNode.Type;

            //Происходит окрашивание текущей вершины графа в серый цвет.
            currentGraphNode.Type = Node1.NodeType.Gray;

            //Организован цикл по всем серым вершинам графа
            Nodes1.ForEach(nd =>
            {
                if (nd.Type == Node1.NodeType.Gray)
                {
                    //Проверяется условие наличия в графе смежных серой желтых вершин
                    var listOfEdgesIncidentWithYellowNode = nd.InsertEdge1s.FindAll(ed =>
                    ((ReferenceEquals(ed.SourceNode, nd) && (ed.TargetNode.Type == Node1.NodeType.Yellow)) ||
                      (ReferenceEquals(ed.TargetNode, nd) && (ed.SourceNode.Type == Node1.NodeType.Yellow)))
                      );
                    //Организован цикл по всем желтым вершинам, соседним с текущей серой вершиной
                    listOfEdgesIncidentWithYellowNode.ForEach(ed =>
                    {
                        //Происходит переход к следующему шагу построения дерева. В качестве новой текущей вершины рассматривается текущая желтая вершина, в качестве запомненной вершины – текущая серая.!!!!!!!!!!!!!!!!!!1 
                        DenseGraph(ReferenceEquals(ed.SourceNode, nd) ? ed.TargetNode : ed.SourceNode, currentDensedGraphNode, densedGraph);
                    });
                }
            });
            //Происходит восстановление после возврата из рекурсивных процедур после достижения граничного условия построения дерева
            currentGraphNode.Type = oldTypeOfCurrentNode;
        }

        public void DeleteGreenNodesFromGraph()
        {
            List<Edge1> listOfEdgesIncidentToGreenAndYellowNodes;
            do
            {
                //Находим ребра, инцидентные одной желтой и одной зеленой вершине, неориентированные или ориентированные, для которых зеленая вершина является началом
                listOfEdgesIncidentToGreenAndYellowNodes = Edges1.FindAll(ed =>
                    (((ed.SourceNode.Type == Node1.NodeType.Green) && (ed.TargetNode.Type == Node1.NodeType.Yellow)) ||
                     ((ed.TargetNode.Type == Node1.NodeType.Green) && (ed.SourceNode.Type == Node1.NodeType.Yellow) && !ed.IsNavigated))
                    );
                if (listOfEdgesIncidentToGreenAndYellowNodes.Count() != 0)
                    listOfEdgesIncidentToGreenAndYellowNodes.ForEach(ed =>
                    {
                        Node1 ndGreen, ndYellow;
                        if (ed.TargetNode.Type == Node1.NodeType.Green)
                        {
                            ndGreen = ed.TargetNode;
                            ndYellow = ed.SourceNode;
                        }
                        else
                        {
                            ndGreen = ed.SourceNode;
                            ndYellow = ed.TargetNode;
                        };
                        //Проверяем, что у этого ребра нет кратных ребер
                        var listOfMultipleEdgesincidentToGreenANdYellowNodes =
                            listOfEdgesIncidentToGreenAndYellowNodes.FindAll(ed1 =>
                            (ReferenceEquals(ndYellow, ed1.SourceNode) && ReferenceEquals(ndGreen, ed1.TargetNode)) ||
                         (ReferenceEquals(ndYellow, ed1.TargetNode) && ReferenceEquals(ndGreen, ed1.SourceNode)));
                        if (listOfMultipleEdgesincidentToGreenANdYellowNodes.Count() == 1)
                        {
                            //Вместе с каждой зеленой вершиной удаляется одно ребро
                            Edges1.Remove(ed);
                            ndGreen.InsertEdge1s.Remove(ed);
                            ndYellow.InsertEdge1s.Remove(ed);

                            // Остальные инцидентные зеленой вершине ребра становятся инцидентны той желтой вершине, с которой удаленная зеленая была связана удаленным ребром.
                            ndGreen.InsertEdge1s.ForEach(ed1 =>
                            {
                                ndYellow.InsertEdge1s.Add(ed1);
                                if (ReferenceEquals(ndGreen, ed1.SourceNode))
                                {
                                    ed1.SourceNode = ndYellow;
                                }
                                else
                                {
                                    ed1.TargetNode = ndYellow;
                                };
                            });
                            ndGreen.InsertEdge1s.Clear() ;                           
                            //Удаление зеленой вершины из графа
                            Nodes1.Remove(ndGreen);
                        }
                    });
            } while (listOfEdgesIncidentToGreenAndYellowNodes.Count() != 0);

            //Все кратные ребра заменяются единственными неориентированными
            Edges1.ForEach(ed =>
            {
                var ed2 = Edges1.Find(ed1 =>
                    (ReferenceEquals(ed.TargetNode, ed1.SourceNode) && ReferenceEquals(ed.SourceNode, ed1.TargetNode)));
                if (ed2 != null)
                {
                    Edges1.Remove(ed2);
                    ed.SourceNode.InsertEdge1s.Remove(ed2);
                    ed.TargetNode.InsertEdge1s.Remove(ed2);
                    ed.IsNavigated = false;
                }

            }
                );
        }
    }
}
