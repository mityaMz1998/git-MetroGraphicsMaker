using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;

using Converters;

namespace View
{
    //Для отладки, описывает отображение секундных линий. 
    /*public class TimeSeconds : AtributesTimeLines
    {

        protected override void OnRender(DrawingContext dc)
        {
            Pen PenTimeLine = new Pen(Brushes.Black, 1);
            double ShiftDownPanelTime = MasterTime.PositionPannelStationTop + MasterTime.MasterDiagram.OY.Height;

            Margin = new Thickness(TimeConverter.SecondsToPixels(TimePosition / TimeConverter.timeScale) * MasterTime.Zoom - ShiftLabel.X, ShiftDownText, 0, 0);
            if (isAllLine)
            {
                //1
                dc.DrawLine(PenTimeLine, ShiftLabel, new Point(ShiftLabel.X, ShiftLabel.Y + PannelTimeConstant.HeightScalePannelTime));
                //2
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.Stations[0].StationLineWorkArea.Margin.Top), new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.Stations[MasterTime.Stations.Count - 1].StationLineWorkArea.Margin.Top));
                //3
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, ShiftDownPanelTime), new Point(ShiftLabel.X, ShiftDownPanelTime + PannelTimeConstant.HeightScalePannelTime));
            }
            else
            {
                //1
                dc.DrawLine(PenTimeLine, ShiftLabel, new Point(ShiftLabel.X, HeightTimeLineOnPanel + ShiftLabel.Y));
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, ShiftLabel.Y + PannelTimeConstant.HeightScalePannelTime - HeightTimeLineOnPanel), new Point(ShiftLabel.X, ShiftLabel.Y + PannelTimeConstant.HeightScalePannelTime));
                //2
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.Stations[0].StationLineWorkArea.Margin.Top), new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.Stations[0].StationLineWorkArea.Margin.Top + HeightTimeLineOnStation));
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.Stations[MasterTime.Stations.Count - 1].StationLineWorkArea.Margin.Top - HeightTimeLineOnStation), new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.Stations[MasterTime.Stations.Count - 1].StationLineWorkArea.Margin.Top));
                //3
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, ShiftDownPanelTime), new Point(ShiftLabel.X, ShiftDownPanelTime + HeightTimeLineOnPanel));
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, ShiftDownPanelTime + PannelTimeConstant.HeightScalePannelTime - HeightTimeLineOnPanel), new Point(ShiftLabel.X, ShiftDownPanelTime + PannelTimeConstant.HeightScalePannelTime));
            }

            if (isTextOutput)
            {
                dc.DrawText(new FormattedText(Content.ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Brushes.Black), new Point(5, ShiftDownPanelTime)); //5 Для синхронизации верхнего текста с нижним по оси X;
            }
        }

        public TimeSeconds(int _HeightWorkAreaDiagram, int Seconds, PannelTime _MasterTime, Boolean _isAllLine, Boolean _isTextOutput)
        {
            isAllLine = _isAllLine;
            isTextOutput = _isTextOutput;
            MasterTime = _MasterTime;
            TimePosition = Seconds;
            //TimePosition = Seconds - MasterTime.TheDiagram.StartTimeOnDiagram;
            HeightWorkAreaDiagram = _HeightWorkAreaDiagram;

            ShiftDownText = 30;
            ShiftLabel = new Point(14, 5);
            HeightTimeLineOnPanel = 0;
            HeightTimeLineOnStation = 5;

            if (isTextOutput)
            {
                int Clock = Seconds % (int)TimeConverter.SecondsToTime(TimeConverter.secondsInHour).TimeOfDay.TotalSeconds / (int)TimeConverter.SecondsToTime(TimeConverter.secondsInMinute).TimeOfDay.TotalSeconds;
                if (Clock < 10)
                {
                    Content = "0" + Clock;
                }
                else
                {
                    Content = Clock;
                }
            }
        }
    }*/
}
