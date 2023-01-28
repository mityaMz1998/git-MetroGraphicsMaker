using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Converters;
using Messages;

namespace Core
{
    /// <summary>
    /// Класс, описывающий "цепочку" маршрутов.
    /// </summary>
    public class Chain : ICloneable
    {
        
        private Int32 numberOfRepairs = 0;

        private Int32 timeWithoutRepair = 0;

        private Int32 additionalTime = 0;

        /// <summary>
        /// Флаг, указывающий после ремонта на линии началась цепочка или нет.
        /// </summary>
        public Boolean IsAfterRepair = false;

        /// <summary>
        /// Коллекция звеньев цепочек.
        /// </summary>
        public List<Link> Links;

        /// <summary>
        /// Коллекция "сырых" маршрутов.
        /// </summary>
        public LinkedList<Route> rawRoutes; 

        /// <summary>
        /// Ссылка на ТО-1
        /// </summary>
        private static readonly RepairType TO1 = MovementSchedule.colRepairType.SingleOrDefault(r => r.name.Equals("ТО1"));

        public Boolean ContainRoute(Int32 routeNumber)
        {
            return rawRoutes.Any(r => r.number == routeNumber);
        }

        /// <summary>
        /// Остаток от деления 
        /// </summary>
        /// <returns></returns>
        private Int32 getDeltaTime()
        {
            var t = timeWithoutRepair % (TO1.period + TO1.duration);
            if (t > TO1.period)
            {
                t = 0;
            }
            Logger.Output(String.Format("deltaT = {0}", t), "Chain.getDeltaTime()");
            return t;
            //return timeWithoutRepair - numberOfRepairs*(TO1.period + TO1.Duration);
            //return timeWithoutRepair - numberOfRepairs*TO1.period - (numberOfRepairs - 1)*TO1.Duration;
        }

        /// <summary>
        /// Интервал времени, определяющий необходимость ввода последнего ремонта 
        /// </summary>
        /// <returns></returns>
        private Int32 getAdditionalTime()
        {
            Int32 t = 0;
            if (NumberOfRepairs > 1)
            {
                t = timeWithoutRepair - NumberOfRepairs * (TO1.period + TO1.duration) + TO1.duration;
            }
            else
               if (NumberOfRepairs == 1)
            {
                t = timeWithoutRepair - TO1.period;
            }
           return t;
        }

        public Int32 AdditionalTime {
            get
            {
                additionalTime = getAdditionalTime();
                return additionalTime;
            }
        }

        private Int32 GetNumberOfRepairs()
        {
            // return numberOfRepairs = timeWithoutRepair / (TO1.period + TO1.Duration);
            numberOfRepairs = GetTimeWithoutRepair() / (TO1.period + TO1.duration);
            if (timeWithoutRepair % (TO1.period + TO1.duration)> TO1.period)
            {
                ++numberOfRepairs;
            }
           return numberOfRepairs;
        }

        public Int32 NumberOfRepairs {
            get
            {
                numberOfRepairs = GetNumberOfRepairs();
                return numberOfRepairs;
            }
        }

        private AdvancedRepairCandidate tmpCandidate;

        /**
         * 1. Рассчитали "окна".
         * 2. Проверели попадаем ли в первое "окно" кандидатом.
         * 2.1. Если да, то пересчитываем остальные окна, с учётом кандидиата.
         * 2.2. Если нет -- выбираем следующего кандидиата.
         */

        public void RecalcTime(AdvancedRepairCandidate candidate)
        {
         // начиная с кандидата  
            var time = candidate != null
                ? candidate.BeginTime() - rawRoutes.First.Value.ElementsOfMovementSchedule.Last.Value.beginTime
                : 0;
            timeWithoutRepair -= time; 
        }

