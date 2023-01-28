using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using View;

namespace Actions
{
    //Реализует логику выделения нитки, изменения ее цвета.
    //Вероятно нужно разбить на две операции: Выделения и девыделения.
    static class SelectTrainPath //Возможно стоит для них сделать прародителя.
    {
        public static void PrimarySelectOrDeSelectTrainPath(TrainPath FreeOrPrimaryTrainPath)
        {
            switch (FreeOrPrimaryTrainPath.Condition)
            {
                case ConditionTrainPath.Free:
                    FreeOrPrimaryTrainPath.Condition = ConditionTrainPath.PrimarySelected;
                    FreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.Add(FreeOrPrimaryTrainPath);
                    FreeOrPrimaryTrainPath.InvalidateVisual();
                    break;
                case ConditionTrainPath.PrimarySelected:
                    FreeOrPrimaryTrainPath.Condition = ConditionTrainPath.Free;
                    FreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.Remove(FreeOrPrimaryTrainPath);
                    FreeOrPrimaryTrainPath.InvalidateVisual();
                    break;
                default:
                    throw new System.ArgumentException("FreeOrPrimaryTrainPath.Condition должен быть равен PrimerySelected или Free", "original");
            }

            SecondarySelectOrDeSelectTrainPath(FreeOrPrimaryTrainPath);

            //SelectOrDeselectTailPrivaryTrainPath(FreeOrPrimaryTrainPath);
        }

        private static void SecondarySelectOrDeSelectTrainPath(TrainPath PrimaryTrainPath)
        {
            ConditionTrainPath MemoryCondition;
            switch (PrimaryTrainPath.Condition)
            {
                case ConditionTrainPath.Free:
                    MemoryCondition = ConditionTrainPath.Free;
                    break;
                case ConditionTrainPath.PrimarySelected:
                    MemoryCondition = ConditionTrainPath.SecondarySelected;
                    break;
                default:
                    throw new System.ArgumentException("PrimaryTrainPath.Condition должен быть равен PrimerySelected или Free", "original");
            }

            TrainPath thTrainPath = PrimaryTrainPath;
            while (thTrainPath.LogicalTrainPath.BackThread != null)
            {
                thTrainPath.LogicalTrainPath.BackThread.ViewTrainPath.Condition = MemoryCondition;
                if (MemoryCondition == ConditionTrainPath.SecondarySelected)
                {
                    thTrainPath.MasterTrainPaths.SecondarySelectedTrainPaths.Add(thTrainPath.LogicalTrainPath.BackThread.ViewTrainPath);
                }
                else
                {
                    thTrainPath.MasterTrainPaths.SecondarySelectedTrainPaths.Remove(thTrainPath.LogicalTrainPath.BackThread.ViewTrainPath);
                }
                thTrainPath.LogicalTrainPath.BackThread.ViewTrainPath.InvalidateVisual();

                if (thTrainPath.LogicalTrainPath.BackThread.ViewTrainPath.TailTrainPath != null)
                {
                    //SelectOrDeselectTailTrainPath(thTrainPath.LogicalTrainPath.backThread.ViewTrainPath);
                }
                thTrainPath = thTrainPath.LogicalTrainPath.BackThread.ViewTrainPath;
            }

            thTrainPath = PrimaryTrainPath;
            while (thTrainPath.LogicalTrainPath.NextThread != null)
            {
                thTrainPath.LogicalTrainPath.NextThread.ViewTrainPath.Condition = MemoryCondition;
                if (MemoryCondition == ConditionTrainPath.SecondarySelected)
                {
                    thTrainPath.MasterTrainPaths.SecondarySelectedTrainPaths.Add(thTrainPath.LogicalTrainPath.NextThread.ViewTrainPath);
                }
                else
                {
                    thTrainPath.MasterTrainPaths.SecondarySelectedTrainPaths.Remove(thTrainPath.LogicalTrainPath.NextThread.ViewTrainPath);
                }
                thTrainPath.LogicalTrainPath.NextThread.ViewTrainPath.InvalidateVisual();

                if (thTrainPath.LogicalTrainPath.NextThread.ViewTrainPath.TailTrainPath != null)
                {
                    //SelectOrDeselectTailTrainPath(thTrainPath.LogicalTrainPath.nextThread.ViewTrainPath);
                }
                thTrainPath = thTrainPath.LogicalTrainPath.NextThread.ViewTrainPath;
            }
        }

        //private static void SelectOrDeselectTailTrainPath(TrainPath thTrainPath)
        //{
        //        thTrainPath.TailTrainPath.Condition = thTrainPath.Condition;
        //        thTrainPath.TailTrainPath.InvalidateVisual();
        //}

