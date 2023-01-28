using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

using Core;
using Actions;
using Converters;
using Exceptions.Actions;

namespace View
{
    //Класс описывает нитки, вызов действий с нитками.
    public class TrainPath : Control
    {
        public ViewTail TailTrainPath = null;

        public List<Point> Node = new List<Point>();

        /// <summary>
        /// Используемый режим движения
        /// </summary>
        public RegimeOfMotion Regime = RegimeOfMotion.None;

        //public int TimeTrainOnLine;

        private Point StartPointPositionMouse = new Point(0, -1);

        public ConditionTrainPath Condition = ConditionTrainPath.Free;
        private double ThicknessPhantomPen = PenGiver.ThicknessPhantomPen;
        private Boolean isModificated = false;

        public ListTrainPaths MasterTrainPaths;
        public double Zoom = 1;

        public Core.TrainPath LogicalTrainPath = new Core.TrainPath();

        protected override void OnRender(DrawingContext DC)
        {
            Pen PenTrainPath;
            double WidthPen;

            switch (Condition)
            {
                case ConditionTrainPath.Free:
                    PenTrainPath = PenGiver.CreatePen(PenGiver.ColorPenForTrainPath.DarkGreen);
                    WidthPen = PenGiver.ThicknessPen / 2;
                    break;
                case ConditionTrainPath.PrimarySelected:
                    PenTrainPath = PenGiver.CreatePen(PenGiver.ColorPenForTrainPath.Red);
                    WidthPen = PenGiver.ThicknessPen / 2;
                    break;
                case ConditionTrainPath.SecondarySelected:
                    PenTrainPath = PenGiver.CreatePen(PenGiver.ColorPenForTrainPath.RoyalBlue);
                    WidthPen = PenGiver.ThicknessPen / 2;
                    break;
                default:
                    throw new ArgumentException("Unknown Condition (Неизвестное состояние нитки)", "original");
            }

            Pen PenPhantom = PenGiver.CreatePen(PenGiver.ColorPenForTrainPath.Phantom, ThicknessPhantomPen);
            double WidthPhantom = ThicknessPhantomPen / 2;

            Margin = new Thickness(TimeConverter.SecondsToPixels(LogicalTrainPath.DepartureTimeFromFirstStation) * MasterTrainPaths.MasterDiagram.Zoom, 0, 0, 0);
            DC.DrawText(new FormattedText("pix=" + Margin.Left + ";" + " allsec=" + LogicalTrainPath.DepartureTimeFromFirstStation + ";" + " Hour=" + (LogicalTrainPath.DepartureTimeFromFirstStation - LogicalTrainPath.DepartureTimeFromFirstStation % (3600 / 5)) / (3600 / 5) + ";" + " Min=" + (LogicalTrainPath.DepartureTimeFromFirstStation % (3600 / 5) - LogicalTrainPath.DepartureTimeFromFirstStation % (60 / 5)) / (60 / 5) + ";" + " Sec=" + LogicalTrainPath.DepartureTimeFromFirstStation % (60 / 5) * 5, System.Globalization.CultureInfo.GetCultureInfo("en-us"), FlowDirection.LeftToRight, new Typeface("zzz"), 10, Brushes.Red), new Point(0, -10)); // 5058 -семь чассов в скундах

            List<int> PosSt = new List<int>();
            foreach (ViewStation ThStation in MasterTrainPaths.MasterDiagram.OY.Stations)
            {
                PosSt.Add((int)ThStation.StationLineWorkArea.Margin.Top);
            }

            if (LogicalTrainPath.direction.value == DirectionValue.ODD)
            {
                PosSt.Reverse();
            }

            int TimeLengthTrainPath = (LogicalTrainPath.MasElRasp.First().departureTime - LogicalTrainPath.DepartureTimeFromFirstStation) - (LogicalTrainPath.MasElRasp.Last().arrivalTime - LogicalTrainPath.DepartureTimeFromFirstStation);
            for (int Index = 0; Index < LogicalTrainPath.MasElRasp.Count; Index++)
            {
                if (TimeLengthTrainPath > 0)
                {
                    TimeLengthTrainPath = TimeLengthTrainPath - LogicalTrainPath.MasElRasp[Index].TimeStoyanOtprav;
                }
                else
                {
                    TimeLengthTrainPath = TimeLengthTrainPath + LogicalTrainPath.MasElRasp[Index].TimeStoyanOtprav;
                }
            }
            double TangentLine = (double)(PosSt.First() - PosSt.Last()) / (TimeLengthTrainPath);

            Node.Clear();
            int SRVTimeShift = 0;
            int IndexNode = LogicalTrainPath.NumFirst;
            {
                /*var newNodeArrive = new Point(TimeConverter.SecondsToPixels((int)LogicalTrainPath.MasElRasp[IndexNode].arrivalTime - LogicalTrainPath.DepartureTimeFromFirstStation, true) * MasterTrainPaths.MasterDiagram.Zoom,
                    PosSt[IndexNode]);
                var newNodeDepart = new Point(TimeConverter.SecondsToPixels((int)LogicalTrainPath.MasElRasp[IndexNode].departureTime - LogicalTrainPath.DepartureTimeFromFirstStation, true) * MasterTrainPaths.MasterDiagram.Zoom,
                    PosSt[IndexNode]);

                Node.Add(newNodeArrive);

                Node.Add(newNodeDepart);
                DC.DrawLine(PenPhantom, newNodeArrive, newNodeDepart);
                DC.DrawEllipse(PenPhantom.Brush, null, newNodeArrive, WidthPhantom, WidthPhantom);
                DC.DrawEllipse(PenPhantom.Brush, null, newNodeDepart, WidthPhantom, WidthPhantom);
                DC.DrawLine(PenTrainPath, newNodeArrive, newNodeDepart);
                DC.DrawEllipse(PenTrainPath.Brush, null, newNodeArrive, WidthPen, WidthPen);
                DC.DrawEllipse(PenTrainPath.Brush, null, newNodeDepart, WidthPen, WidthPen);*/
                var newNodeArrive = new Point(TimeConverter.SecondsToPixels((int)((PosSt[IndexNode] - PosSt.First()) / TangentLine + SRVTimeShift), true) * MasterTrainPaths.MasterDiagram.Zoom,
                     PosSt[IndexNode]);
                SRVTimeShift = SRVTimeShift + LogicalTrainPath.MasElRasp[IndexNode].TimeStoyanOtprav;
                var newNodeDepart = new Point(TimeConverter.SecondsToPixels((int)((PosSt[IndexNode] - PosSt.First()) / TangentLine + SRVTimeShift), true) * MasterTrainPaths.MasterDiagram.Zoom,
                   PosSt[IndexNode]);

                Node.Add(newNodeArrive);
                Node.Add(newNodeDepart);
                DC.DrawLine(PenPhantom, newNodeArrive, newNodeDepart);
                DC.DrawEllipse(PenPhantom.Brush, null, newNodeArrive, WidthPhantom, WidthPhantom);
                DC.DrawEllipse(PenPhantom.Brush, null, newNodeDepart, WidthPhantom, WidthPhantom);
                DC.DrawLine(PenTrainPath, newNodeArrive, newNodeDepart);
                DC.DrawEllipse(PenTrainPath.Brush, null, newNodeArrive, WidthPen, WidthPen);
                DC.DrawEllipse(PenTrainPath.Brush, null, newNodeDepart, WidthPen, WidthPen);

                for (++IndexNode; IndexNode <= LogicalTrainPath.NumLast; IndexNode++)
                {
                    newNodeArrive = new Point(TimeConverter.SecondsToPixels((int)((PosSt[IndexNode] - PosSt.First()) / TangentLine + SRVTimeShift), true) * MasterTrainPaths.MasterDiagram.Zoom,
                         PosSt[IndexNode]);

                    DC.DrawLine(PenPhantom, newNodeDepart, newNodeArrive);
                    DC.DrawLine(PenTrainPath, newNodeDepart, newNodeArrive);

                    SRVTimeShift = SRVTimeShift + LogicalTrainPath.MasElRasp[IndexNode].TimeStoyanOtprav;

                    newNodeDepart = new Point(TimeConverter.SecondsToPixels((int)((PosSt[IndexNode] - PosSt.First()) / TangentLine + SRVTimeShift), true) * MasterTrainPaths.MasterDiagram.Zoom,
                       PosSt[IndexNode]);

                    Node.Add(newNodeArrive);
                    Node.Add(newNodeDepart);

                    DC.DrawLine(PenPhantom, newNodeArrive, newNodeDepart);
                    DC.DrawEllipse(PenPhantom.Brush, null, newNodeArrive, WidthPhantom, WidthPhantom);
                    DC.DrawEllipse(PenPhantom.Brush, null, newNodeDepart, WidthPhantom, WidthPhantom);
                    DC.DrawLine(PenTrainPath, newNodeArrive, newNodeDepart);
                    DC.DrawEllipse(PenTrainPath.Brush, null, newNodeArrive, WidthPen, WidthPen);
                    DC.DrawEllipse(PenTrainPath.Brush, null, newNodeDepart, WidthPen, WidthPen);
                }
            }

            /* if (this.Condition == ConditionTrainPath.PrimarySelected)
            {
                for (int i = 1; i < MasterTrainPaths.TrainPaths.Count; i++)
                {
                    if (MasterTrainPaths.TrainPaths[i].LogicalTrainPath.DepartureTimeFromFirstStation < MasterTrainPaths.TrainPaths[i - 1].LogicalTrainPath.DepartureTimeFromFirstStation)
                    {
                        MessageBox.Show("Bad Time");
                    }
                }

                TrainPathView thTrainPath = MasterTrainPaths.FirstEVEN;
                while (thTrainPath != null)
                {
                    if (thTrainPath.LogicalTrainPath.LeftThread != null && thTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation < thTrainPath.LogicalTrainPath.LeftThread.DepartureTimeFromFirstStation)
                    {
                        MessageBox.Show("Bad Time");
                    }
                    if (thTrainPath.LogicalTrainPath.direction.value != DirectionValue.EVEN)
                    {
                        MessageBox.Show("not EV");
                    }
                    if (thTrainPath.LogicalTrainPath.RightThread != null) thTrainPath = thTrainPath.LogicalTrainPath.RightThread.ViewTrainPath;
                    else thTrainPath = null;
                }
                thTrainPath = MasterTrainPaths.FirstODD;
                while (thTrainPath != null)
                {
                    if (thTrainPath.LogicalTrainPath.LeftThread != null && thTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation < thTrainPath.LogicalTrainPath.LeftThread.DepartureTimeFromFirstStation)
                    {
                        MessageBox.Show("Bad Time");
                    }
                    if (thTrainPath.LogicalTrainPath.direction.value != DirectionValue.ODD)
                    {
                        MessageBox.Show("not ODD");
                    }
                    if (thTrainPath.LogicalTrainPath.RightThread != null) thTrainPath = thTrainPath.LogicalTrainPath.RightThread.ViewTrainPath;
                    else thTrainPath = null; ;
                }
            }*/
            /*     PenTrainPath = PenGiver.CreatePen(PenGiver.ColorPenForTrainPath.TestPhantom);     
                 IndexNode = LogicalTrainPath.NumFirst;
                 {
                     var newNodeArrive = new Point(TimeConverter.SecondsToPixels((int)LogicalTrainPath.MasElRasp[IndexNode].arrivalTime - LogicalTrainPath.DepartureTimeFromFirstStation, true) * MasterTrainPaths.MasterDiagram.Zoom,
                         PosSt[IndexNode]);
                     var newNodeDepart = new Point(TimeConverter.SecondsToPixels((int)LogicalTrainPath.MasElRasp[IndexNode].departureTime - LogicalTrainPath.DepartureTimeFromFirstStation, true) * MasterTrainPaths.MasterDiagram.Zoom,
                         PosSt[IndexNode]);

                     Node.Add(newNodeArrive);

                     Node.Add(newNodeDepart);
                     DC.DrawLine(PenPhantom, newNodeArrive, newNodeDepart);
                     DC.DrawEllipse(PenPhantom.Brush, null, newNodeArrive, WidthPhantom, WidthPhantom);
                     DC.DrawEllipse(PenPhantom.Brush, null, newNodeDepart, WidthPhantom, WidthPhantom);
                     DC.DrawLine(PenTrainPath, newNodeArrive, newNodeDepart);
                     DC.DrawEllipse(PenTrainPath.Brush, null, newNodeArrive, WidthPen, WidthPen);
                     DC.DrawEllipse(PenTrainPath.Brush, null, newNodeDepart, WidthPen, WidthPen);

                     for (++IndexNode; IndexNode <= LogicalTrainPath.NumLast; IndexNode++)
                     {
                         newNodeArrive = new Point(TimeConverter.SecondsToPixels((int)LogicalTrainPath.MasElRasp[IndexNode].arrivalTime - LogicalTrainPath.DepartureTimeFromFirstStation, true) * MasterTrainPaths.MasterDiagram.Zoom,
                             PosSt[IndexNode]);

                         DC.DrawLine(PenPhantom, newNodeDepart, newNodeArrive);
                         DC.DrawLine(PenTrainPath, newNodeDepart, newNodeArrive);

                         newNodeDepart = new Point(TimeConverter.SecondsToPixels((int)LogicalTrainPath.MasElRasp[IndexNode].departureTime - LogicalTrainPath.DepartureTimeFromFirstStation, true) * MasterTrainPaths.MasterDiagram.Zoom,
                            PosSt[IndexNode]);

                         Node.Add(newNodeArrive);
                         Node.Add(newNodeDepart);

                         DC.DrawLine(PenPhantom, newNodeArrive, newNodeDepart);
                         DC.DrawEllipse(PenPhantom.Brush, null, newNodeArrive, WidthPhantom, WidthPhantom);
                         DC.DrawEllipse(PenPhantom.Brush, null, newNodeDepart, WidthPhantom, WidthPhantom);
                         DC.DrawLine(PenTrainPath, newNodeArrive, newNodeDepart);
                         DC.DrawEllipse(PenTrainPath.Brush, null, newNodeArrive, WidthPen, WidthPen);
                         DC.DrawEllipse(PenTrainPath.Brush, null, newNodeDepart, WidthPen, WidthPen);
                     }
                 }*/

            if (TailTrainPath != null)
            {
                TailTrainPath.InvalidateVisual();
            }
        }

