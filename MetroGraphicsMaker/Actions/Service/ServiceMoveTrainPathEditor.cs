


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

    //Должно содержать логику перемещения списка ниток, нужно для перемещения остальных ниток при операциях. (Движение ниток, с которыми пользователь не работает, но которые должны измениться в процессе совершения операции.
    class ServiceMoveTrainPathEditor : BaseEdit
    {
        private List<SimplifiedTrainPath> MemorySimplifiedTrainPath;
        private List<int> StartTimeBeginTail;
        private List<TrainPath> EditTrainPaths;
        private int DeltaTime;

        private Boolean isModify = false; //Использовалось ли изменение данного типа или нет.

        public ServiceMoveTrainPathEditor(List<TrainPath> Selected, ListTrainPaths _MasterTrainPaths)
            : base(Selected[0], _MasterTrainPaths)
        {
            MemorySimplifiedTrainPath = new List<SimplifiedTrainPath>(Selected.Count);
            StartTimeBeginTail = new List<int>();
            EditTrainPaths = new List<TrainPath>();
            foreach (TrainPath thTrainPath in Selected)
            {
                MemorySimplifiedTrainPath.Add(new SimplifiedTrainPath(thTrainPath.DepartureTimeFromFirstStation, thTrainPath.MasElRasp));
                if (thTrainPath.ViewTrainPath.TailTrainPath != null)
                {
                    StartTimeBeginTail.Add(thTrainPath.ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail);
                }
                EditTrainPaths.Add(thTrainPath);
            }
            MasterTrainPaths = _MasterTrainPaths;
        }

        public ServiceMoveTrainPathEditor(List<TrainPath> Selected)
            : base(Selected[0], null)
        {
            MemorySimplifiedTrainPath = new List<SimplifiedTrainPath>(Selected.Count);
            StartTimeBeginTail = new List<int>();
            EditTrainPaths = new List<TrainPath>();
            foreach (TrainPath thTrainPath in Selected)
            {
                MemorySimplifiedTrainPath.Add(new SimplifiedTrainPath(thTrainPath.DepartureTimeFromFirstStation, thTrainPath.MasElRasp));
                if (thTrainPath.ViewTrainPath.TailTrainPath != null)
                {
                    StartTimeBeginTail.Add(thTrainPath.ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail);
                }
                EditTrainPaths.Add(thTrainPath);
            }
            MasterTrainPaths = null;
        }

        public override Boolean Check(int _DeltaTime)
        {
            DeltaTime = _DeltaTime;
            return true;
        }

        public override ActionState Do()
        {
            int OffsetIntoTheStartMarginsTrainTails = 0;
            for (int i = 0; i < EditTrainPaths.Count; i++)
            {
                EditTrainPaths[i].DepartureTimeFromFirstStation = (MemorySimplifiedTrainPath[i].MemoryStartFirstStation + DeltaTime);
                for (int Index = 0; Index < MemorySimplifiedTrainPath[i].MemoryMasElRasp.Count; Index++)
                {
                    EditTrainPaths[i].MasElRasp[Index].arrivalTime = MemorySimplifiedTrainPath[i].MemoryMasElRasp[Index].arrivalTime + DeltaTime;
                    EditTrainPaths[i].MasElRasp[Index].departureTime = MemorySimplifiedTrainPath[i].MemoryMasElRasp[Index].departureTime + DeltaTime;
                }

                EditTrainPaths[i].ViewTrainPath.InvalidateVisual();
                if (EditTrainPaths[i].ViewTrainPath.TailTrainPath != null)
                {
                    EditTrainPaths[i].ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail = StartTimeBeginTail[i - OffsetIntoTheStartMarginsTrainTails] + DeltaTime;
                    EditTrainPaths[i].ViewTrainPath.TailTrainPath.InvalidateVisual();
                }
                else
                {
                    OffsetIntoTheStartMarginsTrainTails++;
                }
            }

            if (!isModify && MasterTrainPaths != null) //Регистрация в журналах выполненых пользователем операций
            {
                MasterTrainPaths.StackAllDoOperation.Push(this);
                MasterTrainPaths.StackAllUndoOperation.Clear();
            }
            isModify = true;

            return ActionState.DONE;
        }

        public override ActionState Undo()
        {
            int OffsetIntoTheStartMarginsTrainTails = 0;
            for (int i = 0; i < EditTrainPaths.Count; i++)
            {
                EditTrainPaths[i].DepartureTimeFromFirstStation = MemorySimplifiedTrainPath[i].MemoryStartFirstStation;
                for (int Index = 0; Index < MemorySimplifiedTrainPath[i].MemoryMasElRasp.Count; Index++)
                {
                    EditTrainPaths[i].MasElRasp[Index].arrivalTime = MemorySimplifiedTrainPath[i].MemoryMasElRasp[Index].arrivalTime;
                    EditTrainPaths[i].MasElRasp[Index].departureTime = MemorySimplifiedTrainPath[i].MemoryMasElRasp[Index].departureTime;
                }

                EditTrainPaths[i].ViewTrainPath.InvalidateVisual();
                if (EditTrainPaths[i].ViewTrainPath.TailTrainPath != null)
                {
                    EditTrainPaths[i].ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail = StartTimeBeginTail[i - OffsetIntoTheStartMarginsTrainTails];
                    EditTrainPaths[i].ViewTrainPath.TailTrainPath.InvalidateVisual();
                }
                else
                {
                    OffsetIntoTheStartMarginsTrainTails++;
                }
            }

            return ActionState.CANCELLED;
        }
    }

    public class SimplifiedTrainPath
    {
        public int MemoryStartFirstStation;
        public List<clsElementOfSchedule> MemoryMasElRasp;

        public SimplifiedTrainPath(int TimeStartFirstStation, List<clsElementOfSchedule> MasElRasp)
        {
            MemoryStartFirstStation = TimeStartFirstStation;

            MemoryMasElRasp = new List<clsElementOfSchedule>(MasElRasp.Count);
            foreach (clsElementOfSchedule thElRasp in MasElRasp) MemoryMasElRasp.Add(new clsElementOfSchedule(thElRasp.arrivalTime, 0, thElRasp.departureTime, null));
        }
    }
}
