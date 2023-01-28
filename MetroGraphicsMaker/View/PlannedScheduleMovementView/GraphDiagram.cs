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
    //Описывает отображение ПГД в целом.
    public class Diagram : Canvas
    {
        const Byte StationFantom = 5;
        public ListTrainPaths MasterTrainPaths;
        public PannelStation OY;
        public PannelTime OX;
        public int StartTimeOnDiagram;
        public int FinishTimeOnDiagram;

        public double Zoom = 1;
        Instruments Instrument = Instruments.EditSchedule;

        public int WidthPannelStation = 125;
        public int WindowWidthProgram = 5000;  //Окно программы для начертания горизонтальной линии станции.

        public Diagram()
        {
            StartTimeOnDiagram = 0;
            FinishTimeOnDiagram = 0;
            OY = new PannelStation(PannelTimeConstant.HeightPannelTime, this);
            OX = new PannelTime(PannelTimeConstant.HeightPannelTime, WidthPannelStation, this);
            MasterTrainPaths = new ListTrainPaths(PannelTimeConstant.HeightPannelTime, WidthPannelStation, this);

            Margin = new Thickness(0, 0, 0, 0);
            Background = Brushes.White;
            Height = PannelTimeConstant.HeightPannelTime * 2;
            Width = WidthPannelStation;

            Children.Add(OX);
            Children.Add(OY);
            Children.Add(MasterTrainPaths);
        }

        public void InputWidthHeightDiagram(int _StartTimeOnDiagram, int _FinishTimeOnDiagram)
        {
            StartTimeOnDiagram = (int)TimeConverter.SecondsToTime(_StartTimeOnDiagram).TimeOfDay.TotalSeconds;
            FinishTimeOnDiagram = _FinishTimeOnDiagram;

            //TimeConverter.MovementTime.begin = StartTimeOnDiagram; //Эту строку раскомментировать для ошибки.

            OX.Width = TimeConverter.SecondsToPixels((FinishTimeOnDiagram) / TimeConverter.timeScale);
            OX.NewTimeLines();

            MasterTrainPaths.Width = TimeConverter.SecondsToPixels((FinishTimeOnDiagram) / TimeConverter.timeScale);

            Height = OY.Height + PannelTimeConstant.HeightPannelTime * 2;
            Width = OX.Width + WidthPannelStation;
            OY.InvalidateStation();
            MasterTrainPaths.InvalideteChildrens();
        }

        private void MouseCreateNewTrainPath(object sender, MouseEventArgs e)
        {
            Point NewPointPositionMouse = Mouse.GetPosition(this);
            NewPointPositionMouse.X = (NewPointPositionMouse.X - MasterTrainPaths.Margin.Left) / Zoom;

            Direction direction = (NewPointPositionMouse.Y <= PannelTimeConstant.HeightPannelTime + OY.Stations.First().StationLineWorkArea.Margin.Top + StationFantom)
               ? MovementSchedule.CurrentLine.evenDirection
               : MovementSchedule.CurrentLine.oddDirection;
            var trainPath = new CreationTrainPath(MasterTrainPaths, direction);
            if (trainPath.Check(TimeConverter.PixelsToSeconds((int)NewPointPositionMouse.X)))
                trainPath.Do();
        }

        /// <summary>
        /// Обработчик события удлинения нитки.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MouseExpandLengthTrainPath(object sender, MouseEventArgs e)
        {
            // Проверяет нажат ли "левый контрол".
            if (Keyboard.IsKeyDown(Key.LeftCtrl))
            {
                //Если "левый контрол" нажат, то ищет станцию на которую нажал пользователь.
                int NumberStation = OY.Stations.FindIndex(thStation => (Math.Abs(OY.Margin.Top + thStation.StationLineWorkArea.Margin.Top - Mouse.GetPosition(this).Y) <= StationFantom));
                //Проверяет нашлась ли станция.
                if (NumberStation >= 0)
                {
                    //Если станция нашлась, то запрашивает у MasterTrainPaths добавление станций ниткам.
                    MasterTrainPaths.AddStationsForSelectPrimaryTrainPath((byte)NumberStation);
                }
            }
        }

        public void HorizontalSrollMove(double PositionX)
        {
            OY.HorizonalScrollMoveAllPanel(PositionX);
        }

        public void ZoomChange(double ZoomCoefficient)
        {
            Zoom = ZoomCoefficient;
            OX.ZoomChangeTimes();
            Width = OX.Width + WidthPannelStation;
            OY.ZoomChangeStations();
            MasterTrainPaths.ZoomChangeTrainPaths();
            Height = OY.Height + PannelTimeConstant.HeightPannelTime * 2;
        }

        /// <summary>
        /// Выбор инструмента работы с диаграммой.
        /// </summary>
        /// <param name="Tool">Название инструмента.</param>
        public void SelectTool(Instruments Tool)
        {
            //Запоминает выбор инструмента для ниток.
            MasterTrainPaths.Instrument = Tool;

            //Сбрасывает события у ниток. (Убирает добавленные процедуры)
            SetDefaultStationsUserEvent();
            //В зависимости от выбранного инструмента делает подготовку к дальнейшей работе.
            switch (Tool)
            {
                case Instruments.CreateOrDeleteTrainPath:
                    {
                        //Если выбран инструмент "создания ниток", то создает для крайних на графике станций события, срабатыващие по щелчку мыши.
                        if (Instrument != Instruments.CreateOrDeleteTrainPath && OY.Stations.Count != 0)
                        {
                            //Событие для верхней станции.
                            OY.Stations[0].StationLineWorkArea.MouseLeftButtonDown += new MouseButtonEventHandler(MouseCreateNewTrainPath);
                            //Событие для нижней станции.
                            OY.Stations[OY.Stations.Count - 1].StationLineWorkArea.MouseLeftButtonDown += new MouseButtonEventHandler(MouseCreateNewTrainPath);
                        }
                    }
                    break;
                case Instruments.EditLengthOfTrainPath:
                    {
                        //Если выбран инструмент "изменения длинны ниток", то создает обработчики событий для каждой из станций.
                        if (Instrument != Instruments.EditLengthOfTrainPath)
                        {
                            //Циклически добавляет обработчики событий для удлинения ниток.
                            for (int IndexStation = 0; IndexStation < OY.Stations.Count; IndexStation++)
                            {
                                OY.Stations[IndexStation].StationLineWorkArea.MouseLeftButtonDown += new MouseButtonEventHandler(MouseExpandLengthTrainPath);
                            }
                        }
                    }
                    break;
                case Instruments.EditSchedule:
                    break;
                case Instruments.CreateOrDeleteСonnectionTrainPaths:
                    break;
                default:
                    //Если инструмент неизвестен, то генерирует исключение.
                    throw new System.ArgumentException("Такой интструмент не предусмотрен.", "original");
                    break;
            }

            //Запоминает выбранный инструмент.
            Instrument = Tool;
        }

        /// <summary>
        /// Отвязывает обработчики пользовательских событий от станций.
        /// </summary>
        protected void SetDefaultStationsUserEvent()
        {
            //Учитывая какой инструмент выбран в данный момент, выбирает необходимые действия по сбрасыванию событий со станций.
            switch (Instrument)
            {
                case Instruments.CreateOrDeleteTrainPath:
                    {
                        //Если выбран инструмент "Создания ниток", то отвязывает обработчки событий от первой и последней станции.
                        if (OY.Stations.Count != 0)
                        {
                            //Отвязывает событие от первой станции.
                            OY.Stations[0].StationLineWorkArea.MouseLeftButtonDown -= new MouseButtonEventHandler(MouseCreateNewTrainPath);
                            //Отвязывает событие от второй станции.
                            OY.Stations[OY.Stations.Count - 1].StationLineWorkArea.MouseLeftButtonDown -= new MouseButtonEventHandler(MouseCreateNewTrainPath);
                        }
                    }
                    break;
                case Instruments.EditLengthOfTrainPath:
                    {
                        //Если выбран инструмент "изменение длинны нитки", то циклически отвязывает события от всех станций.
                        for (int IndexStation = 0; IndexStation < OY.Stations.Count; IndexStation++)
                        {
                            OY.Stations[IndexStation].StationLineWorkArea.MouseLeftButtonDown -= new MouseButtonEventHandler(MouseExpandLengthTrainPath);
                        }
                    }
                    break;
                case Instruments.EditSchedule:
                    break;
                case Instruments.CreateOrDeleteСonnectionTrainPaths:
                    break;
                default:
                    //Если инструмент неизвестен, то генерирует исключение.
                    throw new System.ArgumentException("Такой интструмент не предусмотрен.", "original");
                    break;
            }
        }

        public void DoAction()
        {
            MasterTrainPaths.DoAction();
        }

        public void UndoAction()
        {
            MasterTrainPaths.UndoAction();
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            //MessageBox.Show("ZZZ");
            //this.Focusable = true;
            base.OnMouseLeave(e);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void AddTrainPathView(Core.TrainPath item)
        {
            MasterTrainPaths.AddTrainPathView(new TrainPath(item));
        }
    }
}
