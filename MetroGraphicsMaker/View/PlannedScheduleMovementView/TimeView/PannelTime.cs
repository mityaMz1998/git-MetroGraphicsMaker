using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

using Converters;
using Core;

namespace View
{
    //Описывает область шкалы времени и пересечений со станциями.
    public class PannelTime : Canvas
    {
        public Diagram MasterDiagram;
        public List<TimeMinute> TimeMinutes;
        protected List<TimeHour> TimeHours;
        public int PositionPannelStationTop; //От PannelTime:Canvas до WorkArea по оси Y

        //protected List<TimeSeconds> TimeSeconds;

        protected override void OnRender(DrawingContext dc)
        {
            Pen PenTimeLine = new Pen(Brushes.Black, 1);
            if (TimeMinutes.Count != 0)
            {
                double ShiftDownPanelTime = PannelTimeConstant.HeightPannelTime + MasterDiagram.OY.Height + PannelTimeConstant.PannelStationTopSubScalePannelTime;

                dc.DrawLine(PenTimeLine, new Point(0, PannelTimeConstant.BorderPannelTime), new Point(Width, PannelTimeConstant.BorderPannelTime));
                dc.DrawLine(PenTimeLine, new Point(0, PannelTimeConstant.BorderPannelTime + PannelTimeConstant.HeightScalePannelTime), new Point(Width, PannelTimeConstant.BorderPannelTime + PannelTimeConstant.HeightScalePannelTime));

                dc.DrawLine(PenTimeLine, new Point(0, ShiftDownPanelTime), new Point(Width, ShiftDownPanelTime));
                dc.DrawLine(PenTimeLine, new Point(0, ShiftDownPanelTime + PannelTimeConstant.HeightScalePannelTime), new Point(Width, ShiftDownPanelTime + PannelTimeConstant.HeightScalePannelTime));
            }
        }

        public PannelTime(int _PositionPannelStationTop, int Left, Diagram _TheDiagram)
        {
            PositionPannelStationTop = _PositionPannelStationTop;
            MasterDiagram = _TheDiagram;
            Margin = new Thickness(Left, 0, 0, 0);
            TimeMinutes = new List<TimeMinute>();
            TimeHours = new List<TimeHour>();
            //TimeSeconds = new List<TimeSeconds>();

            Width = TimeConverter.SecondsToPixels(MasterDiagram.FinishTimeOnDiagram / TimeConverter.timeScale, true);
        }

        public PannelTime(int WorkHeightDiagram, int _PositionPannelStationTop, int Left, List<ViewStation> _Stations, Diagram _TheDiagram)
        {
            const int FiveMinutes = 5;
            const int ForHalfMinutes = 2;

            PositionPannelStationTop = _PositionPannelStationTop;
            MasterDiagram = _TheDiagram;
            Margin = new Thickness(Left, 0, 0, 0);
            TimeMinutes = new List<TimeMinute>();
            TimeHours = new List<TimeHour>();
            //TimeSeconds = new List<TimeSeconds>();
            for (int Seconds = MasterDiagram.StartTimeOnDiagram; Seconds <= MasterDiagram.FinishTimeOnDiagram; Seconds += 1)
            {
                if (Seconds % TimeConverter.SecondsToTime(TimeConverter.secondsInHour).TimeOfDay.TotalSeconds == 0)
                {
                    TimeHours.Add(new TimeHour(WorkHeightDiagram, Seconds, this, true, true));
                }
                else
                {
                    if (Seconds % TimeConverter.SecondsToTime(TimeConverter.secondsInMinute).TimeOfDay.TotalSeconds == 0)
                    {
                        if (Seconds % TimeConverter.SecondsToTime(TimeConverter.secondsInMinute * FiveMinutes).TimeOfDay.TotalSeconds == 0)
                        {
                            TimeMinutes.Add(new TimeMinute(WorkHeightDiagram, Seconds, this, true, true));
                        }
                        else
                        {
                            TimeMinutes.Add(new TimeMinute(WorkHeightDiagram, Seconds, this, false, false));
                        }
                    }
                    else
                    {
                        if (Seconds % TimeConverter.SecondsToTime(TimeConverter.secondsInMinute / ForHalfMinutes).TimeOfDay.TotalSeconds == 0)
                        {
                            //TimeSeconds.Add(new TimeSeconds(WorkHeightDiagram, Seconds, this, false, false));
                        }
                    }
                }
            }
            /*
            for (int Seconds = 0; Seconds <= _Seconds; Seconds += 5)
            {
                if (Seconds % MinPeriod != 0) TimeSeconds.Add(new TimeSeconds(WorkHeightDiagram, TopPositionWorkAreaDiagram, Seconds, this));
            }*/
            TimeMinutes.ForEach(Minute => Children.Add(Minute));
            TimeHours.ForEach(Hour => Children.Add(Hour));
            //TimeSeconds.ForEach(Seconds => Children.Add(Seconds));
            Width = TimeConverter.SecondsToPixels(MasterDiagram.FinishTimeOnDiagram / TimeConverter.timeScale);
        }

        public void ZoomChangeTimes()
        {
            foreach (TimeMinute thisTimeMinute in TimeMinutes) thisTimeMinute.InvalidateVisual();
            foreach (TimeHour thisTimeHour in TimeHours) thisTimeHour.InvalidateVisual();
            //foreach (TimeSeconds thisTimeSeconds in TimeSeconds) thisTimeSeconds.InvalidateVisual();

            Width = TimeConverter.SecondsToPixels(MasterDiagram.FinishTimeOnDiagram / TimeConverter.timeScale) * MasterDiagram.Zoom;
        }

        public void NewTimeLines()
        {
            const int FiveMinutes = 5;
            const int ForHalfMinutes = 2;
            Children.Clear();
            TimeHours.Clear();
            TimeMinutes.Clear();
            for (int Seconds = MovementSchedule.MovementTime.begin * 5; Seconds <= MovementSchedule.MovementTime.end * 5; Seconds += 1)
            {
                if (Seconds % TimeConverter.SecondsToTime(TimeConverter.secondsInHour).TimeOfDay.TotalSeconds == 0)
                {
                    TimeHours.Add(new TimeHour((int)MasterDiagram.OY.Height, Seconds, this, true, true));
                }
                else
                {
                    if (Seconds % TimeConverter.SecondsToTime(TimeConverter.secondsInMinute).TimeOfDay.TotalSeconds == 0)
                    {
                        if (Seconds % TimeConverter.SecondsToTime(TimeConverter.secondsInMinute * FiveMinutes).TimeOfDay.TotalSeconds == 0)
                        {
                            TimeMinutes.Add(new TimeMinute((int)MasterDiagram.OY.Height, Seconds, this, true, true));
                        }
                        else
                        {
                            TimeMinutes.Add(new TimeMinute((int)MasterDiagram.OY.Height, Seconds, this, false, false));
                        }
                    }
                    else
                    {
                        if (Seconds % TimeConverter.SecondsToTime(TimeConverter.secondsInMinute / ForHalfMinutes).TimeOfDay.TotalSeconds == 0)
                        {
                            //TimeSeconds.Add(new TimeSeconds((int)HeightWorkAreaDiagram, Seconds, this, false, false));
                        }
                    }
                }
            }

            TimeMinutes.ForEach(Minute => Children.Add(Minute));
            TimeHours.ForEach(Hour => Children.Add(Hour));

            Width = TimeConverter.SecondsToPixels(MasterDiagram.FinishTimeOnDiagram / TimeConverter.timeScale) * MasterDiagram.Zoom;
        }
    }
}
