using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace linii_graph
{
    public class IOXml
    {
        public List<Node1> Nodes;

        public List<Edge1> Edges;

        public IOXml()
        {
            Nodes = new List<Node1>();
            Edges = new List<Edge1>();
        }

        public void LoadFromXml(String filename)
        {
            var root = XDocument.Load(filename).Element("graph");
            var nodes = root.Element("nodes").Elements();

            Nodes.Clear();
            foreach (var nodeElement in nodes)
            {
                var id = Convert.ToInt32(nodeElement.Attribute("id").Value);
                var cp = new Point(
                    MyConverter(nodeElement.Element("X").Value),
                    MyConverter(nodeElement.Element("Y").Value)
                    //Convert.ToDouble(nodeElement.Element("X").Value, CultureInfo.CurrentCulture),
                    //Convert.ToDouble(nodeElement.Element("Y").Value, CultureInfo.CurrentCulture)
                    );
                var isBoss = Boolean.Parse(nodeElement.Element("isBossNode").Value);
                //Node1 NewNode = 
                Nodes.Add(new Node1 { Id = id, CenterPoint = cp, isBossNode = isBoss });
            }

            try
            {
                var edges = root.Element("edges").Elements();
                foreach (var element in edges)
                {
                    //var id = Convert.ToInt32(element.Attribute("id").Value);
                    var sourceId = Convert.ToInt32(element.Attribute("sourceId").Value);
                    var sourceNode = Nodes.SingleOrDefault(node => node.Id == sourceId);

                    var targetId = Convert.ToInt32(element.Attribute("targetId").Value);
                    var targetNode = Nodes.SingleOrDefault(node => node.Id == targetId);

                    if (sourceNode != null && targetNode != null)
                        Edges.Add(new Edge1(sourceNode, targetNode));
                }
            }
            catch
            {
                Nodes.Clear();
                Edges.Clear();
                MessageBox.Show("Файл имеет ошибочную структуру.", "Загрузка из файла", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        public void LoadFromXmlNew(String filename)
        {
            var root = XDocument.Load(filename).Element("graph");
            var nodes = root.Element("nodes").Elements();

            Nodes.Clear();
            foreach (var nodeElement in nodes)
            {
                var id = Convert.ToInt32(nodeElement.Attribute("id").Value);
                var cp = new Point(
                    MyConverter(nodeElement.Element("X").Value),
                    MyConverter(nodeElement.Element("Y").Value)
                     );

                var isBoss = Boolean.Parse(nodeElement.Element("isBossNode").Value);

                Node1.NodeType nt;
                if (nodeElement.Element("type") != null)
                {
                    string nodeType = (nodeElement.Element("type").Value);
                    Boolean res = Node1.NodeType.TryParse(nodeElement.Element("type").Value, out nt);
                    if (!res)
                    {
                        nt = Node1.NodeType.Yellow;
                    }
                }
                else
                {
                    if (isBoss)
                    {
                        nt = Node1.NodeType.White;
                    }
                    else
                    {
                        nt = Node1.NodeType.Yellow;
                    }
                }                
                Nodes.Add(new Node1 { Id = id, CenterPoint = cp, isBossNode = isBoss, Type = nt});                
            }

            try
            {
                var edges = root.Element("edges").Elements();
                foreach (var element in edges)
                {
                    //var id = Convert.ToInt32(element.Attribute("id").Value);
                    var sourceId = Convert.ToInt32(element.Attribute("sourceId").Value);
                    var sourceNode = Nodes.SingleOrDefault(node => node.Id == sourceId);

                    var targetId = Convert.ToInt32(element.Attribute("targetId").Value);
                    var targetNode = Nodes.SingleOrDefault(node => node.Id == targetId);

                    if (sourceNode != null && targetNode != null)
                        Edges.Add(new Edge1(sourceNode, targetNode));
                }
            }
            catch
            {
                Nodes.Clear();
                Edges.Clear();
                MessageBox.Show("Файл имеет ошибочную структуру.", "Загрузка из файла", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected Double MyConverter(String value)
        {
            var culture = CultureInfo.CurrentCulture;
            if (value.Contains(","))
            {
                culture = CultureInfo.GetCultureInfo("ru-ru");
            }
            else if (value.Contains("."))
            {
                culture = CultureInfo.GetCultureInfo("en-US");
            }
            return Convert.ToDouble(value, culture);
        }
    }
}
