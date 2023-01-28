using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

using Core;

namespace View
{
    //Описывает область для размещения станций и хранит их.
    public class PannelStation : Canvas
    {
        public Diagram MasterDiagram;
        public List<ViewStation> Stations;

        /// <summary>
        /// Содержит координату Y для первой станции из БД. Будьте ОЧЕНЬ осторожны при работе с данным полем.
        /// </summary>
        public int FirstStationCoordinateInMm { private set; get; } //Если не хочешь проблем не делай public set. Или десять раз подумай над изменением. 

        protected override void OnRender(DrawingContext DC)
        {
            Pen PenTrainPath = new Pen(new SolidColorBrush(Color.FromArgb(255, 242, 242, 242)), 1);
            DC.DrawRectangle(new SolidColorBrush(Color.FromArgb(255, 242, 242, 242)), PenTrainPath, new Rect(0, -Margin.Top, MasterDiagram.WidthPannelStation, Height + PannelTimeConstant.HeightPannelTime * 2));
        }

        public PannelStation(int PositionPannelStationTop, Diagram _MasterDiagram)
        {
            MasterDiagram = _MasterDiagram;
            Height = 100;
            Width = 100;
            Stations = new List<ViewStation>();
            Margin = new Thickness(0, PannelTimeConstant.HeightPannelTime, 0, 0);
            Canvas.SetZIndex(this, 10000);
        }

        public void HorizonalScrollMoveAllPanel(double PositionX)
        {
            Margin = new Thickness(PositionX, Margin.Top, 0, 0);
        }

        public void ZoomChangeStations()
        {
            foreach (ViewStation ThisStation in Stations) ThisStation.ZoomChangeStation();
            Height = Stations.Last().StationLineWorkArea.Margin.Top;
        }

        public void AddStation(Station LogicalStation)
        {
            if (Stations.Count == 0) FirstStationCoordinateInMm = LogicalStation.CoordinateInMm;
            ViewStation NewStation = new ViewStation(LogicalStation, this);

            Stations.Add(NewStation);
            MasterDiagram.MasterTrainPaths.Children.Add(NewStation.StationLineWorkArea);
        }

        public void InvalidateStation()
        {
            Children.Clear();

            if (Stations.Count != 0) FirstStationCoordinateInMm = Stations[0].LogicalStation.CoordinateInMm;
            foreach (ViewStation thStation in Stations)
            {
                Children.Add(thStation);
            }

            foreach (ViewStation thStation in Stations)
            {
                thStation.StationLineWorkArea.Width = MasterDiagram.Width;
                thStation.ZoomChangeStation();
                // thStation.StationLineWorkArea.Width = MasterDiagram.Width;
            }

            Height = Stations.Last().StationLineWorkArea.Margin.Top;
            MasterDiagram.Height = Height + PannelTimeConstant.HeightPannelTime * 2;
        }
    }
}
