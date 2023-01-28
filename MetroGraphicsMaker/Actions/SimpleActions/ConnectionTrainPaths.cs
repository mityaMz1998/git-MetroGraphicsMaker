using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;
using Core;
using View;
using TrainPath = Core.TrainPath;
using Exceptions.Actions;

namespace Actions
{
    class СonnectionTrainPaths : BaseEdit //Переписать в терминах секунд. 
    {
        private AbstractTailGiver.NamesTails NameTail;
        private TrainPath TrainPathFirst;
        private TrainPath TrainPathSecond;

        private Boolean isModify = false; //Использовалось ли изменение данного типа или нет.

        LinkedTail CreateLogicalTail;

        /// <summary>
        /// Добавлять объект в стек операций MasterTrainPath или нет. True - добавить
        /// </summary>
        public Boolean isAddStack;

        public СonnectionTrainPaths(TrainPath First, TrainPath Second, AbstractTailGiver.NamesTails _NameTail, ListTrainPaths _MasterTrainPaths, Boolean _isAddStack = true)
            : base(First, _MasterTrainPaths)
        {
            NameTail = _NameTail;
            TrainPathFirst = First;
            TrainPathSecond = Second;
            isAddStack = _isAddStack;

            if (!Check())
            {
                state = ActionState.CANT_EXECUTE;
                throw new TheOperationIsNotFeasible(ActionState.CANT_EXECUTE);
            }
            state = ActionState.DONE;
        }

        public override Boolean Check()
        {
            if ((TrainPathFirst == null) || (TrainPathSecond == null))
            {
                return false;
            }
            if (CheckIntersectionTrainPath())
            {
                return false;
            }
            SortTrainPath();

            if (TrainPathFirst.LogicalTail != null || TrainPathSecond.BackThread != null)
            {
                return false;
            }
            return CheckNumbersStation();
        }

        /// <summary>
        /// Проверка на соединяемость ниток с точки зрения на номера нефективных станций.
        /// </summary>
        /// <returns></returns>
        private Boolean CheckNumbersStation()
        {
            if (TrainPathFirst.MasElRasp.Count - (TrainPathFirst.NumLast + 1) != TrainPathSecond.NumFirst)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Устанавливает поля TrainPathFirt и TrainPathSecond в правильное положение
        /// </summary>
        /// <returns></returns>
        private void SortTrainPath()
        {
            if (TrainPathSecond.DepartureTimeFromFirstStation < TrainPathFirst.DepartureTimeFromFirstStation)
            {
                var memTrainPath = TrainPathFirst;
                TrainPathFirst = TrainPathSecond;
                TrainPathSecond = memTrainPath;
            }
        }

        /// <summary>
        /// Проверка на пересечение ниток TrainPathFirst и TrainPathSecond. False - Пересечений нет, True - пересечение есть
        /// </summary>
        /// <returns></returns>
        private Boolean CheckIntersectionTrainPath()
        {
            if (TrainPathFirst.DepartureTimeFromFirstStation > TrainPathSecond.DepartureTimeFromFirstStation && TrainPathFirst.DepartureTimeFromFirstStation < TrainPathSecond.MasElRasp[TrainPathSecond.NumLast].departureTime)
            {
                return true;
            }
            if (TrainPathFirst.DepartureTimeFromFirstStation < TrainPathSecond.DepartureTimeFromFirstStation && TrainPathSecond.DepartureTimeFromFirstStation < TrainPathFirst.MasElRasp[TrainPathFirst.NumLast].departureTime)
            {
                return true;
            }
            return false;
        }


        public override ActionState Do()
        {
            CreateLogicalTail = (LinkedTail)AbstractTailGiver.CreateAbstractTail(AbstractTailGiver.NamesTails.LinkedTail, "LinkedTail", TrainPathFirst, TrainPathFirst.MasElRasp[TrainPathFirst.NumLast].departureTime);
            CreateLogicalTail.DeltaTime = TrainPathSecond.MasElRasp[TrainPathSecond.NumFirst].arrivalTime - CreateLogicalTail.TimeBeginTail;
            CreateLogicalTail.RightLogicalTrainPath = TrainPathSecond;
            ViewTailGiver.CreateViewTail(CreateLogicalTail);
            TrainPathFirst.LogicalTail = CreateLogicalTail;

            TrainPathFirst.ViewTrainPath.TailTrainPath = CreateLogicalTail.ViewAbstractTail;
            MasterTrainPaths.TailsTrainPaths.Add(CreateLogicalTail.ViewAbstractTail);
            MasterTrainPaths.Children.Add(CreateLogicalTail.ViewAbstractTail);
            TrainPathFirst.ViewTrainPath.TailTrainPath.InvalidateVisual();

            TrainPathFirst.NextThread = TrainPathSecond;
            TrainPathSecond.BackThread = TrainPathFirst;

            if (!isModify && isAddStack) //Регистрация в журналах выполненых польхователем операций
            {
                MasterTrainPaths.StackAllDoOperation.Push(this);
                MasterTrainPaths.StackAllUndoOperation.Clear();
            }
            isModify = true;

            return ActionState.DONE;
        }
        //return next
        public override ActionState Undo()
        {
            TrainPathFirst.ViewTrainPath.MasterTrainPaths.TailsTrainPaths.Remove(CreateLogicalTail.ViewAbstractTail);
            TrainPathFirst.ViewTrainPath.MasterTrainPaths.Children.Remove(CreateLogicalTail.ViewAbstractTail);

            TrainPathFirst.ViewTrainPath.TailTrainPath = null;
            TrainPathFirst.LogicalTail = null;
            TrainPathFirst.NextThread = null;
            TrainPathSecond.BackThread = null;

            return ActionState.DONE;
        }
    }
}