        public TrainPath(Core.TrainPath path)
        {
            // ...
        }

        public TrainPath(Core.TrainPath thLogicalTrainPath, ListTrainPaths Master)
        {
            ToolTip = new ToolTip();
            LogicalTrainPath = thLogicalTrainPath;
            MasterTrainPaths = Master;
            if (LogicalTrainPath.flgPeak)
            {
                Regime = RegimeOfMotion.Peak;
            }
            else
            {
                Regime = RegimeOfMotion.NonPeak;
            }

            WorkWithTrainPath();

            Panel.SetZIndex(this, 100000);
        }

        public TrainPath(Direction _DirectionTrainPath, RegimeOfMotion RegOfMotion, ListTrainPaths Master)
        {
            LogicalTrainPath.direction = _DirectionTrainPath;
            LogicalTrainPath.MasElRasp = new List<clsElementOfSchedule>();

            MasterTrainPaths = Master;
            LogicalTrainPath.flgPeak = RegOfMotion == RegimeOfMotion.Peak;

            Int32 arrivalTime = 0;
            Int32 departureTime = 0;
            Boolean flag = true;
            Core.Station thStation;

            if (LogicalTrainPath.direction.value == DirectionValue.ODD)//odd
            {
                thStation = MovementSchedule.colLine[0].EndStationEven;
                var FoundTask = MovementSchedule.colTask.Find(thTask => thTask.departureStation == thStation && thTask.Direction.value == DirectionValue.ODD);
                if (FoundTask == null)
                {
                    MessageBox.Show("Error, NO StartFoundTask");
                }
                else
                {
                    while (flag && thStation != MovementSchedule.colLine[0].EndStationOdd)
                    {
                        if (FoundTask.departureStation == thStation && FoundTask.Direction.value == DirectionValue.ODD)
                        {
                            LogicalTrainPath.MasElRasp.Add(new clsElementOfSchedule(arrivalTime, 0, departureTime, FoundTask));
                            switch (RegOfMotion)
                            {
                                case RegimeOfMotion.Peak:
                                    arrivalTime = departureTime + FoundTask.ThPeak;//+ FoundTask.StatPeak;
                                    departureTime = arrivalTime + FoundTask.StatPeak;
                                    break;
                                case RegimeOfMotion.NonPeak:
                                    arrivalTime = departureTime + FoundTask.ThNonpeak;//+ FoundTask.StatPeak;
                                    departureTime = arrivalTime + FoundTask.StatNonpeak;
                                    break;
                                default:
                                    MessageBox.Show("Unknown  RegimeOfMotion in ListTrainPaths class");
                                    break;
                            }
                            thStation = FoundTask.destinationStation;
                            FoundTask = FoundTask.nextTask;
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                    LogicalTrainPath.MasElRasp.Add(new clsElementOfSchedule(arrivalTime, 0, arrivalTime, FoundTask));
                }

                if (!flag | thStation != MovementSchedule.colLine[0].EndStationOdd)
                {
                    if (!flag)
                    {
                        MessageBox.Show("Нет задания, начинающегося на станции Station в нужном направлении");
                    }
                    else
                    {
                        MessageBox.Show("На конечной станции пути есть лишнее задание в направлении нужное. Либо поменяйте конечное станцию пути либо добавьте задание.");
                    }
                }
            }
            else
            {
                thStation = MovementSchedule.colLine[0].EndStationOdd;
                Task FoundTask = MovementSchedule.colTask.Find(thTask => thTask.departureStation == thStation && thTask.Direction.value == DirectionValue.EVEN);
                if (FoundTask == null)
                {
                    MessageBox.Show("Error, NO StartFoundTask");
                }
                else
                {
                    while (flag && thStation != MovementSchedule.colLine[0].EndStationEven)
                    {
                        if (FoundTask.departureStation == thStation && FoundTask.Direction.value == DirectionValue.EVEN)
                        {
                            LogicalTrainPath.MasElRasp.Add(new clsElementOfSchedule(arrivalTime, 0, departureTime, FoundTask));
                            switch (RegOfMotion)
                            {
                                case RegimeOfMotion.Peak:
                                    arrivalTime = departureTime + FoundTask.ThPeak;//+ FoundTask.StatPeak;
                                    departureTime = arrivalTime + FoundTask.StatPeak;
                                    break;
                                case RegimeOfMotion.NonPeak:
                                    arrivalTime = departureTime + FoundTask.ThNonpeak;//+ FoundTask.StatPeak;
                                    departureTime = arrivalTime + FoundTask.StatNonpeak;
                                    break;
                                default:
                                    MessageBox.Show("Unknown  RegimeOfMotion in ListTrainPaths class");
                                    break;
                            }
                            thStation = FoundTask.destinationStation;
                            FoundTask = FoundTask.nextTask;
                        }
                        else
                        {
                            flag = false;
                        }
                    }
                    LogicalTrainPath.MasElRasp.Add(new clsElementOfSchedule(arrivalTime, 0, arrivalTime, FoundTask));
                }


                if (!flag || thStation != MovementSchedule.colLine[0].EndStationEven)
                {
                    if (!flag)
                    {
                        MessageBox.Show("Нет задания, начинающегося на станции Station в нужном направлении");
                    }
                    else
                    {
                        MessageBox.Show("На конечной станции пути есть лишнее задание в направлении нужное. Либо поменяйте конечное станцию пути либо добавьте задание.");
                    }
                }

            }
            Panel.SetZIndex(this, 1000);
        }

        /// <summary>
        /// Событие для создания текста для всплывающей подсказки при наведении мыши на выделеную нитку.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        protected void ToolTipGenerator(object sender, MouseEventArgs e)
        {
            //Приводим объект типа object, хранящийся внутри нитки к типу ToolTip.
            System.Windows.Controls.ToolTip thToolTip = (System.Windows.Controls.ToolTip)ToolTip;
            //В зависимости от выделенности нитки делаем те или иные действия.
            switch (Condition)
            {
                case ConditionTrainPath.Free:
                    //Если нитка не выделена, то сообщаем всплывающей подсказке, что отображаться ей не надо.
                    thToolTip.Visibility = Visibility.Collapsed;
                    break;
                case ConditionTrainPath.PrimarySelected:
                    //Если нитка выделена первично, то сообщаем подсказке новый текст и говорим что ей нужно отобразиться.
                    thToolTip.Visibility = Visibility.Visible;
                    ToolTipTextGenerator(thToolTip);
                    break;
                case ConditionTrainPath.SecondarySelected:
                    //Если нитка выделена вторично, то сообщаем подсказке новый текст и говорим что ей нужно отобразиться.
                    thToolTip.Visibility = Visibility.Visible;
                    ToolTipTextGenerator(thToolTip);
                    break;
                default:
                    //Неизвестный вариант выделения нитки, сообщаю об ошибке и ничего не делаю.
                    MessageBox.Show("Неизвестный Condition в методе ToolTipGenerator класса TrainPathView.");
                    break;
            }
        }

        /// <summary>
        /// Создает текстовые строки для всплывающей подсказки при наведении мыши на выделенную нитку.
        /// </summary>
        /// <param name="thToolTip">Экземпляр подскаски, для которой следует сгенерировать текст.</param>
        protected void ToolTipTextGenerator(System.Windows.Controls.ToolTip thToolTip)
        {
            //Проверка, наведена ли мышь на станцию.
            int NumberStation = MasterTrainPaths.MasterDiagram.OY.Stations.FindIndex(thStation => Math.Abs(thStation.StationLineWorkArea.Margin.Top - Mouse.GetPosition(this).Y) <= PenGiver.ThicknessPhantomPen);
            if (NumberStation > -1)
            {
                //Если наведена, то генеририрует текст из названия станции, на которую указывает мышь, времен прибытия, отправления и СРС.
                thToolTip.Content = "Станция " + MasterTrainPaths.MasterDiagram.OY.Stations[NumberStation].Content + Environment.NewLine;
                //Если нитка идет снизу вверх, то нужно искать элемент расписания в обратном порядке, так как порядок нумерации станций оказывается отличным от нумерации элементов расписания.
                if (LogicalTrainPath.direction.value == DirectionValue.ODD) NumberStation = MasterTrainPaths.MasterDiagram.OY.Stations.Count - 1 - NumberStation;
                thToolTip.Content = thToolTip.Content + "Приб: " + TimeConverter.SecondsToTime(LogicalTrainPath.MasElRasp[NumberStation].arrivalTime).TimeOfDay.ToString() + Environment.NewLine;
                thToolTip.Content = thToolTip.Content + "Отпр: " + TimeConverter.SecondsToTime(LogicalTrainPath.MasElRasp[NumberStation].departureTime).TimeOfDay.ToString();
                // int SRV = LogicalTrainPath.MasElRasp[NumberStation].SRV.dTStopArrival;
                //Если СРС отлична от нуля, то выводим и ее тоже.
                if (LogicalTrainPath.MasElRasp[NumberStation].TimeStoyanOtprav > 0)
                {
                    thToolTip.Content = thToolTip.Content + Environment.NewLine + "СРС: " + TimeConverter.SecondsToTime(LogicalTrainPath.MasElRasp[NumberStation].TimeStoyanOtprav).TimeOfDay.ToString();
                }
            }
            else
            {
                //В противном случае мышь наведена на межстанционный интервал, поэтому ищем на какой именно.
                NumberStation = MasterTrainPaths.MasterDiagram.OY.Stations.FindIndex(thStation => thStation.StationLineWorkArea.Margin.Top > Mouse.GetPosition(this).Y);
                if (NumberStation > 0)
                {
                    //Зарезервированная строка, где должен быть номер поезда, которому принадлежит данная визуальная нить.
                    thToolTip.Content = "Поезд №" + LogicalTrainPath.trainNumber + Environment.NewLine;

                    //Приверяем направление нитки, снизу вверх или сверху вниз.
                    if (LogicalTrainPath.direction.value == DirectionValue.ODD)
                    {
                        //Если снизу вверх то вывод названий станций в графе "Задание" нужно делать наоборот.
                        thToolTip.Content = thToolTip.Content + "Задание " + MasterTrainPaths.MasterDiagram.OY.Stations[NumberStation].Content + "-" + MasterTrainPaths.MasterDiagram.OY.Stations[NumberStation - 1].Content + Environment.NewLine;
                        //Элемент расписания получается так же в обратном порядке, так как порядок нумерации станций оказывается отличным от нумерации элементов расписания.
                        NumberStation = MasterTrainPaths.MasterDiagram.OY.Stations.Count - NumberStation;
                    }
                    //Если сверху вниз то вывод названий станций в графе "Задание" нужно делать в "прямом" порядке.
                    else thToolTip.Content = thToolTip.Content + "Задание " + MasterTrainPaths.MasterDiagram.OY.Stations[NumberStation - 1].Content + "-" + MasterTrainPaths.MasterDiagram.OY.Stations[NumberStation].Content + Environment.NewLine;
                    //Вывод времен для перегона.
                    thToolTip.Content = thToolTip.Content + TimeConverter.SecondsToTime(LogicalTrainPath.MasElRasp[NumberStation - 1].departureTime).TimeOfDay.ToString() + "-" + TimeConverter.SecondsToTime(LogicalTrainPath.MasElRasp[NumberStation].arrivalTime).TimeOfDay.ToString();
                }
            }
        }

        //Обработка поведения нитки Begin
        public void WorkWithTrainPath()
        {
            MouseLeftButtonDown += new MouseButtonEventHandler(MouseLeftButtonDownTrainPath);
            MouseLeftButtonUp += new MouseButtonEventHandler(MouseLeftButtonUpTrainPath);
            MouseMove += new MouseEventHandler(ToolTipGenerator);
        }

        private void MouseLeftButtonDownTrainPath(object sender, MouseButtonEventArgs e)
        {
            switch (MasterTrainPaths.Instrument)
            {
                case Instruments.EditLengthOfTrainPath:
                    break;
                case Instruments.CreateOrDeleteTrainPath:
                    DeletionTrainPath DeleteEditor = new DeletionTrainPath(LogicalTrainPath, MasterTrainPaths);
                    if (DeleteEditor.Check()) DeleteEditor.Do();
                    break;
                case Instruments.CreateOrDeleteСonnectionTrainPaths:
                    TrainPath OneTrainPath;
                    switch (Condition)
                    {
                        case ConditionTrainPath.PrimarySelected:
                            DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(this);
                            break;
                        case ConditionTrainPath.Free:
                            SelectPrimaryTrainPathEditor.SelectPrimaryTrainPath(this);

                            if (MasterTrainPaths.PrimarySelectedTrainPaths.Count > 1)
                            {
                                OneTrainPath = MasterTrainPaths.PrimarySelectedTrainPaths.First();
                                TrainPath TwoTrainPath = MasterTrainPaths.PrimarySelectedTrainPaths.Last();

                                DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(OneTrainPath);
                                DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(TwoTrainPath);

                                try
                                {
                                    СonnectionTrainPaths СonnectionTrainPaths = new СonnectionTrainPaths(OneTrainPath.LogicalTrainPath, TwoTrainPath.LogicalTrainPath, Core.AbstractTailGiver.NamesTails.LinkedTail, MasterTrainPaths);
                                    СonnectionTrainPaths.Do();
                                }
                                catch (TheOperationIsNotFeasible ane)
                                {
                                    MessageBox.Show(String.Format("State = {0}", ane.State), "Привет, Вася!", MessageBoxButton.OK, MessageBoxImage.Error);
                                }
                            }
                            break;
                        case ConditionTrainPath.SecondarySelected:
                            OneTrainPath = MasterTrainPaths.PrimarySelectedTrainPaths.First();

                            DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(OneTrainPath);

                            try
                            {
                                DisconnectionTrainPaths DeleteСonnectionTrainPaths = new DisconnectionTrainPaths(OneTrainPath.LogicalTrainPath, LogicalTrainPath, Core.AbstractTailGiver.NamesTails.LinkedTail, MasterTrainPaths);
                                DeleteСonnectionTrainPaths.Do();
                            }
                            catch (TheOperationIsNotFeasible ane)
                            {
                                MessageBox.Show(String.Format("State = {0}", ane.State), "Привет, Вася!", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                            break;
                        default:
                            throw new System.ArgumentException("Такой ConditionTrainPath вариант не предусмотрен.", "original");
                            break;
                    }
                    break;
                case Instruments.EditSchedule:
                    Panel.SetZIndex(this, 100000);
                    if (Condition == ConditionTrainPath.PrimarySelected)
                    {
                        StartPointPositionMouse = Mouse.GetPosition(this);
                        int ndx = Node.FindIndex(thNode => Math.Pow(thNode.Y - StartPointPositionMouse.Y, 2) + Math.Pow(thNode.X - StartPointPositionMouse.X, 2) < Math.Pow(PenGiver.ThicknessPhantomPen, 2));
                        if (ndx > -1)
                        {
                            StartPointPositionMouse = Node[ndx];
                            StartPointPositionMouse.Offset(Margin.Left, 0);
                            MouseMove += new MouseEventHandler(MouseMoveSRVPoint);
                        }
                        else
                        {
                            StartPointPositionMouse.Offset(Margin.Left, 0);
                            MouseMove += new MouseEventHandler(MouseMoveAllTrainPath);
                        }
                    }
                    ThicknessPhantomPen = PenGiver.WorkThicknessPhantomPen;
                    break;
                default:
                    MessageBox.Show("Unknown Instrument in TrainPath class метод MouseButtonDownTrainPath");
                    break;
            }
            InvalidateVisual();
            MasterTrainPaths.ScheduleWindowUpdate();
        }

        private void MouseLeftButtonUpTrainPath(object sender, MouseButtonEventArgs e)
        {
            switch (MasterTrainPaths.Instrument)
            {
                case Instruments.EditLengthOfTrainPath:
                    //Смотрит состояние нитки. (Выделена ли она, и если да, то какаим образом)
                    switch (Condition)
                    {
                        case ConditionTrainPath.Free:
                            //Делает выделенной первчино ниткой только себя. 
                            SelectOnlySelf();
                            break;
                        case ConditionTrainPath.PrimarySelected:
                            //Если нитка выделена как первичная, то проверяет нажата ли клавиша "левый контрол".
                            if (Keyboard.IsKeyDown(Key.LeftCtrl))
                            {
                                //Если "левый контрол" нажат, то начинает готовится к изменению длинны нитки.
                                //Поиск номера редактируемой станции по условию.
                                int NumberStation = Node.FindIndex(thNode => Math.Abs(thNode.Y - Mouse.GetPosition(this).Y) <= PenGiver.ThicknessPhantomPen);
                                //Проверяет нашлась ли станция удволетворяющая условиям.
                                if (NumberStation > -1)
                                {
                                    NumberStation = NumberStation / 2 + LogicalTrainPath.NumFirst;

                                    //Выбор необходимой операции. (Между урезанием нитки или добавлением к ней новых станций)
                                    if (LogicalTrainPath.NumLast != NumberStation)
                                    {
                                        //Строка снизу для того чтобы не было багов с выделением лишних ниток, если они были связаны.
                                        DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(this);
                                        //Удалет станции.
                                        LengthOfTrainPathWithDeleteConnectionTrainPath LengthOfTrainPathEditor = new LengthOfTrainPathWithDeleteConnectionTrainPath(LogicalTrainPath, MasterTrainPaths);
                                        if (LengthOfTrainPathEditor.Check(true, (byte)NumberStation))
                                        {
                                            LengthOfTrainPathEditor.Do();
                                        }
                                        //Строка с низу для того чтобы не было багов с выделением лишних ниток, если они были связаны.
                                        SelectPrimaryTrainPathEditor.SelectPrimaryTrainPath(this);
                                    }
                                }
                            }
                            else
                            {
                                //Если клавиша "левый контрол" не нажата, то девыделяет нитку.
                                DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(this);
                            }
                            break;
                        case ConditionTrainPath.SecondarySelected:
                            //Делает выделенной первчино ниткой только себя. 
                            SelectOnlySelf();
                            break;
                        default:
                            throw new System.ArgumentException("Такой ConditionTrainPath вариант не предусмотрен. Класс TrainPath.", "original");
                            break;
                    }
                    break;
                case Instruments.CreateOrDeleteTrainPath:
                    break;
                case Instruments.CreateOrDeleteСonnectionTrainPaths:
                    break;
                case Instruments.EditSchedule:
                    MouseMove -= MouseMoveSRVPoint;
                    MouseMove -= MouseMoveAllTrainPath;

                    if (MasterTrainPaths.PrimarySelectedTrainPaths.Count() == 0 || MasterTrainPaths.PrimarySelectedTrainPaths.Count() == 1 && Condition == ConditionTrainPath.PrimarySelected && !isModificated)
                    {
                        //Проверяет выделена ли нитка.
                        if (Condition == ConditionTrainPath.Free)
                        {
                            //Если нитка свободна, то выделяет ее.
                            SelectPrimaryTrainPathEditor.SelectPrimaryTrainPath(this);
                        }
                        else
                        {
                            //Если нитка выделена первично, то освобождает ее.
                            DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(this);
                        }
                    }
                    else
                    {
                        if (LogicalTrainPath.direction == MasterTrainPaths.PrimarySelectedTrainPaths[0].LogicalTrainPath.direction && !MasterTrainPaths.SecondarySelectedTrainPaths.Equals(this) && !isModificated)
                        {
                            //Проверяет есть ли уже выделенные нитки.
                            if ((MasterTrainPaths.PrimarySelectedTrainPaths.Count() > 0))
                            {
                                //Если выделенные нитки уже есть, то проверяет состояние текущей.
                                switch (Condition)
                                {
                                    case ConditionTrainPath.PrimarySelected:
                                        //В случае, если текущая выделена первично, то готовится к девыделению. А именно, начинает поиск нитки с наименьшим временем начала и с наибольшим.
                                        TrainPath FirstTimeStartFirstStation = MasterTrainPaths.PrimarySelectedTrainPaths.First();
                                        TrainPath SecondTimeStartFirstStation = MasterTrainPaths.PrimarySelectedTrainPaths.First();
                                        for (int Number = 1; Number < MasterTrainPaths.PrimarySelectedTrainPaths.Count; Number++)
                                        {
                                            if (FirstTimeStartFirstStation.LogicalTrainPath.DepartureTimeFromFirstStation > MasterTrainPaths.PrimarySelectedTrainPaths[Number].LogicalTrainPath.DepartureTimeFromFirstStation)
                                            {
                                                FirstTimeStartFirstStation = MasterTrainPaths.PrimarySelectedTrainPaths[Number];
                                            }
                                            if (SecondTimeStartFirstStation.LogicalTrainPath.DepartureTimeFromFirstStation < MasterTrainPaths.PrimarySelectedTrainPaths[Number].LogicalTrainPath.DepartureTimeFromFirstStation)
                                            {
                                                SecondTimeStartFirstStation = MasterTrainPaths.PrimarySelectedTrainPaths[Number];
                                            }
                                        }
                                        //Если текущая нитка является первой или последней по времени среди выделенных, то начинает девыделение.
                                        if (this == SecondTimeStartFirstStation || this == FirstTimeStartFirstStation)
                                        {
                                            DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPathsWithoutBorder(FirstTimeStartFirstStation, SecondTimeStartFirstStation);
                                            DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(this);
                                        }
                                        break;
                                    case ConditionTrainPath.Free:
                                        //Если нитка свободна, то  начинает выделение всех ниток, которые подходят под интервал между уже выделенной и текущей.
                                        SelectPrimaryTrainPathEditor.SelectPrimaryTrainPathsWithBorder(MasterTrainPaths.PrimarySelectedTrainPaths.First(), this);
                                        break;
                                    case ConditionTrainPath.SecondarySelected:
                                        //Не верный вариант, потому ничего не делаем.
                                        break;
                                    default:
                                        throw new System.ArgumentException("Такой ConditionTrainPath вариант не предусмотрен.", "original");
                                        break;
                                }
                            }
                        }
                    }
                    isModificated = false;

                    ThicknessPhantomPen = PenGiver.ThicknessPhantomPen;
                    Panel.SetZIndex(this, 9000);
                    InvalidateVisual();
                    break;
                default:
                    MessageBox.Show("Unknown Instrument in TrainPath class метод MouseButtonUpTrainPath");
                    break;
            }
            MasterTrainPaths.ScheduleWindowUpdate();
        }

        private void MouseMoveSRVPoint(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point NewPointPositionMouse = Mouse.GetPosition(this);
                NewPointPositionMouse.Offset(Margin.Left, 0);
                int DeltaTime;
                DeltaTime = TimeConverter.PixelsToSeconds((int)((NewPointPositionMouse.X - StartPointPositionMouse.X) / MasterTrainPaths.MasterDiagram.Zoom), true);
                //DeltaTime = DeltaTime - DeltaTime % 5;

                if (!isModificated)
                {
                    //Делает выделенной первично ниткой только себя. 
                    SelectOnlySelf();
                    int ndx = Node.FindIndex(thNode => Math.Pow((thNode.Y - StartPointPositionMouse.Y), 2) + Math.Pow((thNode.X - (StartPointPositionMouse.X - Margin.Left)), 2) < Math.Pow(14, 2));
                    StationOvertimeEditor OvertimeEditor;
                    if (ndx % 2 == 0)
                    {
                        //
                        ndx = LogicalTrainPath.NumFirst + ndx / 2;
                        // MessageBox.Show("a" + ndx);
                        OvertimeEditor = new StationOvertimeEditor(LogicalTrainPath, ndx, true, MasterTrainPaths);
                    }
                    else
                    {
                        //
                        ndx = LogicalTrainPath.NumFirst + (ndx - 1) / 2;
                        // MessageBox.Show("b" + ndx);
                        OvertimeEditor = new StationOvertimeEditor(LogicalTrainPath, ndx, false, MasterTrainPaths);
                    }

                    if (OvertimeEditor.Check(DeltaTime))
                    {
                        OvertimeEditor.Do();
                        isModificated = true;
                    }
                }
                else
                {
                    if (MasterTrainPaths.StackAllDoOperation.Peek().Check(DeltaTime))
                    {
                        MasterTrainPaths.StackAllDoOperation.Peek().Do();
                    }
                }
            }
            else
            {
                MouseMove -= MouseMoveSRVPoint;
                EROR_InterfaceRestore();
            }
            MasterTrainPaths.ScheduleWindowUpdate();
        }

        //Обработка видов перемещений ниток, изменения T0 Begin
        private void MouseMoveAllTrainPath(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point NewPointPositionMouse = Mouse.GetPosition(this);
                NewPointPositionMouse.Offset(Margin.Left, 0);

                int DeltaTime = 0;
                DeltaTime = TimeConverter.PixelsToSeconds((int)((NewPointPositionMouse.X - StartPointPositionMouse.X) / MasterTrainPaths.MasterDiagram.Zoom), true);
                //DeltaTime = DeltaTime - DeltaTime % 5;
                if (!isModificated)
                {
                    if (MasterTrainPaths.PrimarySelectedTrainPaths.Count > 1)
                    {
                        List<Core.TrainPath> PrimeryTrainPaths = new List<Core.TrainPath>();
                        foreach (TrainPath thTrainPath in MasterTrainPaths.PrimarySelectedTrainPaths) PrimeryTrainPaths.Add(thTrainPath.LogicalTrainPath);

                        MoveGroupPrimaryTrainPathEditor MoveTrainPaths = new MoveGroupPrimaryTrainPathEditor(PrimeryTrainPaths, MasterTrainPaths);

                        if (MoveTrainPaths.Check(DeltaTime))
                        {
                            MoveTrainPaths.Do();
                            isModificated = true;
                        }
                    }
                    else
                    {
                        MovingPrimaryTrainPath MoveTrainPath = new MovingPrimaryTrainPath(LogicalTrainPath, MasterTrainPaths);

                        if (MoveTrainPath.Check(DeltaTime))
                        {
                            MoveTrainPath.Do();
                            isModificated = true;
                        }
                    }
                }
                else
                {
                    if (MasterTrainPaths.StackAllDoOperation.Peek().Check(DeltaTime))
                    {
                        MasterTrainPaths.StackAllDoOperation.Peek().Do();
                    }
                }
            }
            else
            {
                MouseMove -= MouseMoveAllTrainPath;
                EROR_InterfaceRestore();
            }
            MasterTrainPaths.ScheduleWindowUpdate();
        }

        private void MoveTrainPath(double DeltaTime)
        {
            Margin = new Thickness(Margin.Left + DeltaTime, 0, 0, 0);
            if (TailTrainPath != null)
            {
                TailTrainPath.Margin = new Thickness(TailTrainPath.Margin.Left + DeltaTime, TailTrainPath.Margin.Top, 0, 0);
            }
        }
        //Обработка видов перемещений ниток, изменения T0 End

        private void EROR_InterfaceRestore() //Выполняется в случае обнаруженной рассогласованности интерфейса.   ...Печаль, как сделать лучше пока не знаю...
        {
            PenGiver.ThicknessPhantomPen = 14;
            Panel.SetZIndex(this, 9000);
            if (MasterTrainPaths.PrimarySelectedTrainPaths.Count == 0)
            {
                Condition = ConditionTrainPath.Free;
            }
            isModificated = false;
            InvalidateVisual();
        }

        //Обработка поведения нитки End

        protected override void OnPreviewMouseRightButtonUp(MouseButtonEventArgs e)
        {
            if (Condition == ConditionTrainPath.PrimarySelected && MasterTrainPaths.PrimarySelectedTrainPaths.Count > 0 && MasterTrainPaths.Instrument != Instruments.EditLengthOfTrainPath)
            {
                ContextMenu = new MouseMenuForTrainPath(LogicalTrainPath, MasterTrainPaths);
            }
            else
            {
                this.ContextMenu = null;
            }
            //Проверяет выделена ли текуща нитка, является ли текущий инструмент изменением длинны и проверяет нажата ли клавиша "левый контрол".
            if (Condition == ConditionTrainPath.PrimarySelected && MasterTrainPaths.Instrument == Instruments.EditLengthOfTrainPath && Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                //Ищет станцию, с которой происходит операция.
                int NumberStation = Node.FindIndex(thNode => Math.Abs(thNode.Y - Mouse.GetPosition(this).Y) <= PenGiver.ThicknessPhantomPen);
                if (NumberStation > -1)
                {
                    NumberStation = NumberStation / 2 + LogicalTrainPath.NumFirst;

                    //Выбирает операцию, которую требуется произвести. (Между обрезанием нитки и добавлением новых станций)
                    if (LogicalTrainPath.NumFirst != NumberStation)
                    {
                        //Строка снизу для того чтобы не было багов с выделением лишних ниток, если они были связаны.
                        DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(this);
                        //Укорачивает нитку.
                        LengthOfTrainPathWithDeleteConnectionTrainPath LengthOfTrainPathEdit = new LengthOfTrainPathWithDeleteConnectionTrainPath(LogicalTrainPath, MasterTrainPaths);
                        if (LengthOfTrainPathEdit.Check(false, (byte)NumberStation))
                        {
                            LengthOfTrainPathEdit.Do();
                        }
                        //Строка снизу для того чтобы не было багов с выделением лишних ниток, если они были связаны.
                        SelectPrimaryTrainPathEditor.SelectPrimaryTrainPath(this);
                    }
                    //InvalidateVisual(); Поидее лишняя строка. Пусть пока побудет, если багов не будет, то удалить этот комментарий.
                }
            }
            MasterTrainPaths.ScheduleWindowUpdate();
        }

        /// <summary>
        /// Очищает список первично выделенных ниток в MasterTrainPaths.
        /// </summary>
        private void DeselectAllTrainPaths()
        {
            //Циклически девыделяет все нитки из списка.
            while (MasterTrainPaths.PrimarySelectedTrainPaths.Count > 0)
            {
                DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(MasterTrainPaths.PrimarySelectedTrainPaths.First());
            }
        }

        /// <summary>
        /// Очищает список первчино выделенных ниток в MasterTrainPaths и делает первично выделенной только себя. 
        /// </summary>
        private void SelectOnlySelf()
        {
            //Девыделяет все нитки.
            DeselectAllTrainPaths();
            //Если выделяет первично себя.
            SelectPrimaryTrainPathEditor.SelectPrimaryTrainPath(this);
        }
    }
}
