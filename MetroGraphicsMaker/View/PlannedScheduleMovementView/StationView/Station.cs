using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

using Core;

namespace View
{
    //Описывает расположение станции на панели станций.
    public class ViewStation : Label
    {
        public PannelStation MasterStation;
        public Station LogicalStation;
        /// <summary>
        /// Сдвиг текста имени станции вверх (для правильного расположения линии на панели станций)
        /// </summary>
        protected const int ShiftPositionLabel = 20;
        /// <summary>
        /// Линия станци для отображения на основной области графика.
        /// </summary>
        public StationLineForWorkArea StationLineWorkArea;

        protected override void OnRender(DrawingContext DC)
        {
            Pen PenTrainPath = new Pen(Brushes.Black, 1);
            DC.DrawLine(PenTrainPath, new Point(0, +ShiftPositionLabel), new Point(Width, +ShiftPositionLabel));
        }

        /// <summary>
        /// Конструктор имени станции и "линии на панели станций" для панели станций
        /// </summary>
        /// <param name="Name">Наименование создаваемой станции</param>
        /// <param name="RealPosition">Реальное положение станции на графике</param>
        /// <param name="WidthPannelNameStation">Длинна создаваемой линии на панели станций (Должна быть равна длинне паннели станций)</param>
        /// <param name="WidthAllWorkArea">Длинна всего графика</param>
        public ViewStation(Station _LogicalStation, PannelStation _MasterStation)
        {
            MasterStation = _MasterStation;
            LogicalStation = _LogicalStation;

            Width = MasterStation.MasterDiagram.WidthPannelStation;

            StationLineWorkArea = new StationLineForWorkArea();
            StationLineWorkArea.Width = MasterStation.MasterDiagram.ActualWidth;

            Margin = new Thickness(0, LogicalStation.CoordinateInMm - MasterStation.FirstStationCoordinateInMm - ShiftPositionLabel, 0, 0);
            Content = LogicalStation.name;
            ContextMenu = new MouseMenuForStation(LogicalStation.name,LogicalStation.code);
            StationLineWorkArea.ContextMenu = new MouseMenuForStation(LogicalStation.name, LogicalStation.code);
        }

        public void ZoomChangeStation()
        {
            double TopPositionStation = Math.Abs(LogicalStation.CoordinateInMm - MasterStation.FirstStationCoordinateInMm) * MasterStation.MasterDiagram.Zoom;
            Margin = new Thickness(0, TopPositionStation - ShiftPositionLabel, 0, 0);
            StationLineWorkArea.Margin = new Thickness(0, TopPositionStation, 0, 0);
            StationLineWorkArea.Width = MasterStation.MasterDiagram.Width;
            //StationLineWorkArea.Width = MasterStation.MasterDiagram.ActualWidth;
        }
    }
}