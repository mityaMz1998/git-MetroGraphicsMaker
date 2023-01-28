using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;
using System.Windows;
using Core;
using View;
using TrainPath = Core.TrainPath;
using Exceptions.Actions;

namespace Actions
{
    class CreationTrainPathLastTail : BaseEdit
    {
        private ViewTail CreateViewTail;
        private Core.AbstractTail CreateLogicalTail = null; //Удаленный хвост, если ResultWasConnection = false; Нужно для простого отменены операции
        private Boolean isModify = false; //Использовалось ли изменение данного типа или нет.

        /// <summary>
        /// Добавлять объект в стек операций MasterTrainPath или нет. True - добавить
        /// </summary>
        public Boolean isAddStack = true;

        public CreationTrainPathLastTail(TrainPath First, AbstractTailGiver.NamesTails NameTail, string SecondaryName, ListTrainPaths _MasterTrainPaths, Boolean isAddStack = true)
            : base(First, _MasterTrainPaths)
        {
            CreateLogicalTail = AbstractTailGiver.CreateAbstractTail(NameTail, SecondaryName, First, First.MasElRasp[First.NumLast].departureTime);
            CreateViewTail = ViewTailGiver.CreateViewTail(CreateLogicalTail);
        }

        public override Boolean Check()
        {
            if (CTrainPath.ViewTrainPath.TailTrainPath != null) return false;
            return true;
        }

        public override ActionState Do()
        {
            CTrainPath.ViewTrainPath.TailTrainPath = CreateViewTail;
            CTrainPath.LogicalTail = CreateLogicalTail;

            MasterTrainPaths.TailsTrainPaths.Add(CreateViewTail);
            MasterTrainPaths.Children.Add(CreateViewTail);
            CTrainPath.ViewTrainPath.TailTrainPath.InvalidateVisual();

            if (!isModify && isAddStack) //Регистрация в журналах выполненных пользователем операций
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
            CTrainPath.ViewTrainPath.TailTrainPath = null;
            CTrainPath.LogicalTail = null;

            MasterTrainPaths.TailsTrainPaths.Remove(CreateViewTail);
            MasterTrainPaths.Children.Remove(CreateViewTail);
            //CTrainPath.ViewTrainPath.TailTrainPath.InvalidateVisual();

            return ActionState.CANCELLED;
        }
    }
}
