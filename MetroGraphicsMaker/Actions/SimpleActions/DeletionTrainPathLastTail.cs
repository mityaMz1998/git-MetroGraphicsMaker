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
    class DeletionTrainPathLastTail : BaseEdit
    {
        private ViewTail DeleteViewTail;

        /// <summary>
        /// Добавлять объект в стек операций MasterTrainPath или нет. True - добавить
        /// </summary>
        public Boolean isAddStack = false;

        private Boolean isModify = false; //Использовалось ли изменение данного типа или нет.

        public DeletionTrainPathLastTail(ViewTail _DeleteViewTail, Boolean isAddStack = false)
            : base(_DeleteViewTail.LogicalTail.LeftLogicalTrainPath, _DeleteViewTail.MasterTrainPaths)
        {
            DeleteViewTail = _DeleteViewTail;
        }

        public override Boolean Check()
        {
            return true;
        }

        public override ActionState Do()
        {
            CTrainPath.ViewTrainPath.TailTrainPath = null;
            CTrainPath.LogicalTail = null;

            MasterTrainPaths.TailsTrainPaths.Remove(DeleteViewTail);
            MasterTrainPaths.Children.Remove(DeleteViewTail);

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
            CTrainPath.ViewTrainPath.TailTrainPath = DeleteViewTail;
            CTrainPath.LogicalTail = DeleteViewTail.LogicalTail;

            MasterTrainPaths.TailsTrainPaths.Add(DeleteViewTail);
            MasterTrainPaths.Children.Add(DeleteViewTail);
            DeleteViewTail.InvalidateVisual();
            //CTrainPath.ViewTrainPath.TailTrainPath.InvalidateVisual();

            return ActionState.CANCELLED;
        }
    }
}
