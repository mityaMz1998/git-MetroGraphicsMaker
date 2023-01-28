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
    class DeletionTrainPath : BaseEdit
    {
        private DisconnectionTrainPaths DeleteTailEditorForNext = null;
        private DisconnectionTrainPaths DeleteTailEditorForBack = null;

        private Boolean isModify = false; //Использовалось ли изменение данного типа или нет.

        public DeletionTrainPath(TrainPath _DeleteTrainPath, ListTrainPaths _MasterTrainPaths)
            : base(_DeleteTrainPath, _MasterTrainPaths)
        {

        }

        public override Boolean Check()
        {
            if (CTrainPath.BackThread == null && CTrainPath.NextThread != null)
            {
                try
                {
                    DeleteTailEditorForNext = new DisconnectionTrainPaths(CTrainPath, CTrainPath.NextThread, AbstractTailGiver.NamesTails.LinkedTail, null, false);
                }
                catch (TheOperationIsNotFeasible)
                {
                    return false;
                }
            }
            else
            {
                if (CTrainPath.BackThread != null && CTrainPath.NextThread == null)
                {
                    try
                    {
                        DeleteTailEditorForBack = new DisconnectionTrainPaths(CTrainPath.BackThread, CTrainPath, AbstractTailGiver.NamesTails.LinkedTail, null, false);
                    }
                    catch (TheOperationIsNotFeasible)
                    {
                        return false;
                    }

                }
                else
                {
                    if (CTrainPath.BackThread != null && CTrainPath.NextThread != null)
                    {
                        try
                        {
                            DeleteTailEditorForNext = new DisconnectionTrainPaths(CTrainPath, CTrainPath.NextThread, AbstractTailGiver.NamesTails.LinkedTail, null, false);
                            DeleteTailEditorForBack = new DisconnectionTrainPaths(CTrainPath.BackThread, CTrainPath, AbstractTailGiver.NamesTails.LinkedTail, null, false);
                        }
                        catch (TheOperationIsNotFeasible)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public override ActionState Do()
        {
            MasterTrainPaths.TrainPaths.Remove(CTrainPath.ViewTrainPath);
            MasterTrainPaths.Children.Remove(CTrainPath.ViewTrainPath);
            CTrainPath.direction.ColThreads.Remove(CTrainPath);

            if (DeleteTailEditorForNext != null)
            {
                DeleteTailEditorForNext.Do();
            }
            if (DeleteTailEditorForBack != null)
            {
                DeleteTailEditorForBack.Do();
            }

            if (MasterTrainPaths != null)
            {
                CTrainPath.direction.RemakeСonsistencyTrainPathsByTime();

                if (!isModify) //Регистрация в журналах выполненых пользователем операций
                {
                    MasterTrainPaths.StackAllDoOperation.Push(this);
                    MasterTrainPaths.StackAllUndoOperation.Clear();
                }
            }
            isModify = true;
            return ActionState.DONE;
        }

        public override ActionState Undo()
        {
            MasterTrainPaths.TrainPaths.Add(CTrainPath.ViewTrainPath);
            MasterTrainPaths.Children.Add(CTrainPath.ViewTrainPath);
            CTrainPath.direction.ColThreads.AddFirst(CTrainPath);
            if (DeleteTailEditorForBack != null)
            {
                DeleteTailEditorForBack.Undo();
            }
            if (DeleteTailEditorForNext != null)
            {
                DeleteTailEditorForNext.Undo();
            }

            if (MasterTrainPaths != null)
            {
                CTrainPath.direction.RemakeСonsistencyTrainPathsByTime();
            }

            return ActionState.CANCELLED;
        }
    }
}
