using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Documents.DocumentStructures;

using Core;
using View;
using Forms.AutomationWindows;
using Exceptions.Actions;

namespace Actions.Processes
{
    public enum TransitionResultDoOperation
    {
        Good = 0,
        Bad = 1
    }

    /// <summary>
    /// 
    ///
    /// </summary>
    public sealed class TransitionProcess : BaseEdit
    {
        private Int32 _necessaryPairsForMorningPeak;
        private Int32 _intervalTimeForMorningPeak;
        private Int32 _totalTrainsForMorningPeak;
        private Int32 _necessaryTrainsForNoPeak;
        private Int32 _necessaryPairsForNoPeak;
        private Int32 _intervalTimeForNoPeak;
        private Int32 _startTimeDG;
        private Int32 _endTimeDG;
        private List<ResultForTransitionProcess> _transitionResults;
        private Stack<BaseEdit> MovePrimaryTrainPathEditor = new Stack<BaseEdit>();

        public class Solver
        {
            Stack<Int32> dividends = new Stack<Int32>();
            Stack<Int32> divisors = new Stack<Int32>();
            Stack<Int32> quotients = new Stack<Int32>();
            Stack<Int32> remainders = new Stack<Int32>();

            public void MakeStacks(int firstNumber, int secondNumber)
            {
                dividends.Push(firstNumber);
                divisors.Push(secondNumber);

                while (divisors.Peek() > 0)
                {
                    quotients.Push(dividends.Peek() / divisors.Peek());
                    remainders.Push(dividends.Peek() % divisors.Peek());
                    dividends.Push(divisors.Peek());
                    divisors.Push(remainders.Peek());
                };

                quotients.Push(0);
                remainders.Push(0);
            }

            public void MakeBody(Int32[] res)
            {
                var localDivisors = divisors.Reverse().ToArray();

                var result = new Int32[localDivisors[0]];
                var prevResult = new Int32[localDivisors[0]];
                prevResult[0] = 0;
                prevResult[1] = quotients.Pop();
                result[0] = 0;
                var localDivisor = divisors.Pop();
                do
                {
                    var j = 1;
                    var localQuotient = quotients.Pop();
                    localDivisor = divisors.Pop();
                    for (int i = 1; i < localDivisor; ++i)
                    {
                        if (prevResult[j] == i)
                        {
                            result[i] = result[i - 1] + localQuotient + 1;
                            ++j;
                        }
                        else
                        {
                            result[i] = result[i - 1] + localQuotient;
                        };
                        //MessageBox.Show(result[i].ToString());
                    };
                    for (int i = 0; i < localDivisor; ++i)
                    {
                        prevResult[i] = result[i];
                        res[i] = result[i];
                    }
                }
                while (divisors.Count > 0);
            }
            public int CommonFactor(int firstNum, int secondNum)
            {
                while (secondNum != 0)
                {
                    var remainder = firstNum % secondNum;
                    firstNum = secondNum;
                    secondNum = remainder;
                }
                return firstNum;
            }

        }
        public TransitionProcess(Int32 necessaryPairsForMorningPeak, Int32 intervalTimeForMorningPeak, Int32 totalTrainsForMorningPeak, Int32 necessaryTrainsForNoPeak, Int32 necessaryPairsForNoPeak, Int32 intervalTimeForNoPeak, Int32 startTimeDG, Int32 endTimeDG, List<ResultForTransitionProcess> transitionResults, ListTrainPaths TransitionTrainPaths)
            : base(null, TransitionTrainPaths)
        {
            _necessaryPairsForMorningPeak = necessaryPairsForMorningPeak;
            _intervalTimeForMorningPeak = intervalTimeForMorningPeak;
            _totalTrainsForMorningPeak = totalTrainsForMorningPeak;
            _necessaryTrainsForNoPeak = necessaryTrainsForNoPeak;
            _necessaryPairsForNoPeak = necessaryPairsForNoPeak;
            _intervalTimeForNoPeak = intervalTimeForNoPeak;
            _startTimeDG = startTimeDG;
            _endTimeDG = endTimeDG;
            _transitionResults = new List<ResultForTransitionProcess>(transitionResults);
        }

        public void AnalyzeTransition(TransitionResultDoOperation resultTransition)
        {
            switch (resultTransition)
            {
                case TransitionResultDoOperation.Good:
                    var resultShow = MessageBox.Show("Да - показать, нет - продолжить", "Заголовок", MessageBoxButton.YesNoCancel, MessageBoxImage.Information);
                    switch (resultShow)
                    {
                        case MessageBoxResult.Yes:
                            MessageBox.Show("Показываем");
                            break;
                        case MessageBoxResult.No:
                            MessageBox.Show("Продолжаем");
                            break;
                        case MessageBoxResult.Cancel:
                            MessageBox.Show("Отменяем");
                            Undo();
                            break;
                    }
                    break;
                case TransitionResultDoOperation.Bad:
                    return;
            }
        }

        public TransitionResultDoOperation TransitionMyDo(int i)
        {
            TransitionDataPrepare(0, _totalTrainsForMorningPeak);
            TransitionDataEmploy(0, _totalTrainsForMorningPeak);
            return TransitionResultDoOperation.Good;
        }

