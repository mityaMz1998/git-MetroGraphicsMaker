using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using Exceptions;
using linii_graph;
using Microsoft.Win32;

namespace WpfApplication1.Forms.EditorWindows.GraphCore
{
    /// <summary>
    /// Interaction logic for CreateCustomEndingNew.xaml
    /// </summary>
    public partial class CreateCustomEndingNew : Window
    {
        private const string BinDirectory = @"\bin";
        private const string NightPlacesDirectory = @"\Save Directory\Night Places";
        private Graph graph;

        private MyCanvas1 canvas1;

        private Decoder decoder;

        private BackgroundWorker bwLoader = new BackgroundWorker();
        public CreateCustomEndingNew()
        {
            InitializeComponent(); //инициализация формы

            ScaleSlider.Minimum = 1;
            ScaleSlider.Maximum = 10;

            for (var value = ScaleSlider.Minimum; value <= ScaleSlider.Maximum; value++)
                ScaleSlider.Ticks.Add(value);

            graph = new Graph(); //создания экземпляра класса управления

            Init();

            /*var serif = new HourSerif(12, 30, 45, 150);
            Canvas.SetLeft(serif, 50);
            Canvas.SetTop(serif, 20);
            //myCanvas.Children.Add(serif); */
        }

        void Init()
        {
            //ВГ
            //var swnd = new DlgWindow1();
            //swnd.ShowDialog();

            // DlgWindow wnd = new DlgWindow();
            // wnd.ShowDialog();

            string pathToMagicDirectory = AppDomain.CurrentDomain.BaseDirectory;
            //@"F:\Work\Metro\MetroGraphicsMaker_2_0\Documents\SaveDirectory";
            //Environment.CurrentDirectory;
            int positionBinDirectory = pathToMagicDirectory.IndexOf(BinDirectory);
            if (positionBinDirectory > 0)
            {
                pathToMagicDirectory = pathToMagicDirectory.Substring(0, pathToMagicDirectory.IndexOf(BinDirectory)) + NightPlacesDirectory + "\\";
            }

            var extentionalPattern = "*.jpg";

            bwLoader.WorkerReportsProgress = true;
            bwLoader.DoWork += bwLoader_DoWork;
            bwLoader.ProgressChanged += bwLoader_ProgressChanged;
            bwLoader.RunWorkerAsync(Directory.GetFiles(pathToMagicDirectory, extentionalPattern));
            Show();
        }

        private void bwLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var tmpImage = e.UserState as BitmapImage;
            if (tmpImage == null)
                return;

            var buttonLength = 100;
            var toolTipLength = 200;

            var tmpButton = new Button
            {
                Tag = Path.ChangeExtension(tmpImage.UriSource.ToString(), ".xml"),
                Name = "btn1",
                Height = buttonLength,
                Width = buttonLength,
                Background = Brushes.LimeGreen,
                Content = new Image { Source = tmpImage },
                ToolTip = new ToolTip
                {
                    Background = Brushes.LightGreen,
                    Width = toolTipLength,
                    Height = toolTipLength,
                    Content = new Image { Source = tmpImage }
                }
            };
            tmpButton.Click += previewButton_Click;
            ButtonsWrapPanel.Children.Add(tmpButton);
        }

        private void previewButton_Click(object sender, RoutedEventArgs e)
        {
            var senderButton = sender as Button;
            if (senderButton == null)
                return;

            //DlgWindow wnd = new DlgWindow();
            // wnd.ShowDialog();
            //MessageBox.Show("jdddk");
            {
                //  MessageBox.Show(senderButton.Tag.ToString());
                //  LoadFromXml(senderButton.Tag.ToString());

                var tmpLoader = new IOXml();
                tmpLoader.LoadFromXmlNew(senderButton.Tag.ToString());
                foreach (var node in tmpLoader.Nodes)
                    GraphCanvas.Children.Add(node);

                foreach (var edge in tmpLoader.Edges)
                    GraphCanvas.Children.Add(edge);

                GraphCanvas.InvalidateVisual();
                graph = null;
                graph = new Graph(tmpLoader.Nodes, tmpLoader.Edges);
            }
        }

        private void bwLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            var pathes = e.Argument as string[];
            if (pathes == null)
                return;

