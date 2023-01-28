using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
//using System.Windows.Controls;
//using System.Windows.Media;
//using System.Windows.Input;
using System.Xml.Linq;

using Converters;
using Core;

namespace View
{
    //Генерирует визуальный образ хвоста.
    public static class ViewTailGiver
    {
        public static ViewTail CreateViewTail(AbstractTail Abstract)
        {
            const int ZoomRedactorCustomEnding = 10;
            ViewTail NewViewTail = new ViewTail();

            NewViewTail.MasterTrainPaths = Abstract.LeftLogicalTrainPath.ViewTrainPath.MasterTrainPaths;
            NewViewTail.BackTrainPath = Abstract.LeftLogicalTrainPath.ViewTrainPath;
            NewViewTail.NextTrainPath = null;
            NewViewTail.LogicalTail = Abstract;
            NewViewTail.LogicalTail.ViewAbstractTail = NewViewTail;
            NewViewTail.UpShift=30;

            LinkedTail Linked;
            SingleTail Single;
            switch (Abstract.NameTail)
            {
                case AbstractTailGiver.NamesTails.LinkedTail:
                    Linked = (LinkedTail)Abstract;
                    NewViewTail.NextTrainPath = Linked.RightLogicalTrainPath.ViewTrainPath;

                    NewViewTail.TailPoints.Add(new Point(0, 0));
                    switch (Linked.LeftLogicalTrainPath.direction.value)
                    {
                        case DirectionValue.EVEN:
                            NewViewTail.TailPoints.Add(new Point(0, +NewViewTail.UpShift));
                            NewViewTail.TailPoints.Add(new Point(Linked.DeltaTime, +NewViewTail.UpShift));
                            break;
                        case DirectionValue.ODD:
                            NewViewTail.TailPoints.Add(new Point(0, -NewViewTail.UpShift));
                            NewViewTail.TailPoints.Add(new Point(Linked.DeltaTime, -NewViewTail.UpShift));
                            break;
                        default:
                            throw new System.ArgumentException("Unknown DirectionValue (Неизвестное направление) in AbstractTailGiver class (CreateAbstractTail)", "original");
                    }
                    NewViewTail.TailPoints.Add(new Point(Linked.DeltaTime, 0));
                    NewViewTail.TailEdges.Add(new Point(0, 1));
                    NewViewTail.TailEdges.Add(new Point(1, 2));
                    NewViewTail.TailEdges.Add(new Point(2, 3));
                    break;
                case AbstractTailGiver.NamesTails.LinkedTailCustom:
                    Linked = (LinkedTail)Abstract;
                    //Должно быть!!! => NewViewTail.NextTrainPath = 
                    break;
                case AbstractTailGiver.NamesTails.SingleTail:
                    Single = (SingleTail)Abstract;
                    NewViewTail.TailPoints.Add(new Point(0, 0));
                    switch (Single.LeftLogicalTrainPath.direction.value)
                    {
                        case DirectionValue.EVEN:
                            NewViewTail.TailPoints.Add(new Point(0, -NewViewTail.UpShift));
                            break;
                        case DirectionValue.ODD:
                            NewViewTail.TailPoints.Add(new Point(0, -NewViewTail.UpShift));
                            break;
                        default:
                            throw new System.ArgumentException("Unknown DirectionValue (Неизвестное направление) in AbstractTailGiver class (CreateAbstractTail)", "original");
                    }
                    NewViewTail.TailEdges.Add(new Point(0, 1));
                    break;
                case AbstractTailGiver.NamesTails.NightTail:
                    NightTail Night = (NightTail)Abstract;
                    NewViewTail.TailPoints.Add(new Point(0, 0));
                    NewViewTail.TailPoints.Add(new Point(Night.LengthLine,0));

                    NewViewTail.TailEdges.Add(new Point(0, 1));
                    break;
                case AbstractTailGiver.NamesTails.SingleTailCustom:
                    Single = (SingleTail)Abstract;
                    var root = XDocument.Load(Single.SecondName).Element("graph");
                    var nodes = root.Element("nodes").Elements();
                    int IndexBossNode = 1;
                    int MinNode = Convert.ToInt16(root.Element("nodes").Attribute("maxNodeId").Value);

                    List<Point> MemoryPoints = new List<Point>();

                    foreach (var nodeElement in nodes)
                    {

                        if (Convert.ToInt16(nodeElement.Attribute("id").Value) < MinNode)
                        {
                            MinNode = Convert.ToInt16(nodeElement.Attribute("id").Value);
                        }
                        MemoryPoints.Add(new Point((int)TypeDataConverter.StringToDouble(nodeElement.Element("X").Value)/10,TypeDataConverter.StringToDouble(nodeElement.Element("Y").Value)/10));
                        if (Boolean.Parse(nodeElement.Element("isBossNode").Value))
                        {
                            IndexBossNode = MemoryPoints.Count - 1;
                        }
                    }

                    foreach (Point thNode in MemoryPoints)
                    {
                         NewViewTail.TailPoints.Add(new Point(thNode.X-MemoryPoints[IndexBossNode].X,thNode.Y-MemoryPoints[IndexBossNode].Y));
                    }
                    try
                    {
                        var edges = root.Element("edges").Elements();
                        foreach (var element in edges)
                        {
                            NewViewTail.TailEdges.Add(new Point(Convert.ToInt32(element.Attribute("sourceId").Value)-MinNode, Convert.ToInt32(element.Attribute("targetId").Value)-MinNode));
                        }
                    }
                    catch
                    {
                        MessageBox.Show("Файл имеет ошибочную структуру.", "Загрузка из файла", MessageBoxButton.OK, MessageBoxImage.Error);
                    }                                                                                 
                    break;
                default:
                    throw new System.ArgumentException("Unknown NamesTails (Неизвестное имя хвоста)", "original");
            }
            return NewViewTail;
        }
    }
}
