using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Media;

using Converters;

namespace View
{
    //Описывает логику отображения минутных линий.
    public class TimeMinute : AtributesTimeLines
    {
        //PannelTime MasterTime;

        protected override void OnRender(DrawingContext dc)
        {
            Pen PenTimeLine = new Pen(Brushes.Black, 1);
            double PositionYForDrawLine = PannelTimeConstant.ShiftPositionLabelMinute.Y + PannelTimeConstant.HeightScalePannelTime;
            Margin = new Thickness(TimeConverter.SecondsToPixels(TimePosition / TimeConverter.timeScale) * MasterTime.MasterDiagram.Zoom - ShiftLabel.X, ShiftLabel.Y, 0, 0);
            if (isAllLine)
            {
                //1
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PannelTimeConstant.ShiftPositionLabelMinute.Y), new Point(ShiftLabel.X, PositionYForDrawLine));
                //2
                PositionYForDrawLine += PannelTimeConstant.PannelStationTopSubScalePannelTime;
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PositionYForDrawLine), new Point(ShiftLabel.X, PositionYForDrawLine + MasterTime.MasterDiagram.OY.Height));
                //3
                PositionYForDrawLine += MasterTime.MasterDiagram.OY.Height + PannelTimeConstant.PannelStationTopSubScalePannelTime;
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PositionYForDrawLine), new Point(ShiftLabel.X, PositionYForDrawLine + PannelTimeConstant.HeightScalePannelTime));
            }
            else
            {
                //1
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PannelTimeConstant.ShiftPositionLabelMinute.Y), new Point(ShiftLabel.X, PannelTimeConstant.ShiftPositionLabelMinute.Y + PannelTimeConstant.HeightTimeMinuteLineOnPannel));
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PositionYForDrawLine - PannelTimeConstant.HeightTimeMinuteLineOnPannel), new Point(ShiftLabel.X, PositionYForDrawLine));
                //2
                PositionYForDrawLine += PannelTimeConstant.PannelStationTopSubScalePannelTime;

                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PositionYForDrawLine + MasterTime.MasterDiagram.OY.Stations[0].StationLineWorkArea.Margin.Top), new Point(ShiftLabel.X, PositionYForDrawLine + MasterTime.MasterDiagram.OY.Stations[0].StationLineWorkArea.Margin.Top + PannelTimeConstant.HeightTimeMinuteLineOnStation / 2));
                for (int i = 1; i < MasterTime.MasterDiagram.OY.Stations.Count - 1; i++)
                {
                    dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PositionYForDrawLine + MasterTime.MasterDiagram.OY.Stations[i].StationLineWorkArea.Margin.Top - PannelTimeConstant.HeightTimeMinuteLineOnStation / 2), new Point(ShiftLabel.X, PositionYForDrawLine + MasterTime.MasterDiagram.OY.Stations[i].StationLineWorkArea.Margin.Top + PannelTimeConstant.HeightTimeMinuteLineOnStation / 2));
                }
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PositionYForDrawLine + MasterTime.MasterDiagram.OY.Stations[MasterTime.MasterDiagram.OY.Stations.Count - 1].StationLineWorkArea.Margin.Top), new Point(ShiftLabel.X, PositionYForDrawLine + MasterTime.MasterDiagram.OY.Stations[MasterTime.MasterDiagram.OY.Stations.Count - 1].StationLineWorkArea.Margin.Top - PannelTimeConstant.HeightTimeMinuteLineOnStation / 2));                //3

                PositionYForDrawLine += MasterTime.MasterDiagram.OY.Height + PannelTimeConstant.PannelStationTopSubScalePannelTime;
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PositionYForDrawLine), new Point(ShiftLabel.X, PositionYForDrawLine + PannelTimeConstant.HeightTimeMinuteLineOnPannel));
                PositionYForDrawLine += PannelTimeConstant.HeightScalePannelTime;
                dc.DrawLine(PenTimeLine, new Point(ShiftLabel.X, PositionYForDrawLine - PannelTimeConstant.HeightTimeMinuteLineOnPannel), new Point(ShiftLabel.X, PositionYForDrawLine));
            }

            if (isTextOutput)
            {
                dc.DrawText(new FormattedText(Content.ToString(), System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface(FontFamily, FontStyle, FontWeight, FontStretch), FontSize, Brushes.Black), new Point(5, PositionYForDrawLine)); //5 Для синхронизации верхнего текста с нижним по оси X (Да, это магическое число. Я не нашел способа математичски его получить)
            }
        }

        public TimeMinute(int _HeightWorkAreaDiagram, int Seconds, PannelTime _MasterTime, Boolean _isAllLine, Boolean _isTextOutput)
        {
            isAllLine = _isAllLine;
            isTextOutput = _isTextOutput;
            MasterTime = _MasterTime;
            //TimePosition = Seconds - MasterTime.TheDiagram.StartTimeOnDiagram;

            TimePosition = Seconds;
            HeightWorkAreaDiagram = _HeightWorkAreaDiagram;

            ShiftDownText = 30;
            ShiftLabel = new Point(PannelTimeConstant.ShiftPositionLabelMinute.X, PannelTimeConstant.BorderPannelTime - PannelTimeConstant.ShiftPositionLabelMinute.Y);

            FontSize = 16;
            Width = 28;
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
    }
}