            foreach (var path in pathes)
            {
                //Thread.Sleep(2000);                

                var src = new BitmapImage { CacheOption = BitmapCacheOption.OnLoad };
                src.BeginInit();
                src.UriSource = new Uri(path, UriKind.Relative);
                src.EndInit();
                src.Freeze();

                bwLoader.ReportProgress(0, src);
            }
        }
        private void ButtonBase_Click(object sender, RoutedEventArgs e)
        {
            GraphCanvas.AddEdge();
        }
        #region Old code
        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    drawingContext.DrawEllipse(isSelected ? selectedFillBrush : defaultFillBrush, drawPen, CenterPoint, radius,
        //    radius);
        //    drawingContext.DrawText(new FormattedText("" + this._id, System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("Times New Roman Bold"), 15, Brushes.Maroon), new Point(CenterPoint.X + 5, CenterPoint.Y - 25));
        //}

        /// <summary>
        /// обработка двойного щелчка на форме, создания дочерних элементов формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /* private void Window_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (!graph.editing) 
                return;

            graph.AddNode(e.GetPosition(myCanvas));
            myCanvas.Children.Add(graph.Nodes.Last().View.Circle);
       //     graph.AddNode(e.GetPosition(grid_main_window));
          //  grid_main_window.Children.Add(graph.Nodes.Last().View.Circle);
            //NodesComboBox.Items.Add(graph.Nodes.Last().View.Circle.Name);
            //NodesComboBox.SelectedIndex = graph.Nodes.Count - 1;
        }

        */
        #endregion
        /// <summary>
        /// метод сочетания вершин (создание ребра), создание дочерних элементов формы
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>

        private void BuildButton_Click(object sender, RoutedEventArgs e)
        {
            if (graph.SourceNode != null && graph.TargetNode != null)
            {
                var lastEdge = graph.Edges.Last();
                myCanvas.Children.Add(lastEdge.View.Line);
                //grid_main_window.Children.Add(lastEdge.View.Line);

                Panel.SetZIndex(lastEdge.View.Line, -1);
                // MessageBox.Show(lastEdge.View.Line.ToString());
                graph.enable_conect = true;

                graph.SourceNode.View.Unselect();

                graph.SourceNode = null;
                graph.TargetNode.View.Unselect();
                graph.TargetNode = null;
            }
            else
            {
                MessageBox.Show("Нужно выделить 2 вершины!");
            }
        }

        /// <summary>
        /// очищение окна для создания графов, активация кнопок загрузки, создание рОебер
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, RoutedEventArgs e)
        {
            BuildButton.IsEnabled = true;

            // myCanvas.Children.Clear();
            GraphCanvas.Children.Clear();

            //grid_main_window.Children.RemoveRange(1, grid_main_window.Children.Count);
            graph = new Graph();
            // NodesComboBox.Items.Clear();
        }

        public void LoadFromXml(String filename)
        {
            var root = XDocument.Load(filename).Element("graph");
            var nodes = root.Element("nodes").Elements();

            //var n = nodes.Select(el => new Node1
            //{
            //    Id = Convert.ToInt32(el.Attribute("id").ToString()), 
            //    CenterPoint = new Point(
            //        Convert.ToDouble(el.Element("X").Value), 
            //        Convert.ToDouble(el.Element("Y").Value))
            //}).ToList();

            var nodeList = new List<Node1>();
            foreach (var nodeElement in nodes)
            {
                var id = Convert.ToInt32(nodeElement.Attribute("id").Value);
                var cp = new Point(
                    Convert.ToDouble(nodeElement.Element("X").Value),
                    Convert.ToDouble(nodeElement.Element("Y").Value));
                nodeList.Add(new Node1 { Id = id, CenterPoint = cp });
            }
        }

        private void SaveToXml(String filename)
        {
            var root = new XElement("graph");
            var nodes = new XElement("nodes");
            nodes.SetAttributeValue("maxNodeId", Node1.MaxId);
            foreach (var node in GraphCanvas.Children.OfType<Node1>())
            {
                var nodeElement = new XElement("node");
                nodeElement.SetAttributeValue("id", node.Id);
                nodeElement.Add(new XElement("X", node.CenterPoint.X));
                nodeElement.Add(new XElement("Y", node.CenterPoint.Y));
                nodeElement.Add(new XElement("isBossNode", node.isBossNode));
                nodeElement.Add(new XElement("type", node.Type));
                nodes.Add(nodeElement);
            }

            var edges = new XElement("edges");
            foreach (var edge in GraphCanvas.Children.OfType<Edge1>())
            {
                var edgeElement = new XElement("edge");
                edgeElement.SetAttributeValue("sourceId", edge.SourceNode.Id);
                edgeElement.SetAttributeValue("targetId", edge.TargetNode.Id);
                edgeElement.SetAttributeValue("isNavigated", edge.IsNavigated);
                edges.Add(edgeElement);
            }
            root.Add(nodes);
            root.Add(edges);
            root.Save(filename);
        }

        private void SaveForGraphviz(string filename)
        {
            #region Old code
            //var tree = new XElement("tree");
            //var edges = new XElement("edges");
            //foreach (var edge in GraphCanvas.Children.OfType<Edge1>())
            //{
            //    var edgeElement = new XElement("edges");
            //    edgeElement.SetAttributeValue("N", edge.SourceNode.Id);
            //    edgeElement.SetAttributeValue("Nt", edge.TargetNode.Id);
            //    edges.Add(edgeElement);
            //}
            //tree.Add(edges);
            //tree.Save(filename);     

            //string text = "This is a test";
            //File.WriteAllText("This is a test",text);
            #endregion
            foreach (var edge in GraphCanvas.Children.OfType<Edge1>())
            {
                string source = edge.SourceNode.Id.ToString();
                string target = edge.TargetNode.Id.ToString();
            }

            using (StreamWriter writer = new StreamWriter(filename))
            {
                writer.WriteLine("digraph " + filename + "{");
                writer.WriteLine("node [style = filled , fillcolor = yellow] ;");
                foreach (var edge in GraphCanvas.Children.OfType<Edge1>())
                {
                    string source = edge.SourceNode.Id.ToString();
                    string target = edge.TargetNode.Id.ToString();
                    if (edge.IsNavigated == true)
                    {
                        writer.WriteLine("N" + source + " -> " + "N" + target + " ;");
                    }
                    else
                    {
                        writer.WriteLine("N" + source + " -- " + "N" + target + " ;");
                    }
                    //writer.Write(source);
                    //writer.WriteLine("word 2");
                    //writer.WriteLine("Line");
                }
                writer.WriteLine("}");
            }
        }

        private void TakeDrawingScreenshot_Click(object sender, RoutedEventArgs e)
        {
#region Old code
            /*
                decoder = new Decoder
                {
                    NodesCount = canvas1.Nodes1.Count,
                    //EdgesCount = graph.Edges.Count
                };
                decoder.Initialize();
                decoder.NodeCoordinates = graph.Nodes.Select(node => node.View.Center).ToArray();
                */
            //GraphCanvas.Nodes1.Select(n => n.CenterPoint).ToArray();

            // decoder.NodeCoordinates = canvas1.Nodes1.Select(node => node.)
            // decoder.NodeCoordinates = canvas1.Nodes1.Select(nodes => nodes.);
            //decoder.SourceNodeIdentifiers = graph.Edges.Select(edge => edge.Source.Id).ToArray();
            //decoder.TargetNodeIdentifiers = graph.Edges.Select(edge => edge.Target.Id).ToArray();           
            // MessageBox.Show(decoder.NodesCount.ToString());
            //string NightPlacesDirectory = @"\Save Directory\Night Places";
            #endregion
            var pathSaveDirectory = AppDomain.CurrentDomain.BaseDirectory;
            int positionPathSaveDirectory = pathSaveDirectory.IndexOf(BinDirectory);
            if (positionPathSaveDirectory > 0)
            {
                pathSaveDirectory = pathSaveDirectory.Substring(0, positionPathSaveDirectory) + NightPlacesDirectory;
            }
            var saveBinaryDialog = new SaveFileDialog
            {
                DefaultExt = ".xml",
                FileName = "Your_path",
                Filter = "Your_path (*.xml)|*.xml",
                InitialDirectory = pathSaveDirectory,
            };

            var saveBinareDialog1 = new SaveFileDialog
            {
                DefaultExt = ".txt",
                FileName = "For_Graviz",
                Filter = "For_Graviz (*.txt)|*.txt",
                InitialDirectory = pathSaveDirectory,
            };
            var result1 = saveBinareDialog1.ShowDialog();
            if (result1.HasValue && result1.Value)
            {
                var a = GraphCanvas.Nodes1;
                var defaultTxtName = Path.GetFileName(saveBinareDialog1.FileName);
                SaveForGraphviz(defaultTxtName);
            }

            var result = saveBinaryDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var a = GraphCanvas.Nodes1;
                var DefaultXmlName = Path.GetFileName(saveBinaryDialog.FileName);
                SaveToXml(DefaultXmlName);
#region Old code
                /*
                var node = new XElement("Decoder",
                    new XElement("NodesCount", decoder.NodesCount),
                    new XElement("EdgesCount", decoder.EdgesCount),
                    new XElement("NodeCoordinates", decoder.NodeCoordinates
                        .Select(p => new XElement("StartPoint",
                            new XElement("X", p.X),
                            new XElement("Y", p.Y)))
                        ),
                    new XElement("SourceNodeIdentifiers", decoder.SourceNodeIdentifiers
                        .Select(p => new XElement("int", p))),
                    new XElement("TargetNodeIdentifiers", decoder.TargetNodeIdentifiers
                        .Select(p => new XElement("int", p)))
                    );
                node.Save(DefaultXmlName);
                */

                /*XmlSerializer serializer = new XmlSerializer(typeof(Decoder));
                using (var stream = new FileStream(Path.GetFullPath(saveBinaryDialog.FileName), FileMode.Create, FileAccess.Write))
                {
                 //new BinaryFormatter().Serialize(stream, decoder);
                    serializer.Serialize(stream,decoder);
                }*/
                #endregion
                var DefaultPicName = Path.GetFileNameWithoutExtension(saveBinaryDialog.FileName);
                var DefaultPicExt = ".jpg";

                using (
                    var binaryWriter =
                        new BinaryWriter(new FileStream(DefaultPicName + DefaultPicExt, FileMode.Create,
                            FileAccess.ReadWrite)))
                {
                    binaryWriter.Write(GraphCanvas.GetJpgImage(ScaleSlider.Value, 10));
                }
            }
