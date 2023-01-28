using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

using Actions;
using Core;
using Converters;

namespace View
{
    //Описывает отображение хвостов ниток.
    public class ViewTail : Control
    {
        public List<Point> TailPoints = new List<Point>();
        public List<Point> TailEdges = new List<Point>();
        public double UpShift;

        public TrainPath BackTrainPath;
        public TrainPath NextTrainPath;

        public ListTrainPaths MasterTrainPaths;
        public AbstractTail LogicalTail;

        public ConditionTrainPath Condition = ConditionTrainPath.Free;

        protected override void OnRender(DrawingContext DC)
        {
            Margin = new Thickness(TimeConverter.SecondsToPixels(LogicalTail.TimeBeginTail) * MasterTrainPaths.MasterDiagram.Zoom, LogicalTail.LeftLogicalTrainPath.ViewTrainPath.Node[LogicalTail.LeftLogicalTrainPath.ViewTrainPath.Node.Count - 1].Y, 0, 0);

            Pen PenTail;
            switch (BackTrainPath.Condition)
            {
                case ConditionTrainPath.Free:
                    PenTail = PenGiver.CreatePen(PenGiver.ColorPenForTrainPath.DarkGreen);
                    //WidthPen = PenGiver.ThicknessPen / 2;
                    break;
                case ConditionTrainPath.PrimarySelected:
                    PenTail = PenGiver.CreatePen(PenGiver.ColorPenForTrainPath.Red);
                    //WidthPen = PenGiver.ThicknessPen / 2;
                    break;
                case ConditionTrainPath.SecondarySelected:
                    PenTail = PenGiver.CreatePen(PenGiver.ColorPenForTrainPath.RoyalBlue);
                    //WidthPen = PenGiver.ThicknessPen / 2;
                    break;
                default:
                    throw new System.ArgumentException("Unknown Condition (Неизвестное состояние нитки)", "original");
            }

            for (int i = 0; i < TailEdges.Count; i++)
            {
                //Имеется ввиду что нужно соединить ребра по точкам.
                DC.DrawLine(PenTail, new Point(TimeConverter.SecondsToPixels((int)TailPoints[(int)TailEdges[i].X].X, true) * MasterTrainPaths.MasterDiagram.Zoom, TailPoints[(int)TailEdges[i].X].Y), new Point(TimeConverter.SecondsToPixels((int)TailPoints[(int)TailEdges[i].Y].X, true) * MasterTrainPaths.MasterDiagram.Zoom, TailPoints[(int)TailEdges[i].Y].Y));
            }
        }
        /*
        public ViewTail(TrainPath _BackTrainPath, TrainPath _NextTrainPath, Core.Tail _LogicalTail, ListTrainPaths _MasterTrainPaths)
        {
            MasterTrainPaths = _MasterTrainPaths;
            BackTrainPath = _BackTrainPath;
            NextTrainPath = _NextTrainPath;

            LogicalTail = _LogicalTail;
            LogicalTail.ViewAbstractTail = this;
        }*/

        public ViewTail()
        {
        }
    }
}