        /// <summary>
        /// Метод, вычисляющий и возвращающий суммарное время работы на линии по всем маршрутам цепочки.
        /// </summary>
        /// <returns>Суммарное время работы на линии по всем маршрутам цепочки.</returns>
        private Int32 GetTimeWithoutRepair()
        {
            if (!rawRoutes.Any())
                return 0;

            // @NOTE: Вычислили время отработанное на линии без осмотра/ремонта/ухода в депо.
            var tmpRoute = rawRoutes.First;
            var time =
                tmpRoute.Value.TimeWithoutRepair((tmpRoute.Value.Repairs.Count == 0 || !IsAfterRepair) &&
                                                 !tmpRoute.Value.Repairs.Any(r => r.isContinue));

            tmpRoute = tmpRoute.Next;
            var last = rawRoutes.Last;
            while (tmpRoute != last && tmpRoute != null)
            {
                time += tmpRoute.Value.TimeWithoutRepair(tmpRoute.Value.Repairs.Count == 0);
                tmpRoute = tmpRoute.Next;
            }

            if (last != rawRoutes.First && last != null)
                time += last.Value.TimeWithoutRepair();

            // @NOTE Попытались учесть кандидата
            if (tmpCandidate != null)
                time += TimeConverter.MovementTime.end - tmpCandidate.BeginTime() - TO1.duration;
            // time -= tmpCandidate.BeginTime() - tmpRoute.Value.ElementsOfMovementSchedule.Last.Value.beginTime;

            return timeWithoutRepair = time;
        }

        public Int32 TimeWithoutRepair
        {
            get
            {
                timeWithoutRepair = GetTimeWithoutRepair();
                return timeWithoutRepair;
            }
        }

        public void RemoveFirstRoute(AdvancedRepairCandidate candidate = null)
        {
            tmpCandidate = candidate;
            
            RecalcTime(candidate);
            numberOfRepairs = GetNumberOfRepairs();
            if (rawRoutes.Any())
                rawRoutes.RemoveFirst();
        }

        private Int32 GetRealInterval()
        {
            return (timeWithoutRepair - numberOfRepairs*TO1.duration)/(numberOfRepairs+1);
        }

        public void ClearTimes()
        {
            foreach (var tmpRoute in rawRoutes)
                tmpRoute.Times.Clear();
            Links.Clear();
            //timeWithoutRepair = getTimeWithoutRepair();
        }

