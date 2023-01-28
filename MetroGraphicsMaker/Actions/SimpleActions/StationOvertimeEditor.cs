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
    class StationOvertimeEditor : BaseEdit
    {
        public List<clsElementOfSchedule> memoryMasElRasp = new List<clsElementOfSchedule>();

        private int IndexEditStation;
        private Boolean boolEditTimeArrive;
        private int DeltaTime;

        private int memoryTimeStartFirstStation;

        private ServiceMoveTrainPathEditor BackTrainPathMove = null;
        private int StartTimeBeginTail;
        private ServiceMoveTrainPathEditor NextTrainPathMove = null;

        private Boolean isModify = false; //Использовалось ли изменение данного типа или нет.

        public StationOvertimeEditor(TrainPath EditableTrainPath, int _IndexEditStation, Boolean _EditTimeArrive, ListTrainPaths _MasterTrainPaths)
            : base(EditableTrainPath, _MasterTrainPaths) //EditTimeArrive - какая станционная точка является опорной: прибытие или отправление. True - прибытие. False - отправление.
        {
            /* int SRVShift = 0;
             foreach (clsElementOfSchedule thElementOfSchedule in EditableTrainPath.MasElRasp)
             {
                 memoryMasElRasp.Add(new clsElementOfSchedule((int)thElementOfSchedule.arrivalTime - SRVShift, (int)thElementOfSchedule.TimeStoyanOtprav, (int)thElementOfSchedule.departureTime - (int)thElementOfSchedule.TimeStoyanOtprav - SRVShift, thElementOfSchedule.task));
                 SRVShift = SRVShift + (int)thElementOfSchedule.TimeStoyanOtprav;
             }*/

            memoryTimeStartFirstStation = EditableTrainPath.DepartureTimeFromFirstStation;

            foreach (clsElementOfSchedule thElementOfSchedule in EditableTrainPath.MasElRasp)
            {
                memoryMasElRasp.Add(new clsElementOfSchedule((int)thElementOfSchedule.arrivalTime, (int)thElementOfSchedule.TimeStoyanOtprav, (int)thElementOfSchedule.departureTime, thElementOfSchedule.task));
            }

            IndexEditStation = _IndexEditStation;
            boolEditTimeArrive = _EditTimeArrive;
            MasterTrainPaths = _MasterTrainPaths;

            if (EditableTrainPath.ViewTrainPath.TailTrainPath != null)
            {
                StartTimeBeginTail = EditableTrainPath.ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail;
            }

            List<TrainPath> EditableTrainPaths = EditableTrainPath.CreateListAll_clsThread(TrainPath.DirectionOfReading_clsThread.AllBack);
            if (EditableTrainPaths.Count > 0)
            {
                BackTrainPathMove = new ServiceMoveTrainPathEditor(EditableTrainPaths);
            }

            EditableTrainPaths.Clear();
            EditableTrainPaths = EditableTrainPath.CreateListAll_clsThread(TrainPath.DirectionOfReading_clsThread.AllNext);
            if (EditableTrainPaths.Count > 0)
            {
                NextTrainPathMove = new ServiceMoveTrainPathEditor(EditableTrainPaths);
            }
        }

        public override Boolean Check(int _DeltaTime)
        {
            DeltaTime = _DeltaTime;

            int DeltaTimePointsStation = (int)(memoryMasElRasp[IndexEditStation].departureTime - memoryMasElRasp[IndexEditStation].arrivalTime);

            if (boolEditTimeArrive)
            {
                if (memoryMasElRasp[IndexEditStation].arrivalTime + DeltaTime >= memoryMasElRasp[IndexEditStation].departureTime)
                {
                    if (BackTrainPathMove == null && NextTrainPathMove == null)
                    {
                        ;
                    }
                    else
                    {
                        if (BackTrainPathMove == null && NextTrainPathMove != null)
                        {
                            return NextTrainPathMove.Check(DeltaTime - DeltaTimePointsStation);
                        }
                        else
                        {
                            if (BackTrainPathMove != null && NextTrainPathMove == null)
                            {
                                return BackTrainPathMove.Check(DeltaTimePointsStation);
                            }
                            else
                            {
                                if (BackTrainPathMove != null && NextTrainPathMove != null)
                                {
                                    if (BackTrainPathMove.Check(DeltaTimePointsStation) && NextTrainPathMove.Check(DeltaTime - DeltaTimePointsStation))
                                    {
                                        return true;
                                    }
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (BackTrainPathMove != null) return BackTrainPathMove.Check(DeltaTime);
                }
            }
            else
            {
                if (memoryMasElRasp[IndexEditStation].departureTime + DeltaTime < memoryMasElRasp[IndexEditStation].arrivalTime)
                {
                    if (BackTrainPathMove == null && NextTrainPathMove == null)
                    {
                        ;
                    }
                    else
                    {
                        if (BackTrainPathMove == null && NextTrainPathMove != null)
                        {
                            return NextTrainPathMove.Check(-DeltaTimePointsStation);
                        }
                        else
                        {
                            if (BackTrainPathMove != null && NextTrainPathMove == null)
                            {
                                return BackTrainPathMove.Check(DeltaTime + DeltaTimePointsStation);
                            }
                            else
                            {
                                if (BackTrainPathMove != null && NextTrainPathMove != null)
                                {
                                    if (BackTrainPathMove.Check(DeltaTime + DeltaTimePointsStation) && NextTrainPathMove.Check(-DeltaTimePointsStation))
                                    {
                                        return true;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (NextTrainPathMove != null)
                    {
                        return NextTrainPathMove.Check(DeltaTime);
                    }
                }
            }

            return true;
        }

        public override ActionState Do()
        {
            int WorkEditStation = IndexEditStation;
            int SRVShift = 0;

            List<clsElementOfSchedule> FinishMasElRasp = new List<clsElementOfSchedule>();
            for (int i = 0; i < WorkEditStation; i++)
            {
                FinishMasElRasp.Add(new clsElementOfSchedule((int)memoryMasElRasp[i].arrivalTime, (int)memoryMasElRasp[i].TimeStoyanOtprav, (int)memoryMasElRasp[i].departureTime, memoryMasElRasp[i].task));
                SRVShift = SRVShift + (int)memoryMasElRasp[i].TimeStoyanOtprav;
            }
            clsElementOfSchedule EditSchedule = new clsElementOfSchedule((int)memoryMasElRasp[WorkEditStation].arrivalTime, (int)memoryMasElRasp[WorkEditStation].TimeStoyanOtprav, 0, memoryMasElRasp[WorkEditStation].task);

            int WorkDeltaTime = DeltaTime;
            //FinishMasElRasp[IndexEditStation].TimeStoyanOtprav = WorkDeltaTime;
            if (boolEditTimeArrive)
            {
                if (WorkDeltaTime >= memoryMasElRasp[IndexEditStation].TimeStoyanOtprav)
                {
                    CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation + EditSchedule.TimeStoyanOtprav;
                    foreach (clsElementOfSchedule thElementOfSchedule in FinishMasElRasp)
                    {
                        thElementOfSchedule.arrivalTime += EditSchedule.TimeStoyanOtprav;
                        thElementOfSchedule.departureTime += EditSchedule.TimeStoyanOtprav;
                    }

                    EditSchedule.arrivalTime += EditSchedule.TimeStoyanOtprav;
                    EditSchedule.TimeStoyanOtprav = WorkDeltaTime - EditSchedule.TimeStoyanOtprav;
                    EditSchedule.departureTime = (int)(memoryMasElRasp[WorkEditStation].departureTime + EditSchedule.TimeStoyanOtprav);
                }
                else
                {
                    CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation + WorkDeltaTime;
                    foreach (clsElementOfSchedule thElementOfSchedule in FinishMasElRasp)
                    {
                        thElementOfSchedule.arrivalTime += WorkDeltaTime;
                        thElementOfSchedule.departureTime += WorkDeltaTime;
                    }

                    EditSchedule.arrivalTime += WorkDeltaTime;
                    EditSchedule.TimeStoyanOtprav -= WorkDeltaTime;
                    EditSchedule.departureTime = (int)memoryMasElRasp[WorkEditStation].departureTime;
                }
            }
            else
            {
                if (-WorkDeltaTime >= memoryMasElRasp[IndexEditStation].TimeStoyanOtprav)
                {
                    CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation + (EditSchedule.TimeStoyanOtprav + WorkDeltaTime);

                    EditSchedule.departureTime = (int)memoryMasElRasp[WorkEditStation].departureTime - EditSchedule.TimeStoyanOtprav;
                    EditSchedule.TimeStoyanOtprav = Math.Abs(EditSchedule.TimeStoyanOtprav + WorkDeltaTime);

                    foreach (clsElementOfSchedule thElementOfSchedule in FinishMasElRasp)
                    {
                        thElementOfSchedule.arrivalTime -= EditSchedule.TimeStoyanOtprav;
                        thElementOfSchedule.departureTime -= EditSchedule.TimeStoyanOtprav;
                    }

                    EditSchedule.arrivalTime -= EditSchedule.TimeStoyanOtprav;
                }
                else
                {
                    //CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation + WorkDeltaTime;
                    EditSchedule.TimeStoyanOtprav = EditSchedule.TimeStoyanOtprav + WorkDeltaTime;
                    EditSchedule.departureTime = (int)memoryMasElRasp[WorkEditStation].departureTime + WorkDeltaTime;
                }
            }
            FinishMasElRasp.Add(EditSchedule);

            for (int i = WorkEditStation + 1; i < memoryMasElRasp.Count; i++)
            {
                FinishMasElRasp.Add(new clsElementOfSchedule((int)memoryMasElRasp[i].arrivalTime - (memoryMasElRasp[WorkEditStation].departureTime - EditSchedule.departureTime), (int)memoryMasElRasp[i].TimeStoyanOtprav, (int)memoryMasElRasp[i].departureTime + (int)memoryMasElRasp[i].TimeStoyanOtprav - (memoryMasElRasp[WorkEditStation].departureTime - EditSchedule.departureTime), memoryMasElRasp[i].task));
            }

            if (BackTrainPathMove != null)
            {
                BackTrainPathMove.Do();
            }
            if (CTrainPath.ViewTrainPath.TailTrainPath != null)
            {
                CTrainPath.ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail = FinishMasElRasp.Last().departureTime;
                CTrainPath.ViewTrainPath.TailTrainPath.InvalidateVisual();
                if (NextTrainPathMove != null) NextTrainPathMove.Do();
            }

            /*   int WorkEditStation = IndexEditStation;

               List<clsElementOfSchedule> FinishMasElRasp = new List<clsElementOfSchedule>();
               for (int i = 0; i < memoryMasElRasp.Count; i++)
               {
                   FinishMasElRasp.Add(new clsElementOfSchedule((int)memoryMasElRasp[i].arrivalTime, (int)memoryMasElRasp[i].TimeStoyanOtprav, (int)memoryMasElRasp[i].departureTime, memoryMasElRasp[i].task));
               }
               int WorkDeltaTime = DeltaTime;

               //FinishMasElRasp[IndexEditStation].TimeStoyanOtprav = WorkDeltaTime;
               if (boolEditTimeArrive)
               {
                   if (WorkDeltaTime >= memoryMasElRasp[IndexEditStation].TimeStoyanOtprav)
                   {
                       CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation + FinishMasElRasp[IndexEditStation].TimeStoyanOtprav;
                       FinishMasElRasp[IndexEditStation].TimeStoyanOtprav = WorkDeltaTime - FinishMasElRasp[IndexEditStation].TimeStoyanOtprav;
                    
                       if (BackTrainPathMove != null)
                       {
                           BackTrainPathMove.Do();
                       }
                       if (CTrainPath.ViewTrainPath.TailTrainPath != null)
                       {
                           CTrainPath.ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail = StartTimeBeginTail + WorkDeltaTime;
                           CTrainPath.ViewTrainPath.TailTrainPath.InvalidateVisual();
                           if (NextTrainPathMove != null) NextTrainPathMove.Do();
                       }
                   }
                   else
                   {
                       CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation + WorkDeltaTime;
                       FinishMasElRasp[IndexEditStation].TimeStoyanOtprav = FinishMasElRasp[IndexEditStation].TimeStoyanOtprav - WorkDeltaTime;
                    
                       if (BackTrainPathMove != null)
                       {
                           BackTrainPathMove.Do();
                       }
                   }
               }
               else
               {
                   if (-WorkDeltaTime >= memoryMasElRasp[IndexEditStation].TimeStoyanOtprav)
                   {
                       CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation + (FinishMasElRasp[IndexEditStation].TimeStoyanOtprav + WorkDeltaTime);
                       FinishMasElRasp[IndexEditStation].TimeStoyanOtprav = Math.Abs(FinishMasElRasp[IndexEditStation].TimeStoyanOtprav + WorkDeltaTime);
                    
                       if (CTrainPath.ViewTrainPath.TailTrainPath != null)
                       {
                           CTrainPath.ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail = StartTimeBeginTail + WorkDeltaTime;
                           CTrainPath.ViewTrainPath.TailTrainPath.InvalidateVisual();
                           if (NextTrainPathMove != null) NextTrainPathMove.Do();
                       }
                   }
                   else
                   {
                       //CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation + WorkDeltaTime;
                       FinishMasElRasp[IndexEditStation].TimeStoyanOtprav = FinishMasElRasp[IndexEditStation].TimeStoyanOtprav + WorkDeltaTime;
                    
                       if (BackTrainPathMove != null)
                       {
                           BackTrainPathMove.Do();
                       }
                   }
               }*/

            //Выполнение перемещения ниток и хвостов на время DeltaTime + Учет возможности перехода редактирования с одной точки на другую в пределах одной станции
            //int DeltaTimePointsStation = (int)(memoryMasElRasp[IndexEditStation].departureTime - memoryMasElRasp[IndexEditStation].arrivalTime);
            /*if (boolEditTimeArrive)
            {
                if (WorkDeltaTime >= memoryMasElRasp[IndexEditStation].TimeStoyanOtprav)
                {
                    // Учет возможности перехода редактирования с одной точки на другую в пределах одной станции  

                    CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation + memoryMasElRasp[IndexEditStation].TimeStoyanOtprav;
                    if (BackTrainPathMove != null)
                    {
                        BackTrainPathMove.Do();
                    }
                    WorkDeltaTime = WorkDeltaTime - memoryMasElRasp[IndexEditStation].TimeStoyanOtprav;
                    FinishMasElRasp[WorkEditStation].TimeStoyanOtprav = WorkDeltaTime;
                    if (CTrainPath.ViewTrainPath.TailTrainPath != null)
                    {
                        CTrainPath.ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail = StartTimeBeginTail + WorkDeltaTime;
                        CTrainPath.ViewTrainPath.TailTrainPath.InvalidateVisual();
                        if (NextTrainPathMove != null) NextTrainPathMove.Do();
                    }
                   // WorkDeltaTime = WorkDeltaTime - DeltaTimePointsStation;
                }
                else
                {
                    CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation + WorkDeltaTime;
                    if (BackTrainPathMove != null)
                    {
                        BackTrainPathMove.Do();
                    }
                    WorkDeltaTime = -WorkDeltaTime;
                    FinishMasElRasp[WorkEditStation].TimeStoyanOtprav = FinishMasElRasp[WorkEditStation].TimeStoyanOtprav + WorkDeltaTime;
                }
            }
            else
            {
                  if (WorkDeltaTime <= -DeltaTimePointsStation)
                {
                    // Учет возможности перехода редактирования с одной точки на другую в пределах одной станции
                    if (CTrainPath.ViewTrainPath.TailTrainPath != null)
                    {
                        CTrainPath.ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail = StartTimeBeginTail - DeltaTimePointsStation;
                        CTrainPath.ViewTrainPath.TailTrainPath.InvalidateVisual();
                        NextTrainPathMove.Do();
                    }
                    WorkDeltaTime = WorkDeltaTime + DeltaTimePointsStation;
                    CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation + WorkDeltaTime;
                    if (BackTrainPathMove != null)
                    {
                        BackTrainPathMove.Do();
                    }
                    WorkDeltaTime = -(WorkDeltaTime + DeltaTimePointsStation);
                }
                else
                {
                    if (CTrainPath.ViewTrainPath.TailTrainPath != null)
                    {
                        CTrainPath.ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail = StartTimeBeginTail + WorkDeltaTime;
                        CTrainPath.ViewTrainPath.TailTrainPath.InvalidateVisual();
                        NextTrainPathMove.Do();
                    }
                }

            }
            FinishMasElRasp[WorkEditStation].departureTime = FinishMasElRasp[WorkEditStation].departureTime + WorkDeltaTime;
            ++WorkEditStation;

            //Выполнение перемещения точек на время DeltaTime
            for (; WorkEditStation < memoryMasElRasp.Count; WorkEditStation++)
            {
                FinishMasElRasp.Add(new clsElementOfSchedule((int)memoryMasElRasp[WorkEditStation].arrivalTime + WorkDeltaTime, 0, memoryMasElRasp[WorkEditStation].departureTime + WorkDeltaTime, memoryMasElRasp[WorkEditStation].task));
            }

            foreach (clsElementOfSchedule thElementOfSchedule in FinishMasElRasp)
            {
                thElementOfSchedule.arrivalTime += CTrainPath.DepartureTimeFromFirstStation - memoryTimeStartFirstStation;
                thElementOfSchedule.departureTime += CTrainPath.DepartureTimeFromFirstStation - memoryTimeStartFirstStation;
            }*/
            //CTrainPath.TimeArrive = FinishTimeArrive;
            //CTrainPath.TimeDepart = FinishTimeDepart;

            CTrainPath.MasElRasp = FinishMasElRasp;

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

            CTrainPath.ViewTrainPath.InvalidateVisual();

            return ActionState.DONE;
        }

        public override ActionState Undo()
        {
            //CTrainPath.TimeArrive = memoryTimeArrive;
            //CTrainPath.TimeDepart = memoryTimeDepart;
            CTrainPath.MasElRasp = memoryMasElRasp;
            CTrainPath.ViewTrainPath.InvalidateVisual();

            if (BackTrainPathMove != null)
            {
                BackTrainPathMove.Undo();
            }
            CTrainPath.DepartureTimeFromFirstStation = memoryTimeStartFirstStation;
            if (CTrainPath.ViewTrainPath.TailTrainPath != null)
            {
                CTrainPath.ViewTrainPath.TailTrainPath.LogicalTail.TimeBeginTail = StartTimeBeginTail;
                CTrainPath.ViewTrainPath.TailTrainPath.InvalidateVisual();
            }
            if (NextTrainPathMove != null)
            {
                NextTrainPathMove.Undo();
            }

            if (MasterTrainPaths != null)
            {
                CTrainPath.direction.RemakeСonsistencyTrainPathsByTime();
            }

            return ActionState.DONE;
        }
    }
}