#region Old code
            /* var saveImageDialog = new SaveFileDialog
            {
                DefaultExt = ".jpg",
                FileName = "Your_path",
                Filter = "Изображения|*.png;*.jpeg;*.jpg;*.bmp;*.gif;*.tiff;*.wdp",
                InitialDirectory = @"C:\Nodes\linii_graph\linii_graph\bin\Debug",
            };

            if (saveImageDialog.ShowDialog().Value)
            {
                using (var binaryWriter = new BinaryWriter(new FileStream(saveImageDialog.FileName, FileMode.Create, FileAccess.ReadWrite)))
                {
                    binaryWriter.Write(grid_main_window.GetJpgImage(ScaleSlider.Value, 90));
                 
                }
            }  */
            #endregion
        }

        public void DrawGraph(Graph drawnGraph)
        {
            {
                GraphCanvas.Children.Clear();
                foreach (var node in drawnGraph.Nodes1)
                    GraphCanvas.Children.Add(node);

                foreach (var edge in drawnGraph.Edges1)
                    GraphCanvas.Children.Add(edge);

                GraphCanvas.InvalidateVisual();
            }
        }

        private void CreateTree_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var createdTree = new Graph(graph, false);

                DrawGraph(createdTree);
            }
            catch (TheGraphIsNotFeasible ane)
            {
                switch (ane.State)
                {
                    case (GraphState.WithOutWhite):
                        MessageBox.Show("На графе не указана точка выхода составов");
                        break;
                }
            }
        }

        private void CreateBossNode_OnClick(object sender, RoutedEventArgs e)
        {
            Boolean isBossNodeNotCreate = true;
            foreach (var node in GraphCanvas.Children.OfType<Node1>())
            {
                if (isBossNodeNotCreate && node.IsSelected)
                {
                    node.isBossNode = node.IsSelected;
                    node.Type = Node1.NodeType.White;
                    isBossNodeNotCreate = false;
                }
                else
                {
                    node.isBossNode = false;
                    node.Type = Node1.NodeType.Yellow;
                }
                node.InvalidateVisual();
            }
        }

        private void DenseGraph_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var densedGraph = new Graph(graph, true);

                DrawGraph(densedGraph);
            }
            catch (TheGraphIsNotFeasible ane)
            {
                switch (ane.State)
                {
                    case (GraphState.WithOutWhite):
                        MessageBox.Show("На графе не указана точка выхода составов");
                        break;
                }
            }
        }

        private void DeleteGreenNodes_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                graph.DeleteGreenNodesFromGraph();
                DrawGraph(graph);
            }
            catch (TheGraphIsNotFeasible ane)
            {
                MessageBox.Show("!!!");
            }
        }
    }
}
