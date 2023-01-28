using System;
using System.Collections.Generic;
using System.Text;
using Converters;
using System.Linq;

namespace Converters
{
    public class WorkSchedule
    {
        /// <summary>
        /// Расписание работы -- список интервалов рабочего времени.
        /// </summary>
        public LinkedList<TimeInterval> workTimes;

        /// <summary>
        /// Время начала работы.
        /// </summary>
        public Int32 Begin { get { return workTimes.First().begin; } }

        /// <summary>
        /// Время окончания работы.
        /// </summary>
        public Int32 End { get { return workTimes.Last().end; } }

        /// <summary>
        /// Длительность между начало и концом.
        /// </summary>
        public Int32 Duration { get { return End - Begin; } }

        public WorkSchedule(TimeInterval interval)
        {
            workTimes = new LinkedList<TimeInterval>();
            workTimes.AddFirst(interval);
        }

        public WorkSchedule(Int32 start, Int32 finish)
        {
            workTimes = new LinkedList<TimeInterval>();
            workTimes.AddFirst(new TimeInterval(start, finish));
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var timeInterval in workTimes)
                sb.AppendLine(timeInterval.ToString());
            return sb.ToString();
        }

        public WorkSchedule Clone()
        {
            var ws = new WorkSchedule(Begin, End);
            foreach (var timeInterval in workTimes)
                // @TODO: Переписать этот метод с учётом новой логики!!!
                ws.insertWorkInterval(timeInterval.Clone());
            return ws;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="breakTime"></param>
        /// <returns></returns>
        public Boolean InsertBreakInterval(TimeInterval breakTime)
        {
            return breakTime != null && InsertBreakInterval(breakTime.begin, breakTime.end);

            // TODO: Возможно стоит исправить или продумать анализ пересечения и поглощения (вхождения) интервалов один другим (в другой).
        }

        /// <summary>
        /// Метод, вставляющий перерыв в расписание работы.
        /// </summary>
        /// <param name="beginBreakTime">Время начала перерыва в работе.</param>
        /// <param name="endBreakTime">Время конца перерыва в работе.</param>
        /// <returns>Вставлен ли перерыв.</returns>
        public Boolean InsertBreakInterval(Int32 beginBreakTime, Int32 endBreakTime)
        {
            if (beginBreakTime == endBreakTime)
                return false;

            var start  = Math.Min(beginBreakTime, endBreakTime);
            var finish = Math.Max(beginBreakTime, endBreakTime);


            var current = workTimes.First;
            while (current != null)
            {
                if (current.Value.isInto(start, true))
                {
                    if (current.Value.isInto(finish, true))
                    {
                        workTimes.AddAfter(current, new TimeInterval(finish, current.Value.end));
                    }
                    current.Value.end = start;
                    return true;
                }
                current = current.Next;
            }

            /*
            foreach (var time in workTimes)
            {
                if (time.isInto(start, true))
                {
                    if (time.isInto(finish, true))
                    {
                        workTimes.Insert(workTimes.IndexOf(time) + 1, new TimeInterval { begin = finish, end = time.end});
                    }
                    time.end = start;
                    return true;
                }
            }
            */
            return false;
        }

        /// <summary>
        /// Метод вставляющий дополнительный интервал рабочего времени в расписание работы.
        /// </summary>
        /// <param name="workTime"></param>
        /// <returns>Вставлен ли дополнительный интервал рабочего времени.</returns>
        public Boolean insertWorkInterval(TimeInterval workTime)
        {
            return insertWorkInterval(workTime.begin, workTime.end);
        }


        public Boolean isIntoScheduleBounds(TimeInterval interval)
        {
            return Begin < interval.begin && interval.end < End;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="beginWorkTime"></param>
        /// <param name="endWorkTime"></param>
        /// <returns></returns>
        public Boolean insertWorkInterval(Int32 beginWorkTime, Int32 endWorkTime)
        {
            if (beginWorkTime == endWorkTime)
                return false;

            var start = Math.Min(beginWorkTime, endWorkTime);
            var finish = Math.Max(beginWorkTime, endWorkTime);

            // @TODO: Заплатка!!!
            if (start == Begin && finish == End)
                return false;
            
            /* @TODO: Стоит изменить цикл для пробежки с учётом начала интервала рабочего времени. */
            /*
            var current = workTimes.First;
            while (current != null)
            {
                if (current.Value.isInto(start, true))
                {
                    if (!current.Value.isInto(finish, true))
                        current.Value.end = finish;
                    return true;
                }
                if (current.Value.isInto(finish, true))
                {
                    // @TODO: Проверка на перекрытие с перерывом! А нужна ли она?
                    // Растянули интервал рабочего времени влево.
                    current.Value.begin = start;
                    return true;
                }

                current = current.Next;
            }

            workTimes.AddLast(new TimeInterval(start, finish));
            return true;
            */
            foreach (var time in workTimes)
            {
                if (time.isInto(start, true))
                {
                    if (time.isInto(finish, true))
                    {
                        // Поглотилось тем, что уже было.
                        return true;
                    }
                    else
                    {
                        // @TODO: Проверка на перекрытие с перерывом! А нужна ли она?
                        // Растянули интервал рабочего времени вправо.
                        time.end = finish;
                        return true;
                    }
                }
                else
                {
                    if (time.isInto(finish, true))
                    {
                        // @TODO: Проверка на перекрытие с перерывом! А нужна ли она?
                        // Растянули интервал рабочего времени влево.
                        time.begin = start;
                        return true;
                    }
                    else
                    { 
                        // Ни голова, ни хвост не лежат внутри интервала.
                        if (time.begin == start && time.end == finish)
                            return false;
                        continue;
                    }
                }
            }
            // @TODO: Как быть с упорядочиванием (сортировкой)? 
            workTimes.AddLast(new TimeInterval(start, finish));
            return true;
            
        }
    }
}