        public void TimeCalculation(AdvancedRepairCandidate candidate = null)
        {
            var t = TimeWithoutRepair;
            var n = NumberOfRepairs;

            // Количество "мест" для осмотров
            var repairCounter = 0;
            // Флаг, указывающий попал ли осмотр на первую половину дня (true) или нет (false). Соответствует выбору первого или второго маршрута из звена.
           // var notFirstRoute = false;
            // Текущий элемент списка "сырых" маршрутов, для "помаршрутной" "переработки" этого списка.
            var currentRoute = rawRoutes.First;
#if LOGER_ON
            Logger.Output(
                String.Format("currentRoute = {0}", currentRoute == null ? "null" : "№" + currentRoute.Value.number.ToString("D3")), 
                String.Format("{0}.{1}", GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif
            while (currentRoute != null && repairCounter < numberOfRepairs)
            {
                // Флаг, указывающий был ли совершён nextStep по элементам списка внутри цикла (помимо самого "шагания" цикла).
                var isInnerNextStep = false;

                //if (/*MovementScheduleAlgorythm.RECURSIVE == MovementSchedule.SelectedAlgorythm && */!notFirstRoute)
                #region Условие истинно
                {

                    var tMin = 0;
                    var tDes = 0;
                    var tMax = 0;
                    var lastVariant = currentRoute.Value.Times.Last;
                    if (lastVariant != null)
                    {
                        tMin = lastVariant.Value.Tmin + TO1.period + TO1.duration;
                        tDes = lastVariant.Value.Tdes + TO1.period + TO1.duration;
                        tMax = lastVariant.Value.Tmax + TO1.period + TO1.duration;

                        #region Логирование информации об изменённом времени маршрута
    #if LOGER_ON
                        Logger.Output(
                            String.Format("№{0:D3}: .Times.Last --> {1} ", currentRoute.Value.number, lastVariant.Value), 
                            String.Format("{0}.{1}", GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name));
    #endif
                        #endregion
                    }
                    else
                    {
                        // "Стартовая" временнАя точка, от которой пойдёт отсчёт.
                        var currentBeginTime = (candidate == null || repairCounter > 0)
                            ? currentRoute.Value.ElementsOfMovementSchedule.Last.Value.beginTime
                            : candidate.BeginTime() + TO1.duration;

                        tMin = currentBeginTime + getDeltaTime();
                        tDes = currentBeginTime + GetRealInterval();
                        tMax = currentBeginTime + TO1.period;

                        #region Логирование информации о границах
    #if LOGER_ON
                        Logger.Output(String.Format("deltaT = {0} --> {1} --> {2}", getDeltaTime(), GetRealInterval(), TO1.period), String.Format("{0}.{1}", GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name));
    #endif
                        #endregion
                    }

                    #region Логирование информации о предварительно вычисленных границах
#if LOGER_ON
                    Logger.Output(
                        String.Format("before prepare: [{0} {1} {2}]", 
                            TimeConverter.SecondsToString(tMin), 
                            TimeConverter.SecondsToString(tDes), 
                            TimeConverter.SecondsToString(tMax)), 
                        String.Format("{0}.{1}", 
                            GetType().FullName, 
                            System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif
                    #endregion

                    var isEmptyInterval = false;
                    // Добавляем для текущего маршрута
                    try
                    {
                        currentRoute.Value.Times.AddLast(
                            new TimeVariant(
                                Math.Min(tMin, TimeConverter.MovementTime.end),
                                Math.Min(tDes, TimeConverter.MovementTime.end),
                                Math.Min(tMax, TimeConverter.MovementTime.end)
                                )
                            );
                        #region Логирование информации о внесённых изменениях
#if LOGER_ON
                        Logger.Output(
                            String.Format("tmpRoute.number = {0}: {1}",
                                currentRoute.Value.number,
                                currentRoute.Value.Times.Last.Value),
                            String.Format("{0}.{1}", 
                                GetType().FullName, 
                                System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif
                        #endregion
                        currentRoute = currentRoute.Next;
                        isInnerNextStep = true;
                    }
                    catch
                    {
#if LOGER_ON
//                        Logger.Output(ae.ToString(), String.Format("{0}.{1}", GetType().FullName,   System.Reflection.MethodBase.GetCurrentMethod().Name));
                        Logger.Output(String.Format("Маршрут №{0:D3} изъят из цепочки [{1}], так как не имеет потребности в осмотре.", currentRoute.Value.number, this), String.Format("{0}.{1}", GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif
                        isEmptyInterval = true;
                        currentRoute = currentRoute.Next;
                        //if (currentRoute != null && rawRoutes.Any() && currentRoute.Previous == rawRoutes.First && currentRoute.Previous.Value.Times.Count == 0)
                        //    rawRoutes.Remove(currentRoute.Previous);
#if LOGER_ON
                        Logger.Output(String.Format("Обновлённая цепочка: [{0}]", this), String.Format("{0}.{1}", GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif
                    }
                    try
                    {
                        currentRoute.Value.Times.AddLast(
                            new TimeVariant(
                                Math.Max(tMin - TimeConverter.MovementTime.duration, TimeConverter.MovementTime.begin),
                                Math.Max(tDes - TimeConverter.MovementTime.duration, TimeConverter.MovementTime.begin),
                                Math.Max(tMax - TimeConverter.MovementTime.duration, TimeConverter.MovementTime.begin)
                            )
                        );

#if LOGER_ON
                        Logger.Output(
                            String.Format("tmpRoute.number = {0}: {1}",
                                currentRoute.Value.number,
                                currentRoute.Value.Times.Last.Value),
                            String.Format("{0}.{1}",
                                GetType().FullName,
                                System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif
                        if (currentRoute.Previous != null && !isEmptyInterval)
                            currentRoute.Previous.Value.isGluedToNext = true;
                    }
                    catch (Exception exception)
                    {
#if LOGER_ON
                        Logger.Output(exception.ToString(), GetType().FullName + "." + MethodBase.GetCurrentMethod().Name);
#endif
                    }


                    if (!isInnerNextStep && currentRoute != null)
                        currentRoute = currentRoute.Next;
                    ++repairCounter;

                  //  break;
                }
                #endregion

                #region Осмотр вне временного интервала
                
                
                /* если попал на первый, то ничего не трогаем, а если на второй, то отсекаем первый; всё это проделываем прямо в этой процедуре, так как это редактирование цепочки */

                // необходимо узнать на какой попали и написать кусок, связанный с удалением маршрута

                // узнаём на какой попали, в этом условном операторе
                // необходимо делать это пошагово!!! в цикле могут быть проблемы при кодировании удаления и просмотров
                /*
                if (currentBeginTime + timespan >= TimeConverter.MovementTime.end)
                {
                    var currentTime = currentBeginTime - TimeConverter.timeDay;

                    if (currentBeginTime + minimalTimespan < TimeConverter.MovementTime.end)
                    {
                        tmpRoute.Value.Times.AddLast(new TimeVariant
                        {
                            Tmin = currentBeginTime + minimalTimespan,
                            Tdes = Math.Min(currentBeginTime + timespan, TimeConverter.MovementTime.end),
                            Tmax = Math.Min(currentBeginTime + TO1.period, TimeConverter.MovementTime.end)
                        });
                    }

                    tmpRoute.Value.isGluedToNext = true;
#if LOGER_ON
                    Logger.Output(String.Format("Маршрут №{0} \"приклеиваем\" к следующему.", tmpRoute.Value.number), GetType().FullName);
#endif
                    //tmpRoute = tmpRoute.Next;

                    // Теоретически это возможно ("Если мы будем предполагать невозможное, тогда Вы ошиблись адресом. Обратитесь к астрологам, а не ко мне." (с) Штрилиц), но всё равно что за дурдом?!
                    if (tmpRoute.Next != null)
                        tmpRoute.Next.Value.Times.AddLast(new TimeVariant
                        {
                            Tmin = Math.Max(currentTime + minimalTimespan, TimeConverter.MovementTime.begin),
                            Tdes = currentTime + timespan,
                            Tmax = currentTime + TO1.period
                        });
                }
                 */
                #endregion
                /*
                else
                {
#if LOGER_ON
                    Logger.Output(String.Format("Осмотр маршрута №{0}", tmpRoute.Value.number), GetType().FullName);
#endif
                    tmpRoute.Value.time = tmpRoute.Value.Times.AddLast(new TimeVariant
                    {
                        Tmin = currentBeginTime + minimalTimespan,
                        Tdes = currentBeginTime + timespan,
                        Tmax = Math.Min(TimeConverter.MovementTime.end, currentBeginTime + TO1.period)
                    }).Value;
                }

                //...
                // TODO: перенести в обработку цепочек
                // points.Add(route.With(r => r.Value.nightStayPoint).With(p => p.inspectionPoint));
                /*
                if (route != null && route.Value.nightStayPoint != null && route.Value.nightStayPoint.inspectionPoint != null)
                    points.Add(route.Value.nightStayPoint.inspectionPoint);
                */

               // tmpRoute = tmpRoute.Next;

               // ++repairCounter;
            }

            MakeLinks();
        }

        public override String ToString()
        {
            const String separator = "; ";
            //var buffer = new StringBuilder();
            //foreach (var route in rawRoutes)
            //    buffer.AppendFormat("{0:D3}{1}", route.number, separator);
            //buffer.Remove(buffer.Length - separator.Length, separator.Length);
            //return buffer.ToString();

            return String.Join(separator, rawRoutes.Select(r => r.number.ToString("D3")));
        }
        
        public Chain()
        {
            rawRoutes = new LinkedList<Route>();
            Links = new List<Link>();
        }

        public void Clear()
        {
            Links.Clear();
            rawRoutes.Clear();
        }

        public Int32 Time(Boolean flag)
        {
            return Links.Sum(link => link.TimeBetweenRepair(flag));
        }

        public Chain(IEnumerable<Route> routes)
        {
            if (routes == null)
                throw new ArgumentNullException("routes");

            if (rawRoutes == null)
            {
                rawRoutes = new LinkedList<Route>(routes);
            }
            else
            {
                var tmp = rawRoutes.Concat(routes);
                rawRoutes.Clear();
                rawRoutes = new LinkedList<Route>(tmp);
            }
        }

        public Chain TrimAndClone(AdvancedRepairCandidate mergeCandidate)
        {
            var newRawRoutes =
                new LinkedList<Route>(rawRoutes.SkipWhile(route => !route.Equals(mergeCandidate.Route())).Skip(1));
            var newChain = new Chain
            {
                tmpCandidate = mergeCandidate,
                // нет глубокого копирования (времена перетераются, поэтому пересчёт времён в случае ошибки)
                rawRoutes = newRawRoutes
                // Links and rawTimes collections are empty by default.
                // Имеет смысл не пересчитывать звенья, а исключать те, которые стали лишними. То есть 10-11, 11-12, 12-13, назначили на 11, значит 10-11 -- долой, а с 11-12 начинаем с candidate.BeginTime() это стоит учесть в методе типа makeLinks или же написать некий PrepareLinks
              //  Links = this.Links.SkipWhile(link => !Equals(link.SecondRoute, mergeCandidate.Route())).ToList()
            };
            
            // Возможно стоило бы добавлять информацию о назначении ремонта
            // newChain.getTimeWithoutRepair();
            if (newChain.GetNumberOfRepairs() > 0)
            {
                foreach (var rawRoute in newChain.rawRoutes)
                {
                    rawRoute.Times.Clear();
                    rawRoute.isGluedToNext = false;
                }
                newChain.PrepareTimes();
            }
            return newChain;
        }

        public void AddRepairCandidate(AdvancedRepairCandidate candidate)
        {
            tmpCandidate = candidate;
            var tmpLink = Links.First(link 
                => !link.IsCheked);
            tmpLink.Candidate = candidate;
            tmpLink.IsCheked = true;

            PrepareTimes();
        }
        /// <summary>
        /// 
        /// </summary>
        public void PrepareRoutes()
        {
            #region Логирование информации о входе в метод
#if LOGER_ON
            Logger.Output(String.Format("in the method"), String.Format("{0}.{1}", GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif
            #endregion

            #region Логирование информации о необработанных маршрутах данной цепочки.
#if LOGER_ON
            const String separator = ", ";
            var buffer = new StringBuilder("[");
            foreach (var route in rawRoutes)
                buffer.AppendFormat("{0:D3}{1}", route.number, separator);
            buffer.Remove(buffer.Length - separator.Length, separator.Length);
            buffer.Append("]" + Environment.NewLine);
            Logger.Output(buffer.ToString(), GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            #endregion

            #region Вычисление времени работы состава (суммарно по нескольким маршрутам) на линии без осмотра/ремонта/захода в депо.
            // GetTimeWithoutRepair();
            #endregion

            #region Логирование информации о длительности времени работы без осмотра/ремонта/захода в депо.
#if LOGER_ON
            Logger.Output(
                String.Format("[{2:D3} --> {3:D3}] Время без ремонта = {0} ({1})", TimeWithoutRepair, TimeConverter.SecondsToString(timeWithoutRepair), rawRoutes.First.Value.number, rawRoutes.Last.Value.number), 
                GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            #endregion
            
            // @NOTE: Подсчитали число ТО-1, которые необходимо провести в связи с наработкой часов
            if (GetNumberOfRepairs() == 0)
            {
                #region Логирование информации о выходе из метода
#if LOGER_ON
                Logger.Output("out from method (number of repairs = 0)", GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
                #endregion
                return;
            }

            #region Логирование информации о числе ремонтов для данной цепочки.
#if LOGER_ON
            Logger.Output(String.Format("Число ремонтов = {0}", NumberOfRepairs), GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            #endregion
           
            PrepareTimes();
            //TimeCalculation();

            #region Логирование информации о выходе из метода
#if LOGER_ON
            Logger.Output("out from method", GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            #endregion
        }

        /// <summary>
        /// Сырые времена
        /// </summary>
        private LinkedList<TimeVariant> rawTimes = new LinkedList<TimeVariant>();


        /// <summary>
        /// Подготовка времён осмотров. Расчёт сырых
        /// </summary>
        public void PrepareTimes()
        {
            // Криво!!!

            #region Логирование информации о входе в метод
#if LOGER_ON
            Logger.Output(String.Format("in the method with chain {0}", ToString()), String.Format("{0}.{1}", GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif
            #endregion

            var isFirstTime = true;

            // Нужно начинать с нужного (текущего) маршрута и не пересчитывать, а добавлять к уже известному. Возможно стоит написать отдельный метод.

            #region Расчёт "сырых" времён
            var currentRoute = rawRoutes.First;

            while (currentRoute != null) // && (MovementSchedule.calculationNotStarted || isFirstTime))
            {
                isFirstTime = false;

                var currentMinimalTime = 0;
                var currentDesiredTime = 0;
                var currentMaximalTime = 0;

                var lastTimeVariant = rawTimes.Last;
                if (lastTimeVariant == null)
                {
                    var currentBeginTime = tmpCandidate != null 
                        ? tmpCandidate.BeginTime() + TO1.duration // - TimeConverter.MovementTime.Duration
                        : currentRoute.Value.ElementsOfMovementSchedule.Last.Value.beginTime;

                    currentMinimalTime = currentBeginTime + getDeltaTime();
                    currentDesiredTime = currentBeginTime + GetRealInterval();
                    currentMaximalTime = currentBeginTime + TO1.period;
                }
                else
                {   
                    var additionalTime = TO1.duration + TO1.period;
                    currentMinimalTime = lastTimeVariant.Value.Tmin + additionalTime;
                    currentDesiredTime = lastTimeVariant.Value.Tdes + additionalTime;
                    currentMaximalTime = lastTimeVariant.Value.Tmax + additionalTime;
                }
                try
                {
                    var newTime = new TimeVariant(currentMinimalTime, currentDesiredTime, currentMaximalTime);
                    if (!rawTimes.Contains(newTime))
                        rawTimes.AddLast(newTime);
                }
                catch (Exception exception)
                {
#if LOGER_ON
                    Logger.Output(exception.ToString());
#endif
                }
                currentRoute = currentRoute.Next;
            }
            #endregion

            currentRoute = rawRoutes.First;
            var substractTime = tmpCandidate != null ? TimeConverter.MovementTime.duration : 0;

           // var substractTime = 0;

            var repairCounter = 0;
            var currentTimeVariant = rawTimes.First;
            while (currentTimeVariant != null && repairCounter < numberOfRepairs)
            {
                if (currentRoute == null)
                    break;

                try
                {
#if LOGER_ON
                    Logger.Output(currentRoute.Value.ToString(), GetType().FullName);
#endif
                    var newTime = new TimeVariant(
                        Math.Min(currentTimeVariant.Value.Tmin - substractTime, TimeConverter.MovementTime.end),
                        Math.Min(currentTimeVariant.Value.Tdes - substractTime, TimeConverter.MovementTime.end),
                        Math.Min(currentTimeVariant.Value.Tmax - substractTime, TimeConverter.MovementTime.end)
                        );

                    if (newTime.IsPoint && newTime.Tmin == TimeConverter.MovementTime.end)
                    {
                        currentRoute = currentRoute.Next;
                        substractTime += TimeConverter.MovementTime.duration;
                        continue;
                    }
                    
                    var lastTime = currentRoute.Value.Times.AddLast(newTime);
                    //
                    // Странное условие для той ситуации, при которой кусок отваливается на следующий маршрут
                    //
                    //if (lastTime.Value.Tmax == TimeConverter.MovementTime.end && currentTimeVariant.Value.Tmax - substractTime > TimeConverter.MovementTime.begin)
                    repairCounter++;
#if LOGER_ON
                    Logger.Output(String.Format("repairCounter = {0}", repairCounter), GetType().FullName);
#endif
                    if (lastTime.Value.IsInto(TimeConverter.MovementTime.end))
                    {
                        currentRoute.Value.isGluedToNext = true;

                        // CS1612
                        var tmpBox = currentRoute.Value.Times.Last.Value;
                        tmpBox.TypeOfInterval = TimeVariantType.LEFT;
                        
                        currentRoute = currentRoute.Next;
                        substractTime += TimeConverter.MovementTime.duration;
                        newTime = new TimeVariant(
                            Math.Max(currentTimeVariant.Value.Tmin - substractTime, TimeConverter.MovementTime.begin),
                            Math.Max(currentTimeVariant.Value.Tdes - substractTime, TimeConverter.MovementTime.begin),
                            Math.Max(currentTimeVariant.Value.Tmax - substractTime, TimeConverter.MovementTime.begin),
                            TimeVariantType.RIGHT
                            );
                        if (currentRoute != null && !newTime.IsPoint)
                            currentRoute.Value.Times.AddLast(newTime);
                        else break;
                    }
                    currentTimeVariant = currentTimeVariant.Next;
                }
                catch (Exception ae)
                {
#if LOGER_ON
                    Logger.Output(ae.ToString(), "ChainError");
#endif
                    if (currentRoute != null)
                        currentRoute = currentRoute.Next;
                    else break;
                    substractTime += TimeConverter.MovementTime.duration;
                }
            }

            #region Логирование информации о текущей цепочке
#if LOGER_ON
            var buffer = new StringBuilder(ToString() + Environment.NewLine);
            foreach (var tmpRoute in rawRoutes)
            {
                buffer.AppendFormat("[№{0:D3}].Times is {1}", tmpRoute.number, tmpRoute.Times.Count > 0 ? Environment.NewLine : "empty");
                foreach (var time in tmpRoute.Times)
                    buffer.AppendLine("\t" + time);
            }
            Logger.Output(buffer.ToString(), GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            #endregion

            MakeLinks();

            //rawTimes.Clear();

            #region Логирование информации о выходе из метода
#if LOGER_ON
            Logger.Output(String.Format("out from method"), String.Format("{0}.{1}", GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif
            #endregion
        }

       // не корректно работает
        /// <summary>
        /// Создание звеньев для сырой цепочки (приготовление цепочки).
        /// </summary>
        public void MakeLinks()
        {
            #region Логирование о входе в метод
#if LOGER_ON
            Logger.Output("in the method", GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            #endregion

            Links = rawRoutes
                .Where(route => route.Times.Any())
                .Select(route => new Link
                {
                    FirstRoute  = route,
                    SecondRoute = route.isGluedToNext ? route.nextRoute : null,
                    ParentChain = this
                }).ToList();

            #region Old code
            /*
            Links = rawRoutes.Where(route => route != null && route.Times.Count > 0).Select(route => new Link { FirstRoute = route, SecondRoute = route.isGluedToNext ? route.nextRoute : null }).ToList();
            */

            /*
            // Создали временный контейнер
            var range = new List<Link>();
            // Задали начальные условия
            var current = rawRoutes.First;
            // Пока текущий не последний и не пустой
            while (current != null)
            {
                // Если значение текущего не пусто и у него (значения) есть времена
                if (current.Value != null && current.Value.Times.Count > 0)
                    // Создали и добавили новое звено
                    range.Add(new Link
                    {
                        FirstRoute  = current.Value,
                        SecondRoute = current.Value.isGluedToNext ? current.Value.nextRoute : null,
                    });
                // Шагнули дальше
                current = current.Next;
            }
            
            #region Заполнение вектора звеньев элементами из временного контейнера.
            if (Links == null)
                Links = new List<Link>(range);
            else
            {
                Links.Clear();
                Links.AddRange(range);
            }
            #endregion
            */

            /*
            if (rawRoutes.Count > 1 && current != null && current.Previous != null && !current.Previous.Value.isGluedToNext || rawRoutes.Count == 1)
            {
                range.Add(new Link
                {
                    FirstRoute = rawRoutes.Last.Value,
                });
            }
             */

            /*
            if (rawRoutes.Count == 1)
                range.Add(new Link
                {
                    FirstRoute = rawRoutes.First.Value,
                });
            */
            #endregion

            #region Логирование информации о результатах работы метода и выходе из него.
#if LOGER_ON
            var separator = ", ";
            var buffer = new StringBuilder();
            foreach (var link in Links)
                buffer.AppendFormat("{0}{1}", link, separator);
            if (buffer.Length >= separator.Length)
                buffer.Remove(buffer.Length - separator.Length, separator.Length);
            Logger.Output(buffer.ToString(), GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            Logger.Output("out from method", GetType().FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            #endregion
        }

        public Object Clone()
        {
            // @TODO: Это заглушка, переписать!

            var copy = new Chain(rawRoutes)
            {
                numberOfRepairs = this.numberOfRepairs,
                timeWithoutRepair = this.timeWithoutRepair,
                Links = new List<Link>()
            };

            // здесь всё не так!!!! об этом нужно подумать!!!

            #region Поэлементное копирование звеньев
            foreach (var link in Links)
                copy.Links.Add(link.Clone() as Link);
            #endregion

            return copy;
        }
    }
}