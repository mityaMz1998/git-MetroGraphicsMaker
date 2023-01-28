using System;
using System.Collections.Generic;
using System.Windows;
using Core;
using View;
using Exceptions.Actions;

namespace Actions.Processes
{
    //public struct StringOperation
    //{
    //    string BeforeMorning = "В период перед утренним пиком на линии должно быть";

    //    string AfterMorning = "К дневному непику на линии должно быть";
    //}

    // В период перед утренним пиком на линии должно быть

    public class StationaryProcess : BaseEdit
    {
        public Int32 _startTime;
        public Int32 _endTime;
        public Int32 _specifiedPair;
        public Int32 _calculatedInterval;
        public Int32 _totalTrain;
        public Int32 _timeFrom1To2;
        public Int32 _timeFrom2To1;
        public Direction _selectedDirection;
        public Int32 _fullTurnaroudTime;

        private Stack<BaseEdit> MovePrimaryTrainPathEditors = new Stack<BaseEdit>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTime">Время начала процесса</param>
        /// <param name="endTime">Время окончания процесса</param>
        /// <param name="specifiedPair">Заданная парность</param>
        /// <param name="calculatedInterval">Расчетный интервал</param>
        /// <param name="totalTrain">Доступное количество составов</param>
        /// <param name="timeFrom1To2">Время оборота с 1-го пути на 2-й</param>
        /// <param name="timeFrom2To1">Время оборота со 2-го пути на 1-й</param> 
        /// <param name="selectedDirection">Направление, с которого начинается построение</param>
        /// <param name="MasterTrainPaths">Доступ к коллекции ниток</param>
        public StationaryProcess(Int32 startTime, Int32 endTime, Int32 specifiedPair, Int32 calculatedInterval, Int32 totalTrain,
                                             Int32 timeFrom1To2, Int32 timeFrom2To1, Direction selectedDirection, ListTrainPaths MasterTrainPaths)
            : base(null, MasterTrainPaths)
        {
            _startTime = startTime;
            _endTime = endTime;
            _specifiedPair = specifiedPair;
            _calculatedInterval = calculatedInterval;
            _totalTrain = totalTrain;
            _timeFrom1To2 = timeFrom1To2;
            _timeFrom2To1 = timeFrom2To1;
            _selectedDirection = selectedDirection;
            _fullTurnaroudTime = CalculateFullTurnaroudTime();
            if (!Check())
            {
                state = ActionState.CANT_EXECUTE;
                throw new TheOperationIsNotFeasible(ActionState.CANT_EXECUTE);
            }
        }

        public Int32 CalculateFullTurnaroudTime()
        {
            return MasterTrainPaths.StandartTrainPathEVENPeak.LogicalTrainPath.MasElRasp
                [MasterTrainPaths.StandartTrainPathEVENPeak.LogicalTrainPath.MasElRasp.Count - 1].arrivalTime + MasterTrainPaths.StandartTrainPathODDPeak.LogicalTrainPath.MasElRasp[MasterTrainPaths.StandartTrainPathODDPeak.LogicalTrainPath.MasElRasp.Count - 1].arrivalTime + _timeFrom1To2 + _timeFrom2To1;
        }

        public override Boolean Check()
        {
            return ((_fullTurnaroudTime / _calculatedInterval) < _totalTrain);
        }

        public void Analyze(ActionState result)
        {
            switch (result)
            {
                case ActionState.DONE:
                    var res = MessageBox.Show("Да - показать, нет - продолжить", "Заголовок", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                    switch (res)
                    {
                        case MessageBoxResult.Yes:
                            MessageBox.Show("Показываем");
                            break;
                        case MessageBoxResult.No:
                            //MessageBox.Show("Продолжаем"); 
                            var transitionWindow = new Forms.AutomationWindows.TransitionProcessWindow(_specifiedPair, _endTime, _totalTrain, _calculatedInterval, "в период перед утренним пиком на линии должно быть", CalculateFullTurnaroudTime());
                            transitionWindow.Show();
                            // var someVar = transitionWindow.necessaryPairsForMorningPeak;
                            break;
                        case MessageBoxResult.Cancel:
                            MessageBox.Show("Отменяем");
                            Undo();
                            break;
                    }
                    break;

                case ActionState.CANT_EXECUTE:
                    return;
            }
        }

        public ActionState MyDo()
        {
            var currentTime = _startTime;
            var currentStartTime = _startTime;
            var currentDirection = _selectedDirection;
            Core.TrainPath currentTrainPath = null;//_selectedDirection.colThreads.Last();

            //2
            for (int i = 1; i <= _totalTrain; i++)
            {
                //3
                currentStartTime += _calculatedInterval;
                currentTime = currentStartTime;
                //4
                //5
                currentTrainPath = null;
                //6
                currentDirection = _selectedDirection;
                //7
                do
                {
                    //8
                    //Инициализация действия "Создание нитки"
                    var trainPathCreator = new CreationTrainPath(MasterTrainPaths, currentDirection);
                    //Проверка возможности выполнения действия "Создание нитки"
                    if (trainPathCreator.Check(currentTime))
                    {
                        //Выполнение действия "Создание нитки"
                        trainPathCreator.Do();
                        //Запись действия "Создание нитки" в стек выполненных действий
                        MovePrimaryTrainPathEditors.Push(trainPathCreator);
                        //9-10
                        //Инициализация действия "Соединение ниток"
                        try
                        {
                            var сonnectionTrainPathsCreator = new СonnectionTrainPaths(currentTrainPath, trainPathCreator.StartTrainPath, Core.AbstractTailGiver.NamesTails.LinkedTail, MasterTrainPaths, false);
                            //9
                            //Проверка возможности выполнения действия "Соединение ниток"
                            //if (СonnectionTrainPathsCreator.Check())
                            //{
                            //10
                            //Выполнение действия "Соединение ниток"
                            сonnectionTrainPathsCreator.Do();
                            //Запись действия ""Соединение ниток"" в стек выполненных действий
                            MovePrimaryTrainPathEditors.Push(сonnectionTrainPathsCreator);
                            //}
                        }
                        catch (TheOperationIsNotFeasible ane)
                        {
                            //                            MessageBox.Show("Не удалось построить оборотную нитку", GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    //11
                    currentTrainPath = trainPathCreator.StartTrainPath;
                    //12
                    currentTime = (Convert.ToInt32(currentTrainPath.MasElRasp[currentTrainPath.NumLast].arrivalTime) + _timeFrom1To2);
                    //13
                    currentDirection = currentDirection.ContrDirection;
                }
                while (currentTime <= _endTime + _fullTurnaroudTime);
            }

            return ActionState.DONE;
        }
        public override ActionState Undo()
        {
            while (MovePrimaryTrainPathEditors.Count > 0)
            {
                MovePrimaryTrainPathEditors.Pop().Undo();
            }

            return ActionState.CANCELLED;
        }
    }
}
