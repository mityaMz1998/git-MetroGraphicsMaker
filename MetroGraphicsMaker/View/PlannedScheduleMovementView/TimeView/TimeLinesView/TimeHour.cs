using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;

using Converters;

namespace View
{
    //Описывает отображение часовых линий.
    public class TimeHour : AtributesTimeLines
    {
        protected override void OnRender(DrawingContext dc)
        {
            Pen PenTimeLine = new Pen(Brushes.Black, 1);
            double PositionYForDrawLine = PannelTimeConstant.ShiftPositionLabelHourForOneSymbol.Y + PannelTimeConstant.HeightScalePannelTime;
            Margin = new Thickness(TimeConverter.SecondsToPixels(TimePosition / TimeConverter.timeScale) * MasterTime.MasterDiagram.Zoom - ShiftLabel.X, ShiftLabel.Y, 0, 0);
            if (isAllLine)
            {
                //1
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PannelTimeConstant.ShiftPositionLabelHourForOneSymbol.Y), new Point(ShiftLabel.X, PositionYForDrawLine));
                //2
                PositionYForDrawLine += PannelTimeConstant.PannelStationTopSubScalePannelTime;
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PositionYForDrawLine), new Point(ShiftLabel.X, PositionYForDrawLine + MasterTime.MasterDiagram.OY.Height));
                //3
                PositionYForDrawLine += MasterTime.MasterDiagram.OY.Height + PannelTimeConstant.PannelStationTopSubScalePannelTime;
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PositionYForDrawLine), new Point(ShiftLabel.X, PositionYForDrawLine + PannelTimeConstant.HeightScalePannelTime));
            }
            /*else
            {
                //1
                dc.DrawLine(PenTimeLine, ShiftLabel, new Point(ShiftLabel.X, HeightTimeLineOnPanel + ShiftLabel.Y));
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, ShiftLabel.Y + PannelTimeConstant.HeightScalePannelTime - HeightTimeLineOnPanel), new Point(ShiftLabel.X, ShiftLabel.Y + PannelTimeConstant.HeightScalePannelTime));
                //2
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.MasterDiagram.OY.Stations[0].StationLineWorkArea.Margin.Top), new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.MasterDiagram.OY.Stations[0].StationLineWorkArea.Margin.Top + HeightTimeLineOnStation));
                for (int i = 1; i < MasterTime.Stations.Count - 1; i++)
                {
                    dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.MasterDiagram.OY.Stations[i].StationLineWorkArea.Margin.Top - HeightTimeLineOnStation / 2), new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.MasterDiagram.OY.Stations[i].StationLineWorkArea.Margin.Top + HeightTimeLineOnStation / 2));
                }
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.MasterDiagram.OY.Stations[MasterTime.Stations.Count - 1].StationLineWorkArea.Margin.Top - HeightTimeLineOnStation), new Point(ShiftLabel.X, MasterTime.PositionPannelStationTop - ShiftDownText + MasterTime.MasterDiagram.OY.Stations[MasterTime.Stations.Count - 1].StationLineWorkArea.Margin.Top));
                //3
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, ShiftDownPanelTime), new Point(ShiftLabel.X, ShiftDownPanelTime + HeightTimeLineOnPanel));
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, ShiftDownPanelTime + PannelTimeConstant.HeightScalePannelTime - HeightTimeLineOnPanel), new Point(ShiftLabel.X, ShiftDownPanelTime + PannelTimeConstant.HeightScalePannelTime));
            }*/

            if (isTextOutput)
            {
                dc.DrawText(new FormattedText(Content.ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Brushes.Black), new Point(5, PositionYForDrawLine + PannelTimeConstant.HeightScalePannelTime)); //5 Магическое число следует удалить.
            }
        }

        public TimeHour(int _HeightWorkAreaDiagram, int Seconds, PannelTime _MasterTime, Boolean _isAllLine, Boolean _isTextOutput)
        {
            isAllLine = _isAllLine;
            isTextOutput = _isTextOutput;
            MasterTime = _MasterTime;
            TimePosition = Seconds;
            //TimePosition = Seconds - MasterTime.TheDiagram.StartTimeOnDiagram;
            HeightWorkAreaDiagram = _HeightWorkAreaDiagram;

            ShiftDownText = 0;

            FontWeight = FontWeights.Bold;
            FontSize = 20;

            Pen PenTimeLine = new Pen(Brushes.Black, 1);

            if (isTextOutput)
            {
                var ppp = TimeConverter.GetRealTime(TimeConverter.secondsInDay);
                ppp = 86400; /////////ВНИМАНИЕ СЮДА. Причина в некорректной записи строчки сверху/////////////////
                //Background = Brushes.Red;
                int Clock = Seconds % (int)ppp / (int)TimeConverter.SecondsToTime(TimeConverter.secondsInHour).TimeOfDay.TotalSeconds;
                if (Clock < 10)
                {
                    ShiftLabel = new Point(PannelTimeConstant.ShiftPositionLabelHourForOneSymbol.X, PannelTimeConstant.BorderPannelTime - PannelTimeConstant.ShiftPositionLabelHourForOneSymbol.Y);
                    Width = 22;
                }
                else
                {
                    ShiftLabel = new Point(PannelTimeConstant.ShiftPositionLabelHourForTwoSymbols.X, PannelTimeConstant.BorderPannelTime - PannelTimeConstant.ShiftPositionLabelHourForTwoSymbols.Y);
                    Width = 32;
                }
                Content = Clock;
            }
        }
    }
}
