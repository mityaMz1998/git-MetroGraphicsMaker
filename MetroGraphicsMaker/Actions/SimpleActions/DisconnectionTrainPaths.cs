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
    class DisconnectionTrainPaths : BaseEdit
    {
        private AbstractTailGiver.NamesTails NameTail;
        private TrainPath TrainPathFirst;
        private TrainPath TrainPathSecond;

        private AbstractTail TailDelete = null; //Удаленный хвост, если ResultWasConnection = false; Нужно для простого отменены операции
        private Boolean isModify = false; //Использовалось ли изменение данного типа или нет.

        /// <summary>
        /// Добавлять объект в стек операций MasterTrainPath или нет. True - добавить
        /// </summary>
        public Boolean isAddStack;

        public DisconnectionTrainPaths(TrainPath First, TrainPath Second, AbstractTailGiver.NamesTails _NameTail, ListTrainPaths _MasterTrainPaths, Boolean _isAddStack = true)
            : base(First, _MasterTrainPaths)
        {
            NameTail = _NameTail;
            isAddStack = _isAddStack;
            //Проверь последовательность ниток для удаления хвоста забблаговременно. Сделай чек и т.д.
            TrainPathFirst = First;
            TrainPathSecond = Second;
            if (TrainPathSecond.NextThread == TrainPathFirst)
            {
                TrainPathSecond = First;
                TrainPathFirst = Second;
            }
            TailDelete = TrainPathFirst.LogicalTail;

            if (!Check())
            {
                state = ActionState.CANT_EXECUTE;
                throw new TheOperationIsNotFeasible(ActionState.CANT_EXECUTE);
            }

            state = ActionState.DONE;
        }

        public override Boolean Check()
        {
            if (TrainPathFirst.NextThread == TrainPathSecond || TrainPathSecond.NextThread == TrainPathFirst)
            {
                return true;
            }
            return false;
        }

        public override ActionState Do()
        {
            TrainPathFirst.ViewTrainPath.MasterTrainPaths.TailsTrainPaths.Remove(TailDelete.ViewAbstractTail);
            TrainPathFirst.ViewTrainPath.MasterTrainPaths.Children.Remove(TailDelete.ViewAbstractTail);

            TrainPathFirst.LogicalTail = null;
            TrainPathFirst.ViewTrainPath.TailTrainPath = null;
            TrainPathFirst.NextThread = null;
            TrainPathSecond.BackThread = null;

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
            TrainPathFirst.LogicalTail = TailDelete;
            TrainPathFirst.NextThread = TrainPathSecond;
            TrainPathSecond.BackThread = TrainPathFirst;
            TrainPathFirst.ViewTrainPath.TailTrainPath = TailDelete.ViewAbstractTail;

            TrainPathFirst.ViewTrainPath.MasterTrainPaths.TailsTrainPaths.Add(TailDelete.ViewAbstractTail);
            TrainPathFirst.ViewTrainPath.MasterTrainPaths.Children.Add(TailDelete.ViewAbstractTail);

            return ActionState.CANCELLED;
        }
    }
}