        ///<summary>
        ///Подготовка данных о этапах переходного процесса
        /// </summary>
        public TransitionResultDoOperation TransitionDataPrepare(int i, int startTotalTrains)
        {
            var solver01 = new Solver();
            solver01.MakeStacks(startTotalTrains, _transitionResults[i].TrainsByFirstWay + _transitionResults[i].TrainsBySecondWay);
            solver01.MakeBody(_transitionResults[i].Results);
            _transitionResults[i].CommonFactorResult = solver01.CommonFactor(startTotalTrains, _transitionResults[i].TrainsByFirstWay + _transitionResults[i].TrainsBySecondWay);
            if (i < _transitionResults.Count - 1)
            {
                TransitionDataPrepare(i + 1, startTotalTrains - _transitionResults[i].TrainsByFirstWay - _transitionResults[i].TrainsBySecondWay);
            }
            else
            {
                MessageBox.Show("i (Data Prepare) greater than _transitionResults.Count");
            }
            return TransitionResultDoOperation.Good;
        }


        ///<summary>
        ///Укорачиваем выбранную группу равномерно расположенных ниток 
        ///</summary>
        public TransitionResultDoOperation TransitionDecrease(Core.TrainPath startTrainPath, int i)
        {
            LengthOfTrainPath CutTrainPath;
            CreationTrainPathLastTail CreateTail;
            Core.TrainPath currentTrainPath = startTrainPath;
            for (int j = 2; j < _transitionResults[i].TrainsByFirstWay + _transitionResults[i].TrainsBySecondWay; j++)
            {
                CutTrainPath = new LengthOfTrainPath(currentTrainPath, currentTrainPath.ViewTrainPath.MasterTrainPaths);

                if (CutTrainPath.Check(true, 5)) //true, потому что точки точно удалаются справа. 8-костыль.
                {
                    CutTrainPath.Do();

                    CreateTail = new CreationTrainPathLastTail(currentTrainPath, AbstractTailGiver.NamesTails.SingleTail, "LowEnd", currentTrainPath.ViewTrainPath.MasterTrainPaths);
                    if (CreateTail.Check())
                    {
                        CreateTail.Do();
                    }
                }
                else
                {
                    state = ActionState.CANT_EXECUTE;
                    throw new TheOperationIsNotFeasible(ActionState.CANT_EXECUTE);
                }

                // TODO Добавить инициализацию, исполнение и запись в коллекцию исполненных действий по укорачиванию нитки

                //MovementSchedule.colDepot(0).stationOnFirstWay

                Core.TrainPath nextTrainPath = currentTrainPath.toGetRightPoezd(_transitionResults[i].Results[j] - _transitionResults[i].Results[j - 1], Direction.FlgRez.ALL);
                if (nextTrainPath == null)
                {
                    state = ActionState.CANT_EXECUTE;
                    throw new TheOperationIsNotFeasible(ActionState.CANT_EXECUTE);
                }
                currentTrainPath = nextTrainPath;
            }
            return TransitionResultDoOperation.Good;
        }

        ///<summary>
        ///Обработка данных на этапах переходного процесса 
        ///</summary>
        public TransitionResultDoOperation TransitionDataEmploy(int i, int startTotalTrains)
        {
            Core.TrainPath currentStartTrainPath = null;
            Core.TrainPath beforeLeftmostThread = null;
            Core.TrainPath leftmostThread = null;
            Core.TrainPath rightmostThread = null;
            Core.TrainPath afterRightmostThread = null;
            //Ищем первую нитку внутри заданного интервала времени, равного длительности полного оборота

            Direction selectedDirection = MovementSchedule.CurrentLine.oddDirection;

            selectedDirection.toGetTrainPathesIntoTimeInterval(_transitionResults[i].StartTimeTransitionProcess - 200, _transitionResults[i].EndTimeTransitionProcess,
            0, Direction.FlagPribOtpr.Departure, Direction.FlgRez.ExcludingRezerv, ref beforeLeftmostThread, ref leftmostThread, ref rightmostThread, ref afterRightmostThread);

            //Просматриваем все варианты начальных позиций равномерного снятия составов для текущего интервала времени, равного длительности полного оборота. Для исключения возможности рассмотрения повторяющихся вариантов учитываем наличие наибольшего общего делителя, большего единицы

            currentStartTrainPath = (leftmostThread == null) ? ((beforeLeftmostThread == null) ? afterRightmostThread : beforeLeftmostThread) : leftmostThread;

            if (currentStartTrainPath == null)
            {
                state = ActionState.CANT_EXECUTE;
                throw new TheOperationIsNotFeasible(ActionState.CANT_EXECUTE);
            }

            for (int y = 0; y < startTotalTrains / _transitionResults[i].CommonFactorResult; y++)
            {
                //Для текущего интервала времени, равного длительности полного оборота, проводим равномерное снятие составов с заданной начальной позиции                 
                TransitionDecrease(currentStartTrainPath, i);

                //После выполнения равномерного снятия составов для текущего интервала времени, переходим к следующему, если он существует                  
                if (i < _transitionResults.Count - 1)
                {
                    TransitionDataEmploy(i + 1, startTotalTrains - _transitionResults[i].TrainsByFirstWay - _transitionResults[i].TrainsBySecondWay);
                }
                else
                {
                    MessageBox.Show("i (DataEmploy) greater than _transitionResults.Count");
                }
                currentStartTrainPath = currentStartTrainPath.RightThread;
            }

            return TransitionResultDoOperation.Good;
        }

        public override ActionState Undo()
        {
            while (MovePrimaryTrainPathEditor.Count > 0)
            {
                MovePrimaryTrainPathEditor.Pop().Undo();
            }

            return ActionState.CANCELLED;
        }

    }

}


