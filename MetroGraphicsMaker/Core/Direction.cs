/** 
 * @organization Departament "Management and Informatics in Technical Systems" (MITS@MIIT)
 * @author Konstantin Filipchenko (konstantin-649@mail.ru)
 * @version 0.1 
 * @date 2013-04-25
 * @description Source code based on clsNapravlenie.cls from GraphicPL@BASIC.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Messages;
using View;

namespace Core
{
    /// <summary>
    /// Набор значений направления движения.
    /// </summary>
    public enum DirectionValue
    {
        /// <summary>
        /// -1 - Чётное направление (DOWN)
        /// </summary>
        EVEN = -1,

        /// <summary>
        /// 0 - Значение по умолчанию
        /// </summary>
        NONE = 0,

        /// <summary>
        /// 1 - Нечётное навравление (UP)
        /// </summary>
        ODD = 1
    }

    /// <summary>
    /// Направление движения.
    /// </summary>
    public class Direction : Entity
    {
        /// <summary>
        /// Значение направления движения.
        /// </summary>
        public DirectionValue value = DirectionValue.NONE;

        /// <summary>
        /// Путь соответствующий данному направлению.
        /// </summary>
        public Int32 track;

        /// <summary>
        /// Противоположное направление по той же линии.
        /// </summary>
        public Direction ContrDirection;

        /// <summary>
        /// Направление продолжения по другой линии
        /// </summary>
        public Direction NextDirection;

        /// <summary>
        /// Линия
        /// </summary>
        public Line Line;

        /// <summary>
        /// Предшествующее направление по другой линии.
        /// </summary>
        public Direction PrevDirection;


        /*****************************************************************************************************************************
         * П О Д У М А Т Ь   Н А Д   И М Е Н О В А Н И Е М   И   С М Ы С Л О М   Н И Ж Е   С Л Е Д У Ю Щ И Х   П Е Р Е М Е Н Н Ы Х ! *
         *****************************************************************************************************************************/

        public TimetablesList Tables = new TimetablesList();

        /// <summary>
        /// Массив заданий стандартной нитки графика в заданном направлении
        /// </summary>
        private Task[] colStandartTreadTasks;

        /// <summary>
        /// Коллекция заданий стандартной нитки графика в заданном направлении.
        /// </summary>
        private List<Task> colTasks;

        /// <summary>
        /// Коллекция ниток данного направления
        /// </summary>
        public LinkedList<TrainPath> ColThreads = new LinkedList<TrainPath>();

        /*****************************************************************************************************************************
         * П О Д У М А Т Ь   Н А Д   И М Е Н О В А Н И Е М   И   С М Ы С Л О М   В Ы Ш Е   С Л Е Д У Ю Щ И Х   П Е Р Е М Е Н Н Ы Х ! *
         *****************************************************************************************************************************/


        /// <summary>
        /// Инициализация объекта, описывающего направление движения.
        /// </summary>
        /// <param name="aValue">Значение направления движения -- чётное/по умолчанию/нечётное направление.</param>
        /// <param name="aLine">Линия, по которой осуществляется движение.</param>
        /// <param name="aContrDirection">Направление движения по той же линии (aLine), противоположное данному.</param>
        public void Initialize(DirectionValue aValue, Line aLine, Direction aContrDirection)
        {
            if (aValue == DirectionValue.NONE)
                throw new ArgumentException("aValue has (illegal in this method) value DirectionValue.NONE");

            if (aLine == null)
                throw new ArgumentNullException("aLine");

            if (aContrDirection == null)
                throw new ArgumentNullException("aContrDirection");

            value = aValue;
            Line = aLine;
            ContrDirection = aContrDirection;

            switch (value)
            {
                case DirectionValue.EVEN:
                    Line.evenDirection = this;
                    track = (Line.flgTrack == 2) ? 1 : 2;
                    break;
                case DirectionValue.NONE:
                    track = 2;
                    break;
                case DirectionValue.ODD:
                    Line.oddDirection = this;
                    track = (Line.flgTrack == 1) ? 1 : 2;
                    break;
            }

            initializeTaskArray();
        }

        public static int CompareByDepartureTimeFirstStation(TrainPath FirstTrainPath, TrainPath SecondTrainPath)
        {
            return FirstTrainPath.DepartureTimeFromFirstStation - SecondTrainPath.DepartureTimeFromFirstStation;
        }

        public void ColThreadsOrderBy() //Со временем дожен начать получать делегата и работать через него.
        {
            for (int i = 0; i < ColThreads.Count; i++)
            {
                for (int j = 0; j < ColThreads.Count - i - 1; j++)
                {
                    if (CompareByDepartureTimeFirstStation(ColThreads.ElementAt(j), ColThreads.ElementAt(j + 1)) > 0)
                    {
                        Core.TrainPath memTrainPath = ColThreads.ElementAt(j);
                        ColThreads.Remove(memTrainPath);
                        ColThreads.AddAfter(ColThreads.Find(ColThreads.ElementAt(j)), memTrainPath);
                    }
                }
            }
        }

        public void RemakeСonsistencyTrainPathsByTime()
        {

            ColThreadsOrderBy();

            LinkedListNode<TrainPath> thNodeTrainPath = ColThreads.First;
            int NumberTrainPath;
            if (thNodeTrainPath.Value.direction.value == DirectionValue.EVEN)
            {
                NumberTrainPath = 1;
            }
            else
            {
                NumberTrainPath = 2;
            }
            while (thNodeTrainPath != null)
            {
                if (thNodeTrainPath.Previous != null)
                {
                    thNodeTrainPath.Value.LeftThread = thNodeTrainPath.Previous.Value;
                }
                else
                {
                    thNodeTrainPath.Value.LeftThread = null;
                }

                if (thNodeTrainPath.Next != null)
                {
                    thNodeTrainPath.Value.RightThread = thNodeTrainPath.Next.Value;
                }
                else
                {
                    thNodeTrainPath.Value.RightThread = null;
                }
                thNodeTrainPath.Value.code = NumberTrainPath;
                thNodeTrainPath.Value.trainNumber = thNodeTrainPath.Value.code.ToString();
                NumberTrainPath += 2;
                thNodeTrainPath = thNodeTrainPath.Next;
            }

            /*            ColThreads.First.Value.BackThread=null;

                        ColThreads.First.Value.BackThread=null;
                        ColThreads.First.Value.NextThread=ColThreads.First.Next.Value;
                        for (int Index = 1; Index < ColThreads.Count; Index++)
                        {
                            ColThreads.ElementAt(Index).BackThread=ColThreads.ElementAt(Index-1);
                            ColThreads.ElementAt(Index).NextThread=ColThreads.ElementAt(Index-1);
                        }
                        */
            /*  IndexForConsistency = 0;
              while (IndexForConsistency < TrainPaths.Count)
              {
                  if (TrainPaths[IndexForConsistency].LogicalTrainPath.direction.value == Core.DirectionValue.EVEN)
                  {
                      FirstEVEN = TrainPaths[IndexForConsistency];
                      TrainPaths[IndexForConsistency].LogicalTrainPath.LeftThread = null;
                      break;
                  }
                  IndexForConsistency++;
              }
              IndexForConsistency = 0;
              while (IndexForConsistency < TrainPaths.Count)
              {
                  if (TrainPaths[IndexForConsistency].LogicalTrainPath.direction.value == Core.DirectionValue.ODD)
                  {
                      FirstODD = TrainPaths[IndexForConsistency];
                      TrainPaths[IndexForConsistency].LogicalTrainPath.LeftThread = null;
                      break;
                  }
                  IndexForConsistency++;
              }

              IndexForConsistency = TrainPaths.Count;
              while (IndexForConsistency < TrainPaths.Count)
              {
                  if (TrainPaths[IndexForConsistency].LogicalTrainPath.direction.value == Core.DirectionValue.EVEN)
                  {
                      TrainPaths[IndexForConsistency].LogicalTrainPath.RightThread = null;
                      break;
                  }
                  IndexForConsistency--;
              }
              IndexForConsistency = TrainPaths.Count;
              while (IndexForConsistency < TrainPaths.Count)
              {
                  if (TrainPaths[IndexForConsistency].LogicalTrainPath.direction.value == Core.DirectionValue.ODD)
                  {
                      TrainPaths[IndexForConsistency].LogicalTrainPath.RightThread = null;
                      break;
                  }
                  IndexForConsistency--;
              }*/
            /*
                        Core.TrainPathLogic thTrain;
                        if (FirstEVEN != null)
                        {
                            thTrain = FirstEVEN.LogicalTrainPath;
                            thTrain.Name = 1;
                            thTrain = thTrain.RightThread;
                            while (thTrain != null)
                            {
                                thTrain.Name = thTrain.LeftThread.Name + 2;
                                thTrain = thTrain.RightThread;
                            }
                        }

                        if (FirstODD != null)
                        {
                            thTrain = FirstODD.LogicalTrainPath;
                            thTrain.Name = 2;
                            thTrain = thTrain.RightThread;
                            while (thTrain != null)
                            {
                                thTrain.Name = thTrain.LeftThread.Name + 2;
                                thTrain = thTrain.RightThread;
                            }
                        }*/
        }


        /// <summary>
        /// Получение станции по индексу задания в стандартной нитке 
        /// </summary>
        /// <param name="index">Индекс</param>
        /// <returns>Станция или null.</returns>
        public Station toGetStation(Int32 index)
        {
            Station result = null;
            try
            {
                result = colStandartTreadTasks.ElementAt(index).departureStation;
            }
            catch (IndexOutOfRangeException e)
            {

            }
            catch (NullReferenceException e)
            {

            }
            return result;
        }

        /// <summary>
        /// Получение станции отправления по индексу задания.
        /// </summary>
        /// <param name="index">Индекс задания, для которого пытаемся получить стацию отправления</param>
        /// <returns>Станция отправления или null.</returns>
        public Station getStation(Int32 index)
        {
            Station result = null;
            try
            {
                result = colTasks.ElementAt(index).departureStation;
            }
            catch (IndexOutOfRangeException e)
            {

            }
            catch (NullReferenceException e)
            {

            }
            return result;
        }

        /// <summary>
        /// Получение задания по индексу.
        /// </summary>
        /// <param name="index">Индекс задания.</param>
        /// <returns>Задание расположенное по индексу или null.</returns>
        public Task getTask(Int32 index)
        {
            Task result = null;
            try
            {
                result = colTasks.ElementAt(index);
            }
            catch (IndexOutOfRangeException e)
            {

            }
            catch (NullReferenceException e)
            {

            }
            return result;
        }

        /// <summary>
        /// Инициализация массивов заданий и проч.
        /// </summary>
        public void initializeTaskArray()
        {
            byte i, j, k = new byte();
            Station station = null;
            Task task = null;
            Boolean flg = new Boolean();
            int T, T1 = new int();
            //string s, s1;

            // откуда взялось число 40?
            // нужен список. хотя необходимо отследить, что оно именно этой длины
            colStandartTreadTasks = new Task[40];//colStandartTreadTasks[1;40]

            if (Line.colStations.Count == 1)
            {
                // Создаем "нитку" для линии из одной станции
                colStandartTreadTasks[0] = Line.colStations[0].colTasks[0];
                switch (value)
                {
                    case DirectionValue.EVEN:
                        colStandartTreadTasks[0].departureStation.TaskNumberFirstTrack = 1;
                        break;
                    case DirectionValue.NONE:
                        Error.showErrorMessage(colStandartTreadTasks[0], this, "DirectionValue.NONE");
                        break;
                    case DirectionValue.ODD:
                        colStandartTreadTasks[0].departureStation.TaskNumberSecondTrack = 1;
                        break;
                }
                Line.MaxEl = 1;
            }
            else
            {
                // Создаем нитку для нормальной линии
                switch (value)
                {
                    case DirectionValue.EVEN:
                        station = Line.EndStationOdd;
                        break;
                    case DirectionValue.NONE:
                        Error.showErrorMessage(new Station(), this, "DirectionValue.NONE");
                        break;
                    case DirectionValue.ODD:
                        station = Line.EndStationEven;
                        break;
                }

                if (station == null)
                    return;

                task = station.colTasks.FirstOrDefault(t => t.Direction.value == value);
                if (task == null)
                    return;

                ////Считываем времена хода по заданиям для непараллельного графика
                //flg = (task.ColTh != null);
                //if (flg)
                //{
                //    flg = (task.ColTh.Count > 0);

                //    if (flg)
                //    {
                //        MasTh = new int [task.ColTh.Count + 1];//MasTh[1;task.ColTh.Count]
                //        for (i = 1; i <= task.ColTh.Count; i++)
                //            MasTh[i] = 0;
                //    }
                //}
                //Создаем стандартную нитку
                i = 0;
                //while (task.Napr != (-1 * Napravlenie))
                while (task.Direction.value == value)
                {
                    colStandartTreadTasks[i] = task;

                    if (value == DirectionValue.ODD)
                        colStandartTreadTasks[i].departureStation.TaskNumberFirstTrack = i;
                    else
                        colStandartTreadTasks[i].departureStation.TaskNumberSecondTrack = i;

                    //if (task.taskDirection != DirectionValue.NONE)
                    //{
                    //    //Определение времен хода по направлению для непараллельного графика
                    //    if (flg)
                    //        for (j = 1; j <= task.ColTh.Count; j++)
                    //            MasTh[j] += (task.ColTh[j - 1] ?? 0) + ((i == 1) ? 0 : (task.ColTs[j - 1] ?? 0));
                    //}//Для кольцевой линии время хода определяется вместе со стоянками на всех станциях
                    //else
                    //    if ((task.taskDirection == DirectionValue.NONE) && (MovementSchedule.flgTypeLine == 2))//flgTypeLine = 2
                    //    {
                    //        //Определение времен хода по направлению для непараллельного графика
                    //        if (flg)
                    //            for (j = 1; j <= colStandartTreadTasks[1].ColTh.Count; j++)
                    //                MasTh[j] += colStandartTreadTasks[i].StatPeak;
                    //    }
                    if (Line.Equals(MovementSchedule.CurrentLine) && (MovementSchedule.flgTypeLine == 2))
                    {
                        MovementSchedule.TpoPik += colStandartTreadTasks[i].StatPeak + colStandartTreadTasks[i].ThPeak;
                        MovementSchedule.TpoNPik += colStandartTreadTasks[i].StatNonpeak + colStandartTreadTasks[i].ThNonpeak;
                    }
                    task = task.nextTask;
                    i++;
                }//end while



                Line.MaxEl = i;
                //Считываем времена хода по направлению для непараллельного графика
                //if (flg)
                //{
                //    ColTh = new List<int>();
                //    for (j=1;j<=this.colStandartTreadTasks[1].ColTh.Count;j++)
                //    {
                //        ColTh.Add(this.MasTh[j] ?? 0);
                //        ColTh.Add(this.colTypeRasp[j - 1]);                        
                //    }

                //}
                //If flg Then
                //    Set ColTh = New Collection
                //    For j = 1 To colStandartTreadTasks(1).ColTh.count
                //        ColTh.Add MasTh(j), colTypeRasp(j)
                //    Next
                //End If
                //Упорядочим коллекцию переходных времен хода по линии по возрастанию
                //    if (flg)
                //        for (j = 1; j < colStandartTreadTasks[1].ColTh.Count; j++)
                //            for (i = Convert.ToByte(j + 1); i <= Convert.ToByte(Convert.ToInt32(colStandartTreadTasks[1].ColTh)); i++)
                //                if (ColTh[j] > ColTh[i])
                //                {
                //                    T  = ColTh[j - 1];
                //                    T1 = ColTh[i - 1];
                //                    ColTh.RemoveAt(j - 1);
                //                    ColTh.RemoveAt(i - 2);
                //            /*            
                //            If j > ColTh.count Then
                //              ColTh.Add T1
                //            Else 
                //              ColTh.Add T1, colTypeRasp(i), j
                //            End If

                //            If i > ColTh.count Then
                //              ColTh.Add T
                //            Else
                //              ColTh.Add T, colTypeRasp(j), i
                //            End If

                //            s = colTypeRasp(j)
                //            s1 = colTypeRasp(i)
                //            colTypeRasp.Remove j
                //            If j > colTypeRasp.count Then
                //              colTypeRasp.Add s1
                //            Else: colTypeRasp.Add s1, , j
                //            End If

                //            colTypeRasp.Remove i
                //            If i > colTypeRasp.count Then
                //              colTypeRasp.Add s
                //            Else: colTypeRasp.Add s, , i
                //            End If*/
                //                    for (k = 1; k < line.MaxEl; k++)
                //                    {
                //                        T  = Convert.ToInt32(colStandartTreadTasks[k].ColTh[j - 1] ?? 0);
                //                        T1 = Convert.ToInt32(colStandartTreadTasks[k].ColTh[i - 1] ?? 0);

                //                        colStandartTreadTasks[k].ColTh.RemoveAt(j - 1);
                //                        if (j > (colStandartTreadTasks[k].ColTh.Count))
                //                            colStandartTreadTasks[k].ColTh.Add(T1);
                //                        else
                //                            colStandartTreadTasks[k].ColTh.Insert(j, T1);

                //                        colStandartTreadTasks[k].ColTh.RemoveAt(i - 1);
                //                        if (i > colStandartTreadTasks[k].ColTh.Count)
                //                            colStandartTreadTasks[k].ColTh.Add(T);
                //                        else
                //                            colStandartTreadTasks[k].ColTh.Insert(i, T);

                //                        T  = Convert.ToInt32(colStandartTreadTasks[k].ColTs[j - 1] ?? 0);
                //                        T1 = Convert.ToInt32(colStandartTreadTasks[k].ColTs[i - 1] ?? 0);

                //                        colStandartTreadTasks[k].ColTs.RemoveAt(j - 1);
                //                        if (j > colStandartTreadTasks[k].ColTs.Count)
                //                            colStandartTreadTasks[k].ColTs.Add(T1);
                //                        else
                //                            colStandartTreadTasks[k].ColTs.Insert(j, T1);

                //                        colStandartTreadTasks[k].ColTs.RemoveAt(i - 1);
                //                        if (i > colStandartTreadTasks[k].ColTs.Count)
                //                            colStandartTreadTasks[k].ColTs.Add(T);
                //                        else
                //                            colStandartTreadTasks[k].ColTs.Insert(i, T);
                //                    }
                //                }
            }//if (Line.clStation.Count==1) ELSE
            ////маскимальный элемент - i-й, при учете что элементы используем с 1-ого, а есть еще 
            ////нулевой, поэтому размер нового массива Line.MaxEl + 1
            //Array.Resize(ref colStandartTreadTasks, (line.MaxEl + 1));
        }

        public enum FlagPribOtpr
        {
            Arrival = -1,

            ALL = 0,

            Departure = 1
        }

        public enum FlgRez
        {
            OnlyRezerv = -1,

            ALL = 0,

            ExcludingRezerv = 1
        }

        //public List<clsElementOfSchedule> MasElRasp;
        /// <summary>
        /// Поиск ниток, проходящих заданную станцию в заданный временной интервал
        /// </summary>
        /// <param name="startTime">Время начала</param>
        /// <param name="endTime">Время конца</param>
        /// <param name="beforeLeftmostThread"> Нитка, ближайшая к заданному временному интервалу слева </param>
        /// <param name="leftmostThread">Нитка, крайняя ближайшая к заданному временному интервалу слева </param>
        /// <param name="rightmostThread">Нитка, крайняя ближайшая к заданному временному интервалу справа</param>
        /// <param name="afterRightmostThread">Нитка, ближайшая к заданному временному интервалу справа</param>
        /// <returns></returns>
        public Boolean toGetTrainPathesIntoTimeInterval(Int32 startTime, Int32 endTime, Int32 station, Enum flgPribOtpr, Enum flgRez, ref TrainPath beforeLeftmostThread, ref TrainPath leftmostThread, ref TrainPath rightmostThread, ref TrainPath afterRightmostThread)
        {
            // Если коллекция пустая
            if (ColThreads == null)
                return false;

            // Если все нитки лежат правее заданного интервала   
            var thread = ColThreads.First.Value;
            if (endTime < thread.GetElementOfSchedule(station).departureTime)
            {
                beforeLeftmostThread = null;
                leftmostThread = null;
                rightmostThread = null;
                afterRightmostThread = ColThreads.First.Value;
                return false;
            }

            thread = ColThreads.Last.Value;
            // Если все нитки лежат левее заданного интервала   
            if (startTime > thread.GetElementOfSchedule(station).departureTime)
            {
                beforeLeftmostThread = ColThreads.Last.Value;
                leftmostThread = null;
                rightmostThread = null;
                afterRightmostThread = null;
                return false;
            }

            // Хотя бы одна нитка внутри заданного интервала
            Int32 leftIndex = 0;
            Int32 rightIndex = ColThreads.Count - 1;
            Int32 middleIndex = 0;
            var leftThread = ColThreads.ElementAt(leftIndex);
            var rightThread = ColThreads.ElementAt(rightIndex);
            // Самая правая нитка внутри заданного интервала
            if ((startTime < rightThread.GetElementOfSchedule(station).departureTime) && (endTime > rightThread.GetElementOfSchedule(station).departureTime))
            {
                middleIndex = rightIndex;
                thread = ColThreads.ElementAt(middleIndex);
            }
            else
            {
                // Самая правая нитка внутри заданного интервала
                if ((startTime < leftThread.GetElementOfSchedule(station).departureTime) && (endTime > leftThread.GetElementOfSchedule(station).departureTime))
                {
                    middleIndex = leftIndex;
                    thread = ColThreads.ElementAt(middleIndex);
                }
                else
                {
                    middleIndex = (leftIndex + rightIndex) / 2;
                    thread = ColThreads.ElementAt(middleIndex);
                    while ((leftIndex + 1 < rightIndex) &&
                        ((startTime > thread.GetElementOfSchedule(station).departureTime) && (endTime < thread.GetElementOfSchedule(station).departureTime)))
                    {
                        if (endTime < thread.GetElementOfSchedule(station).departureTime)
                        {
                            rightIndex = middleIndex;
                        }
                        else
                        {
                            leftIndex = middleIndex;
                        }
                        middleIndex = (leftIndex + rightIndex) / 2;
                        thread = ColThreads.ElementAt(middleIndex);
                    }
                }
            }
            leftmostThread = thread;
            beforeLeftmostThread = thread;
            while (beforeLeftmostThread != null &&
                ((startTime < beforeLeftmostThread.GetElementOfSchedule(station).departureTime) && (endTime > beforeLeftmostThread.GetElementOfSchedule(station).departureTime)))
            {
                leftmostThread = beforeLeftmostThread;
                beforeLeftmostThread = beforeLeftmostThread.LeftThread;
            }
            rightmostThread = thread;
            afterRightmostThread = thread;
            while (afterRightmostThread != null &&
                ((startTime < afterRightmostThread.GetElementOfSchedule(station).departureTime) && (endTime > afterRightmostThread.GetElementOfSchedule(station).departureTime)))
            {
                rightmostThread = afterRightmostThread;
                afterRightmostThread = afterRightmostThread.RightThread;
            }
            return true;
        }
    }
}
