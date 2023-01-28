using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;
using linii_graph;
using Microsoft.Win32;

namespace WpfApplication1.Forms.EditorWindows.GraphCore
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    ///  
    /// 

    public partial class CreateCustomEnding : Window
    {
        private Graph graph;
        private MyCanvas1 canvas1;
        private Decoder decoder;
        private BackgroundWorker bwLoader = new BackgroundWorker();

        private const string binDirectory = @"\bin";
        private const string DinamicTailsDirectory = @"\Save Directory\DinamicTails";

        public CreateCustomEnding()
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

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            GraphCanvas.AddEdge();
        }
        
        private void Init()
        {
            //ВГ
            //var swnd = new DlgWindow1();
            //swnd.ShowDialog();            
            // DlgWindow wnd = new DlgWindow();
            // wnd.ShowDialog();

            var pathToMagicDirectory = AppDomain.CurrentDomain.BaseDirectory;
                //Environment.CurrentDirectory;
                //AppDomain.CurrentDomain.BaseDirectory;         
            int positionBinDirectory = pathToMagicDirectory.IndexOf(binDirectory);
            if (positionBinDirectory > 0)
            {
                pathToMagicDirectory = pathToMagicDirectory.Substring(0, positionBinDirectory) + DinamicTailsDirectory +
                                       @"\";
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
                Content = new Image {Source = tmpImage},
                ToolTip = new ToolTip
                {
                    Background = Brushes.LightGreen,
                    Width = toolTipLength,
                    Height = toolTipLength,
                    Content = new Image {Source = tmpImage}
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
                tmpLoader.LoadFromXml(senderButton.Tag.ToString());
                foreach (var node in tmpLoader.Nodes)
                    GraphCanvas.Children.Add(node);

                foreach (var edge in tmpLoader.Edges)
                    GraphCanvas.Children.Add(edge);

                GraphCanvas.InvalidateVisual();
            }
        }

        //DlgWindow.OKBtn_OnClick += previewButton_Click;
        /*
            try
            {
               
                
               /* XmlSerializer serializer = new XmlSerializer(typeof(Decoder));
                using (var files = new FileStream(senderButton.Tag.ToString(), FileMode.Open, FileAccess.ReadWrite))
                {
                   //decoder = (Decoder) new BinaryFormatter().Deserialize(files);
                    decoder = (Decoder) serializer.Deserialize(files);
                }
                */

        //clear all
        // grid_main_window.Children.RemoveRange(1, grid_main_window.Children.Count);

        // GraphCanvas.Children.Clear();

        // graph = new Graph();
        //NodesComboBox.Items.Clear();

        // LoadFromXml(senderButton.Tag.ToString());

        // LoadFromXml(filename);

        /*
                var xd = XDocument.Load(senderButton.Tag.ToString());
                var x = xd.Root;
                decoder = new Decoder
                {
                    NodesCount = int.Parse(x.Element("NodesCount").Value),
                    EdgesCount = int.Parse(x.Element("EdgesCount").Value),
                    NodeCoordinates = x.Element("NodeCoordinates").Elements("Point")
                    .Select(y =>
                        new Point
                        {
                            X = double.Parse(y.Element("X").Value),
                            Y = double.Parse(y.Element("Y").Value)
                        }).ToArray(),
                    SourceNodeIdentifiers = x.Element("SourceNodeIdentifiers")
                    .Elements("int").Select(y => int.Parse(y.Value)).ToArray(),
                    TargetNodeIdentifiers = x.Element("TargetNodeIdentifiers")
                    .Elements("int").Select(y => int.Parse(y.Value)).ToArray()
                };


                //восстановление вершин
                for (int opi = 0; opi < decoder.NodesCount; opi++)
                {
                    graph.AddNode(decoder.NodeCoordinates[opi]);
                    GraphCanvas.Children.Add(graph.Nodes.Last().View.Circle);
                    
                    //  grid_main_window.Children.Add(graph.Nodes.Last().View.Circle);
                    //NodesComboBox.Items.Add(graph.Nodes.Last().View.Circle.Name);
                }
                //NodesComboBox.SelectedIndex = 0;
                
                //восстановление ребер
                for (int opi = 0; opi < decoder.EdgesCount; opi++)
                {
                    graph.SourceNode = graph.Nodes[decoder.SourceNodeIdentifiers[opi]];
                    graph.TargetNode = graph.Nodes[decoder.TargetNodeIdentifiers[opi]];

                    graph.AddEdge();

                    var lastEdge = graph.Edges.Last();
                    myCanvas.Children.Add(lastEdge.View.Line);
                   // grid_main_window.Children.Add(lastEdge.View.Line);
                  // MessageBox.Show(lastEdge.View.Line.ToString());


                    Panel.SetZIndex(lastEdge.View.Line, -1);

                    graph.enable_conect = true;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
         */

        private void bwLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            var pathes = e.Argument as string[];
            if (pathes == null)
                return;

            foreach (var path in pathes)
            {
                //Thread.Sleep(2000);                
                var src = new BitmapImage {CacheOption = BitmapCacheOption.OnLoad};
                src.BeginInit();
                src.UriSource = new Uri(path, UriKind.Relative);
                src.EndInit();
                src.Freeze();
                bwLoader.ReportProgress(0, src);
            }
        }

        private void Window_MouseMove(object sender, MouseEventArgs e)
        {
        }

        private void main_vindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

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
                nodeList.Add(new Node1 {Id = id, CenterPoint = cp});
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
                nodes.Add(nodeElement);
            }

            var edges = new XElement("edges");
            foreach (var edge in GraphCanvas.Children.OfType<Edge1>())
            {
                var edgeElement = new XElement("edge");
                edgeElement.SetAttributeValue("sourceId", edge.SourceNode.Id);
                edgeElement.SetAttributeValue("targetId", edge.TargetNode.Id);
                edges.Add(edgeElement);
            }
            root.Add(nodes);
            root.Add(edges);
            root.Save(filename);
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
#endregion
            var pathSaveDirectory = AppDomain.CurrentDomain.BaseDirectory;
            int positionBinDirectory = pathSaveDirectory.IndexOf(binDirectory);
            if (positionBinDirectory > 0)
            {
                pathSaveDirectory = pathSaveDirectory.Substring(0, positionBinDirectory) + DinamicTailsDirectory + @"\";
            }
            var saveBinaryDialog = new SaveFileDialog
            {
                DefaultExt = ".xml",
                FileName = "Your_path",
                Filter = "Your_path (*.xml)|*.xml",

                InitialDirectory = pathSaveDirectory,
            };

            var result = saveBinaryDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var a = GraphCanvas.Nodes1;
                var DefaultXmlName = Path.GetFileName(saveBinaryDialog.FileName);
                SaveToXml(DefaultXmlName);

#region Old Codes
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

#region old code
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
        }
#endregion
        private void CreateBossNode_OnClick(object sender, RoutedEventArgs e)
        {
            Boolean isBossNodeNotCreate = true;
            foreach (var node in GraphCanvas.Children.OfType<Node1>())
            {
                if (isBossNodeNotCreate && node.IsSelected)
                {
                    node.isBossNode = node.IsSelected;
                    isBossNodeNotCreate = false;
                }
                else
                {
                    node.isBossNode = false;
                }
                node.InvalidateVisual();
            }
        }
    }
}
