using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using View;

using Core;
using Exceptions.Actions;

namespace Actions
{
    class LengthOfTrainPathWithDeleteConnectionTrainPath : BaseEdit
    {
        private byte StartNumberFirstStation;
        private byte StartNumberLastStation;
        private Boolean DirectionOfRemoval;
        private byte NumberStation;
        private BaseEdit DeleteTailTrainPaths = null;
        private List<BaseEdit> DeleteTrainPaths = null;

        private Boolean isModify = false; //Использовалось ли изменение данного типа или нет.

        public LengthOfTrainPathWithDeleteConnectionTrainPath(Core.TrainPath TrainPath, ListTrainPaths _MasterTrainPaths)
            : base(TrainPath, _MasterTrainPaths)
        {
            StartNumberFirstStation = TrainPath.NumFirst;
            StartNumberLastStation = TrainPath.NumLast;
        }

        public override Boolean Check(Boolean _DirectionOfRemoval, byte _NumberStation) //ButtonMouse - Выбор направления удаления точек: false удаляет левые относительно позиции NumberStation, true правые.
        {
            DirectionOfRemoval = _DirectionOfRemoval;
            NumberStation = _NumberStation;
            if (NumberStation >= 0)
            {
                if (_DirectionOfRemoval)
                {
                    if (CTrainPath.LogicalTail != null)
                    {
                        if (CTrainPath.LogicalTail.NameTail == AbstractTailGiver.NamesTails.LinkedTail || CTrainPath.LogicalTail.NameTail == AbstractTailGiver.NamesTails.LinkedTailCustom)
                        {
                            DeleteTailTrainPaths = new DisconnectionTrainPaths(CTrainPath, CTrainPath.NextThread, AbstractTailGiver.NamesTails.LinkedTail, MasterTrainPaths, false);
                        }
                        else
                        {
                            DeleteTailTrainPaths = new DeletionTrainPathLastTail(CTrainPath.LogicalTail.ViewAbstractTail);
                        }

                        DeleteTrainPaths = new List<BaseEdit>();
                        Core.TrainPath thTrainPaths = CTrainPath.NextThread;
                        bool CheckResult = DeleteTailTrainPaths.Check();
                        while (thTrainPaths != null)
                        {
                            DeleteTrainPaths.Add(new DeletionTrainPath(thTrainPaths, MasterTrainPaths));
                            CheckResult = CheckResult && DeleteTrainPaths.Last().Check();
                            thTrainPaths = thTrainPaths.NextThread;
                        }

                        return CheckResult;
                    }
                    else
                    {
                        return true;
                    }
                }
                else
                {
                    if (CTrainPath.BackThread != null)
                    {
                        DisconnectionTrainPaths NewCreateOrDeleteСonnectionTrainPathsEditor = new DisconnectionTrainPaths(CTrainPath, CTrainPath.BackThread, AbstractTailGiver.NamesTails.LinkedTail, MasterTrainPaths, false);
                        DeleteTailTrainPaths = NewCreateOrDeleteСonnectionTrainPathsEditor;

                        DeleteTrainPaths = new List<BaseEdit>();
                        Core.TrainPath thTrainPaths = CTrainPath.BackThread;
                        bool CheckResult = DeleteTailTrainPaths.Check();
                        while (thTrainPaths != null)
                        {
                            DeleteTrainPaths.Add(new DeletionTrainPath(thTrainPaths, MasterTrainPaths));
                            CheckResult = CheckResult && DeleteTrainPaths.Last().Check();
                            thTrainPaths = thTrainPaths.BackThread;
                        }

                        return CheckResult;
                    }
                    else
                    {
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        public override ActionState Do()
        {
            if (DirectionOfRemoval)
            {
                CTrainPath.NumLast = NumberStation;
            }
            else
            {
                CTrainPath.NumFirst = NumberStation;
            }

            if (DeleteTailTrainPaths != null)
            {
                DeleteTailTrainPaths.Do();
            }

            if (DeleteTrainPaths != null)
            {
                foreach (DeletionTrainPath thDeleteTrainPath in DeleteTrainPaths)
                {
                    thDeleteTrainPath.Do();
                }
            }

            if (!isModify) //Регистрация в журналах выполненых польхователем операций
            {
                MasterTrainPaths.StackAllDoOperation.Push(this);
                MasterTrainPaths.StackAllUndoOperation.Clear();
                isModify = true;
            }
            CTrainPath.ViewTrainPath.InvalidateVisual();

            return ActionState.DONE;
        }

        public override ActionState Undo()
        {
            CTrainPath.NumFirst = StartNumberFirstStation;
            CTrainPath.NumLast = StartNumberLastStation;
            if (DeleteTailTrainPaths != null)
            {
                DeleteTailTrainPaths.Undo();
            }
            if (DeleteTrainPaths != null)
            {
                foreach (DeletionTrainPath thDeleteTrainPath in DeleteTrainPaths)
                {
                    thDeleteTrainPath.Undo();
                }
            }
            CTrainPath.ViewTrainPath.InvalidateVisual();

            return ActionState.CANCELLED;
        }
    }
}