        //private static void SelectOrDeselectTailPrivaryTrainPath(TrainPath PrimaryTrainPath)
        //{
        //    if (PrimaryTrainPath.TailTrainPath!=null)
        //    {
        //        PrimaryTrainPath.TailTrainPath.Condition = PrimaryTrainPath.Condition;
        //        PrimaryTrainPath.TailTrainPath.InvalidateVisual();
        //    }
        //    if (PrimaryTrainPath.LogicalTrainPath.backThread!=null)
        //    {
        //        PrimaryTrainPath.LogicalTrainPath.backThread.ViewTrainPath.TailTrainPath.Condition = PrimaryTrainPath.Condition;
        //        PrimaryTrainPath.LogicalTrainPath.backThread.ViewTrainPath.TailTrainPath.InvalidateVisual();
        //    }
        //}

        public static void PrimarySelectOrDeSelectTrainPaths(TrainPath SecondFreeOrPrimaryTrainPath)
        {
            if (SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.Count() == 0)
            {
                throw new System.ArgumentException("MasterTrainPaths.PrimarySelectedTrainPaths.Count() не может быть равен 0", "original");
            }
            if (SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths[0].LogicalTrainPath.direction != SecondFreeOrPrimaryTrainPath.LogicalTrainPath.direction)
            {
                throw new System.ArgumentException("Direction у ниток должен быть одинаков", "original");
            }

            int FirstTimeStartFirstStation;
            int SecondTimeStartFirstStation;
            List<TrainPath> SelectTrainPaths;
            switch (SecondFreeOrPrimaryTrainPath.Condition)
            {
                case ConditionTrainPath.Free:
                    if (SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.Count() > 1)
                    {
                        throw new System.ArgumentException("MasterTrainPaths.PrimarySelectedTrainPaths.Count() не может быть больше 1 при SecondFreeOrPrimaryTrainPath.Condition=Free", "original");
                    }

                    if (SecondFreeOrPrimaryTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation > SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.First().LogicalTrainPath.DepartureTimeFromFirstStation)
                    {
                        FirstTimeStartFirstStation = SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.First().LogicalTrainPath.DepartureTimeFromFirstStation;
                        SecondTimeStartFirstStation = SecondFreeOrPrimaryTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation;
                    }
                    else
                    {
                        FirstTimeStartFirstStation = SecondFreeOrPrimaryTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation;
                        SecondTimeStartFirstStation = SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.First().LogicalTrainPath.DepartureTimeFromFirstStation;
                    }
                    break;
                case ConditionTrainPath.PrimarySelected:
                    if (SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.Count() == 1)
                    {
                        throw new System.ArgumentException("MasterTrainPaths.PrimarySelectedTrainPaths.Count() не может быть равен 1 при SecondFreeOrPrimaryTrainPath.Condition=PrimarySelected", "original");
                    }

                    FirstTimeStartFirstStation=SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths[0].LogicalTrainPath.DepartureTimeFromFirstStation;
                    SecondTimeStartFirstStation = SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths[0].LogicalTrainPath.DepartureTimeFromFirstStation;
                    for (int Number = 1; Number < SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.Count; Number++)
                    {
                        if (FirstTimeStartFirstStation > SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths[Number].LogicalTrainPath.DepartureTimeFromFirstStation)
                        {
                            FirstTimeStartFirstStation = SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths[Number].LogicalTrainPath.DepartureTimeFromFirstStation;
                        }
                        if (SecondTimeStartFirstStation < SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths[Number].LogicalTrainPath.DepartureTimeFromFirstStation)
                        {
                            SecondTimeStartFirstStation = SecondFreeOrPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths[Number].LogicalTrainPath.DepartureTimeFromFirstStation;
                        }
                    }
                    if (SecondTimeStartFirstStation != SecondFreeOrPrimaryTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation && FirstTimeStartFirstStation != SecondFreeOrPrimaryTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation)
                    {
                        throw new System.ArgumentException("SecondFreeOrPrimaryTrainPath не является первой или последней Primary ниткой на графике по времени TimeStartFirstStation", "original");
                    }
                    break;
                default:
                    throw new System.ArgumentException("SecondFreeOrPrimaryTrainPath.Condition должен быть равен PrimerySelected или Free", "original");
            }
            SelectTrainPaths = SecondFreeOrPrimaryTrainPath.MasterTrainPaths.TrainPaths.FindAll(thTrainPath => FirstTimeStartFirstStation < thTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation && SecondTimeStartFirstStation > thTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation && SecondFreeOrPrimaryTrainPath.LogicalTrainPath.direction == thTrainPath.LogicalTrainPath.direction && SecondFreeOrPrimaryTrainPath.Condition == thTrainPath.Condition);

            PrimarySelectOrDeSelectTrainPath(SecondFreeOrPrimaryTrainPath);
            SelectTrainPaths.ForEach(thTrainPath => PrimarySelectOrDeSelectTrainPath(thTrainPath));
        }
    }
}
