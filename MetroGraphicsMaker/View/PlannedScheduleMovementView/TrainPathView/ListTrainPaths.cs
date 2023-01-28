using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Actions;

using Exceptions.Actions;
using WpfApplication1;

namespace View
{
    //Область рисования ниток и их хранение.
    public class ListTrainPaths : Canvas
    {
        public List<TrainPath> TrainPaths = new List<TrainPath>();
        public List<ViewTail> TailsTrainPaths = new List<ViewTail>();

        public List<TrainPath> PrimarySelectedTrainPaths = new List<TrainPath>();
        public List<TrainPath> SecondarySelectedTrainPaths = new List<TrainPath>();

        public Diagram MasterDiagram;

        public Stack<BaseEdit> StackAllDoOperation = new Stack<BaseEdit>();
        public Stack<BaseEdit> StackAllUndoOperation = new Stack<BaseEdit>();

        public TrainPath StandartTrainPathODDNonpeak;
        public TrainPath StandartTrainPathEVENNonpeak;
        public TrainPath StandartTrainPathODDPeak;
        public TrainPath StandartTrainPathEVENPeak;

        public TrainPath FirstODD, FirstEVEN;

        public OneTrainPathScheduleWindow ScheduleWindow = null;

        private Instruments _Instrument = Instruments.EditSchedule;
        public Instruments Instrument
        {
            get { return _Instrument; }
            set
            {
                //Девыделяет все выделенные нитки при переходе к другому инструменту.
                DeselectAllPrimaryTrainPaths();

                _Instrument = value;
            }
        }

        public ListTrainPaths(int PannelStationPositionTop, int Left, Diagram _TheDiagram)
        {
            //this.Background = Brushes.Red;
            MasterDiagram = _TheDiagram;
            Height = 0;
            Width = 0;
            Margin = new Thickness(Left, PannelStationPositionTop, 0, 0);
            RenderSize = new Size(5, 5);
        }

        /// <summary>
        /// Изменение масштаба отрисовки TrainPaths.
        /// </summary>
        public void ZoomChangeTrainPaths()
        {
            //Запоминает собственную длинну.
            Height = MasterDiagram.OY.Height;
            //Перерисовывает все нитки и хвосты.
            InvalideteChildrens();
        }

        /// <summary>
        /// Перерисовывает все свои личные объекты, которые обрабатывает лично. (На момент написания, кроме станционных линий на рабочей области)
        /// </summary>
        public void InvalideteChildrens()
        {
            //Перерисовывает нитки.
            foreach (TrainPath thisTrainPath in TrainPaths) thisTrainPath.InvalidateVisual();
            //Перерисовывает хвосты.
            foreach (ViewTail thisTail in TailsTrainPaths) thisTail.InvalidateVisual();
        }

        public void UndoAction()
        {
            while (PrimarySelectedTrainPaths.Count != 0)
            {
                DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(PrimarySelectedTrainPaths.First());
            }
            if (StackAllDoOperation.Count > 0)
            {
                BaseEdit UndoAction = StackAllDoOperation.Pop();
                UndoAction.Undo();
                StackAllUndoOperation.Push(UndoAction);

                ScheduleWindowUpdate();
            }
        }

        public void DoAction()
        {
            while (PrimarySelectedTrainPaths.Count != 0)
            {
                DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(PrimarySelectedTrainPaths.First());
            }
            if (StackAllUndoOperation.Count > 0)
            {
                BaseEdit DoAction = StackAllUndoOperation.Pop();
                DoAction.Do();
                StackAllDoOperation.Push(DoAction);

                ScheduleWindowUpdate();
            }
        }

        /// <summary>
        /// Добавление образа (визуализации) нитки ПГД к коллекции ниток-образов.
        /// </summary>
        /// <param name="item">Новая (добавляемая) нитка-образ.</param>
        public void AddTrainPathView(TrainPath item)
        {
            // Что-то ещё...
            Children.Add(item);
        }

        /// <summary>
        /// Продливает все выделенные первично нитки до заданной станции.
        /// </summary>
        /// <param name="NumberStation">Номер станции до которой следует сделать удлинение ниток.</param>
        public void AddStationsForSelectPrimaryTrainPath(byte NumberStation)
        {
            //Циклически обращается к каждой выделенной первично нитке.
            foreach (TrainPath thTrainPath in PrimarySelectedTrainPaths)
            {
                //Добавляет станции, если это можно сделать.
                ExpandLengthOfTrainPath ExpandLengthOfTrainPathEditor = new ExpandLengthOfTrainPath(thTrainPath.LogicalTrainPath, this);
                if (ExpandLengthOfTrainPathEditor.Check(NumberStation))
                {
                    ExpandLengthOfTrainPathEditor.Do();
                }
            }
        }

        public void ShowScheduleWindow()
        {
            ScheduleWindow = new OneTrainPathScheduleWindow();
            if (PrimarySelectedTrainPaths.Count > 0)
            {
                ScheduleWindow.SetTrainPath(PrimarySelectedTrainPaths.First().LogicalTrainPath);
            }
        }

        public void CloseScheduleWindow()
        {
            ScheduleWindow.Close();
            ScheduleWindow = null;
        }

        public void ScheduleWindowUpdate()
        {
            if (ScheduleWindow != null)
            {
                if (PrimarySelectedTrainPaths.Count == 1)
                {
                    ScheduleWindow.SetTrainPath(PrimarySelectedTrainPaths.First().LogicalTrainPath);
                }
                else
                {
                    ScheduleWindow.SetTrainPath(null);
                }
            }
        }

        public Boolean SelectPrimaryRightTrainPath(TrainPath CurrentTrainPath)
        {
            Boolean isFoundThread = CurrentTrainPath.LogicalTrainPath.RightThread != null;
            if (isFoundThread)
            {
                DeselectAllPrimaryTrainPaths();

                SelectPrimaryTrainPathEditor.SelectPrimaryTrainPath(CurrentTrainPath.LogicalTrainPath.RightThread.ViewTrainPath);
            }
            return isFoundThread;
        }

        public Boolean SelectPrimaryLeftTrainPath(TrainPath CurrentTrainPath)
        {
            Boolean isFoundThread = CurrentTrainPath.LogicalTrainPath.LeftThread != null;
            if (isFoundThread)
            {
                DeselectAllPrimaryTrainPaths();

                SelectPrimaryTrainPathEditor.SelectPrimaryTrainPath(CurrentTrainPath.LogicalTrainPath.LeftThread.ViewTrainPath);
            }
            return isFoundThread;
        }

        public void DeselectAllPrimaryTrainPaths()
        {
            while (PrimarySelectedTrainPaths.Count != 0)
            {
                DeselectPrimaryTrainPathEditor.DeselectPrimaryTrainPath(PrimarySelectedTrainPaths.First());
            }
            ScheduleWindowUpdate();
        }
    }
}
