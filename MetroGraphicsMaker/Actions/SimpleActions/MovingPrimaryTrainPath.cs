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
    class MovingPrimaryTrainPath : BaseEdit
    {
        private int MemoryTimeStartFirstStation;
        // private TrainPath EditPrimaryTrainPath;
        private int DeltaTime;
        private List<clsElementOfSchedule> memoryMasElRasp = new List<clsElementOfSchedule>();

        private AbstractTailEditor NextTailEditor = null;
        private AbstractTailEditor BackTailEditor = null;

        private Boolean isModify = false; //Использовалось ли изменение данного типа или нет.

        public MovingPrimaryTrainPath(TrainPath PrimaryTrainPath, ListTrainPaths _MasterTrainPaths)
            : base(PrimaryTrainPath, _MasterTrainPaths)
        {
            MemoryTimeStartFirstStation = PrimaryTrainPath.DepartureTimeFromFirstStation;
            foreach (clsElementOfSchedule thElementOfSchedule in PrimaryTrainPath.MasElRasp) memoryMasElRasp.Add(new clsElementOfSchedule((int)thElementOfSchedule.arrivalTime, (int)thElementOfSchedule.TimeStoyanOtprav, (int)thElementOfSchedule.departureTime, thElementOfSchedule.task));

            if (PrimaryTrainPath.BackThread != null)
            {
                BackTailEditor = new AbstractTailEditor(PrimaryTrainPath.BackThread.LogicalTail, PrimaryTrainPath);
            }
            if (PrimaryTrainPath.LogicalTail != null)
            {
                NextTailEditor = new AbstractTailEditor(PrimaryTrainPath.LogicalTail, PrimaryTrainPath);
            }

            MasterTrainPaths = _MasterTrainPaths;
        }

        public MovingPrimaryTrainPath(TrainPath PrimaryTrainPath)
            : base(PrimaryTrainPath, null)
        {
            MemoryTimeStartFirstStation = PrimaryTrainPath.DepartureTimeFromFirstStation;
            foreach (clsElementOfSchedule thElementOfSchedule in PrimaryTrainPath.MasElRasp) memoryMasElRasp.Add(new clsElementOfSchedule((int)thElementOfSchedule.arrivalTime, (int)thElementOfSchedule.TimeStoyanOtprav, (int)thElementOfSchedule.departureTime, thElementOfSchedule.task));

            if (PrimaryTrainPath.BackThread != null)
            {
                BackTailEditor = new AbstractTailEditor(PrimaryTrainPath.BackThread.LogicalTail, PrimaryTrainPath);
            }
            if (PrimaryTrainPath.LogicalTail != null)
            {
                NextTailEditor = new AbstractTailEditor(PrimaryTrainPath.LogicalTail, PrimaryTrainPath);
            }
        }

        /// <summary>
        /// Проверят можно ли производить данную операцию с таким DeltaTime.
        /// </summary>
        /// <param name="_DeltaTime">На сколько следует сместить нитку. (DeltaTime>0 => Смещение вправо. В противном случае смещение влево.)</param>
        /// <returns>True - операцию можно делать; False - операцию выполнять не рекомендуется.</returns>
        public override Boolean Check(int _DeltaTime)
        {
            DeltaTime = _DeltaTime;
            //Если есть обработчик хвоста от предыдущей нитки, то проверяет можно ли с хвостом производить такую операцию.
            if (BackTailEditor != null && !BackTailEditor.Check(DeltaTime))
            {
                //Если проверка обработчика хвоста вернула False, то значит операцию выполнять нельзя.
                return false;
            }
            //Если есть обработчик хвоста от текущей нитки, то проверяет можно ли с хвостом производить такую операцию.
            if (NextTailEditor != null && !NextTailEditor.Check(DeltaTime))
            {
                //Если проверка обработчика хвоста вернула False, то значит операцию выполнять нельзя.
                return false;
            }
            //P.S. Предыдущие два IF можно при желании объеденить в один общий через ИЛИ.
            return true;
        }

        public override ActionState Do()
        {
            CTrainPath.DepartureTimeFromFirstStation = MemoryTimeStartFirstStation + DeltaTime;
            CTrainPath.ViewTrainPath.InvalidateVisual();

            for (int Index = 0; Index < memoryMasElRasp.Count; Index++)
            {
                CTrainPath.MasElRasp[Index].arrivalTime = memoryMasElRasp[Index].arrivalTime + DeltaTime;
                CTrainPath.MasElRasp[Index].departureTime = memoryMasElRasp[Index].departureTime + DeltaTime;
            }

            //Проверяет есть ли обработчик хвоста от предыдущей нитки.
            if (BackTailEditor != null)
            {
                //Выполняет редактирование.
                BackTailEditor.Do();
            }
            //Проверяет есть ли обработчик хвоста для текущей нитки.
            if (NextTailEditor != null)
            {
                //Выполняет редактирование.
                NextTailEditor.Do();
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
            CTrainPath.DepartureTimeFromFirstStation = MemoryTimeStartFirstStation;
            CTrainPath.ViewTrainPath.InvalidateVisual();

            for (int Index = 0; Index < memoryMasElRasp.Count; Index++)
            {
                CTrainPath.MasElRasp[Index].arrivalTime = memoryMasElRasp[Index].arrivalTime;
                CTrainPath.MasElRasp[Index].departureTime = memoryMasElRasp[Index].departureTime;
            }

            if (BackTailEditor != null)
            {
                BackTailEditor.Undo();
            }
            if (NextTailEditor != null)
            {
                NextTailEditor.Undo();
            }

            if (MasterTrainPaths != null)
            {
                CTrainPath.direction.RemakeСonsistencyTrainPathsByTime();
            }
            return ActionState.CANCELLED;
        }
    }
}
