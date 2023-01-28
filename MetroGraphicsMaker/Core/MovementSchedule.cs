using System.Collections.Generic;
using System.Diagnostics;
using Converters;
using System;
using System.Linq;
using System.Text;
using System.Windows;
using Messages;
using Schedule;
using Munkres;
using Forms;
using MetroGraphicsMaker;
using WpfApplication1.Forms;
using Excel = Microsoft.Office.Interop.Excel;


namespace Core
{
    /// <summary>
    /// Тип алгоритма решения задачи о назначениях
    /// </summary>
    [Flags]
    public enum MovementScheduleAlgorythm
    {
        /// <summary>
        /// Пустой (заглушка)
        /// </summary>
        NONE,
        /// <summary>
        /// Венгерский алгоритм (постоянный шаг)
        /// </summary>
        HUNGARIAN,
        /// <summary>
        /// Венгерский алгоритм (динамический шаг)
        /// </summary>
        DINAMIC_HUNGARIAN,
        /// <summary>
        /// Рекурсивный алгоритм
        /// </summary>
        RECURSIVE
    }

    /// <summary>
    /// Тип критерия
    /// </summary>
    [Flags]
    public enum CriteraTypes
    {
        /// <summary>
        /// Равномерность размещения
        /// </summary>
        EQUALITY,
        /// <summary>
        /// Минимум превышения
        /// </summary>
        MIN_ADDITIONAL_TIME,
}

/// <summary>
/// Способ упорядочивания цепочек
/// </summary>
public enum ChainsSortingModes
    {
        /// <summary>
        /// Пустой (заглушка)
        /// </summary>
        NONE,
        /// <summary>
        /// по уменьшению дополнительного времени 
        /// </summary>
        DESCENDING_ADDITIONAL_TIME,
        /// <summary>
        /// по возрастанию величины окна возможностей
        /// </summary>
        ASCENDING_POSSIBILITY_WINDOW,
     }

    /// <summary>
    /// Способ остановки работы алгоритма
    /// </summary>
    public enum AlgorithmTerminationModes
    {
        /// <summary>
        /// Расчет до конца
        /// </summary>
        NONE,
        /// <summary>
        /// до заданного числа успешных
        /// </summary>
        SPECIFIED_SUCCESS_NUMBER,
        /// <summary>
        /// до заданного числа начатых
        /// </summary>
        SPECIFIED_INITIATING_NUMBER,
    }

    public static class MovementSchedule
    {
        /// <summary>
        /// Коллекция станций для данного графика движения.
        /// </summary>
        public static List<Station> colStation;

        /// <summary>
        /// Коллекция депо для данного графика движения.
        /// </summary>
        public static List<Depot> colDepot;

        /// <summary>
        /// Коллекция линий для данного графика движения.
        /// </summary>
        public static List<Line> colLine;

        /// <summary>
        /// Коллекция заданий для данного графика движения.
        /// </summary>

        public static List<Task> colTask;
        /// <summary>
        /// Коллекция элементов расписания для данного графика движения.
        /// </summary>
        public static List<clsElementOfSchedule> colElementOfSchedule;


                /// <summary>
        /// Коллекция ниток для данного графика движения.
        /// </summary>
        public static List<TrainPath> colThread;

        /// <summary>
        /// Коллекция маршрутов, работающих на линиях для данного графика движения.
        /// </summary>
        public static List<Route> colRoute;

        /// <summary>
        /// Коллекция возможных типов ремонтов/осмотров ЭПС для данного графика движения.
        /// </summary>
        public static List<RepairType> colRepairType;

        /// <summary>
        /// Коллекция  ремонтов/осмотров ЭПС для данного графика движения.
        /// </summary>
        public static List<Repair> colRepair;
        public static RepairType TO1;

        /// <summary>
        /// 
        /// </summary>
        public static List<NightStayPoint> colNightStayPoint;

        /// <summary>
        /// 
        /// </summary>
        public static List<ElementOfMovementSchedule> colElementOfMovementSchedule = new List<ElementOfMovementSchedule>();

        /// <summary>
        /// 
        /// </summary>
        public static List<InspectionPoint> colInspectionPoint;

        /// <summary>
        /// Коллекция пунктов осмотра, в которых возможно провести ремонт (ТО-1).
        /// </summary>
        private static HashSet<InspectionPoint> points = new HashSet<InspectionPoint>();

        /// <summary>
        /// 
        /// </summary>
        private static List<RepairCandidate> Candidates = new List<RepairCandidate>();

        /// <summary>
        /// Коллекция цепочек, с которыми осуществляется работа.
        /// </summary>
        private static List<Chain> Chains = new List<Chain>();

        /// <summary>
        /// Коллекция звеньев, с которыми осуществляется работа.
        /// </summary>
        public static List<Link> LinksWhichContainRepair = new List<Link>();

        /// <summary>
        /// 
        /// </summary>
        public static List<Train> colTrain;

        /// <summary>
        /// 
        /// </summary>
        public static List<Wagon> colWagon;

        /// <summary>
        /// 
        /// </summary>
        public static List<WagonType> colWagonType;

        /// <summary>
        /// 
        /// </summary>
        public static List<TimeNorm> colTimeNorm;

        /// <summary>
        /// 
        /// </summary>
        public static List<Legend> colLegend;

        public static List<clsRazmerDvizheniya> colRazmerDvizheniya = new List<clsRazmerDvizheniya>();

        /// <summary>
        /// Коллекция режимов движения.
        /// </summary>
        public static List<MovementMode> colMovementMode;

        // -----------------------

        /// <summary>
        /// Временной интервал движения по линии (0:05:20 -- 1:02:00)
        /// </summary>
        public static TimeInterval MovementTime = new TimeInterval(5 * TimeConverter.secondsInHour + 20 * TimeConverter.secondsInMinute, TimeConverter.secondsInDay + 2 * TimeConverter.secondsInHour);

        /// <summary>
        /// Временной интервал утреннего пика   
        /// </summary>
        public static TimeInterval MorningPeak = new TimeInterval(7 * TimeConverter.secondsInHour, 9 * TimeConverter.secondsInHour);

        /// <summary>
        /// Временной интервал вечернего пика
        /// </summary>
        public static TimeInterval EveaningPeak = new TimeInterval(17 * TimeConverter.secondsInHour, 19 * TimeConverter.secondsInHour);

        // ------------------------

        /// <summary>
        /// Начальные цифры для резервных составов
        /// </summary>
        public const Int32 dNumber = 5000;

        /// <summary>
        /// Начальные цифры для резервных составов из депо
        /// </summary>
        public const Int32 dNumberBis = 1000;

        /// <summary>
        /// Текущая линия
        /// </summary>
        public static Line CurrentLine;

        /// <summary>
        /// Количество пикселов в мм по горизонтали
        /// </summary>
        public const int HpixelINmm = 3;

        /// <summary>
        /// Количество пикселов в мм по вертикали
        /// </summary>
        public const int VpixelINmm = HpixelINmm;

        /// <summary>
        /// Флаг того, что график строится для монорельса
        /// </summary>
        public static byte flgTypeLine;

        /// <summary>
        /// Время полного оборота в час-пик
        /// </summary>
        public static int TpoPik;

        /// <summary>
        /// Время полного оборота в час-непик
        /// </summary>
        public static int TpoNPik;

        public static bool flgReady = false; 
        
        public static string pathOfBD;

        //Флаг, указывающий на тип создаваемого графика
        //0 - не определен
        //1 - для рабочих дней
        //2 - для выходных дней
        public static ScheduleType scheduleType;

        /// <summary>
        /// Выполненый шаг в пошаговом синтезе график
        /// </summary>
        public static Int32 synthesisStep;

        public static Single scale = 1;
        /// <summary>
        /// Флаг выполнения пошагового синтеза графика
        /// </summary>
        public static Boolean flgSynthesisStep;

        public static Boolean flgOpen;     //True - открытие из базы данных, False - создание нового

        public static Boolean calculationNotStarted = true;

        /// <summary>
        /// Выбранный тип алгоритма решения задачи о назначениях
        /// </summary>
        public static MovementScheduleAlgorythm SelectedAlgorythm = MovementScheduleAlgorythm.NONE;

        public static CriteraTypes SelectedCriteraType = CriteraTypes.EQUALITY;

        public static ChainsSortingModes SelectedSortingMode = ChainsSortingModes.NONE;

        public static AlgorithmTerminationModes SelectedTerminationMode = AlgorithmTerminationModes.NONE;


        public static List<Core.TrainPath> colTrainPath = new List<TrainPath>();

        public static void AddTrainPath(Core.TrainPath path)
        {
            colTrainPath.Add(path);
        }

        /// <summary>
        /// Получение коды(id) указателей для выбраной станции
        /// </summary>
        /// <param name="_selectedStationCode"></param>
        /// <param name="pointerCodesForSelectedStation"></param>

        public static List<NightStayPoint> giveAllStationsPointers()
        {
            List<NightStayPoint> nightStayPointers = new List<NightStayPoint>();
            foreach (var thColNightStayPointer in colNightStayPoint)
            {
                nightStayPointers.Add(thColNightStayPointer);
            }
            return nightStayPointers;
        }

        public static List<NightStayPoint> giveStationPointers(Station thStation)
        {
            List<NightStayPoint> nightStayPointers = giveAllStationsPointers();
            nightStayPointers.RemoveAll(thNightStayPoint => thNightStayPoint.station != thStation);

            return nightStayPointers;
        }

        public static List<NightStayPoint> giveStationPointers(string stationName)
        {
            List<NightStayPoint> nightStayPointers = giveAllStationsPointers();
            nightStayPointers.RemoveAll(thNightStayPoint => thNightStayPoint.name != stationName);

            return nightStayPointers;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="route"></param>
        public static void InsertTO1Holyday(Route route)
        { }


        /// <summary>
        /// Построение графика оборота 
        /// </summary>
        /// <returns>Истина -- построение успешно, ложь в противном случае.</returns>
        public static Boolean CreateGraphicOborota()
        {
#if LOGER_ON
            Logger.Output("in CreateGraphicOborota()", typeof(MovementSchedule).FullName);
#endif
            if (colRoute == null || colRoute.Count == 0)
                return false;

            // Создание графика оборота из БД
            if (scheduleType == ScheduleType.ADDITIONAL)
            {
                var messageText =
                    "Данные исходного графика будут отредактированы в соответствии с треборваниями к графику оборота подвижного состава на выходные дни";
                var messageTitle = "Автоматизированный синтез графика оборота подвижного состава";
                var result = MessageBox.Show(messageText, messageTitle,MessageBoxButton.OKCancel);

                if (MessageBoxResult.Cancel == result)
                    return false;


                foreach (var route in colRoute)
                {
                    route.Repairs.Clear();
                    route.nextRoute = route;
                    route.prevRoute = route;
                    route.CreateMovementSchedule();
#if LOGER_ON
                    Logger.PrintMovementSchedule(route);
#endif
                    InsertTO1Holyday(route);
                }
            }
            else
            {
                foreach (var route in colRoute)
                    route.CreateMovementSchedule();
            }

            CreateMatrixOfRepairs();

            if (!flgOpen)
            {
                synthesisStep = 1;
                Dialogs.currentSynthesisStep = "fromNightRest";
            }

            /*
            if (mdlData.flgSynthesisStep && !mdlData.flgOpen)
                return (EndStepQuery());
            else 
                return true;
            */
            return (!flgSynthesisStep || flgOpen || Dialogs.EndStepQuery());

        }
        
        /// <summary>
        /// Метод, создающий цепочки.
        /// </summary>
        public static void MakeChains()
        {
            #region Логирование информации о входе в метод
#if LOGER_ON
            Logger.Output("in the method", typeof(MovementSchedule).FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            #endregion

            if (colRoute == null) return;

            var localChainOfRoutes = new List<Route>();
           
            while (colRoute.Any(r => r.State == RouteState.DEFAULT || r.State == RouteState.VISITED))
            {
                var currentRoute =
                    colRoute.FirstOrDefault(
                        r =>
                            (r.State == RouteState.DEFAULT || r.State == RouteState.VISITED) &&
                            r.prevRoute.nightStayPoint != null &&
                            r.prevRoute.nightStayPoint.depot != null);

                if (currentRoute == null) break;

                while (currentRoute.State != RouteState.CHECKED)
                {
                    var afterRepair = false;
                    do
                    {
                        if (currentRoute.State == RouteState.VISITED)
                        {
                            localChainOfRoutes.Add(currentRoute);
                            afterRepair = true;
                            
                            currentRoute.State = RouteState.CHECKED;
                            if (currentRoute.nightStayPoint != null && currentRoute.nightStayPoint.depot != null)
                                break;
                            currentRoute = currentRoute.nextRoute;
                        }

                        localChainOfRoutes.Add(currentRoute);
                        if (currentRoute.State == RouteState.DEFAULT && currentRoute.Repairs.Any(r => !r.isContinue))
                        {
                            currentRoute.State = RouteState.VISITED;
                            if (currentRoute.nextRoute.Repairs.Any(r => r.isContinue))
                                currentRoute.State = RouteState.CHECKED;
                            break;
                        }

                        #region repeat_region

                        currentRoute.State = RouteState.CHECKED;

                        if (currentRoute.nightStayPoint != null && currentRoute.nightStayPoint.depot != null)
                            break;

                        currentRoute = currentRoute.nextRoute;

                        #endregion
                    } while (true);

                    foreach (var point in localChainOfRoutes.SelectMany(r => r.depot.inspectionPoints))
                        points.Add(point);

                    // points.Add(MovementSchedule.colInspectionPoint.First(point => point.name.Equals("Новокосино"));

                    Chains.Add(new Chain(localChainOfRoutes){IsAfterRepair = afterRepair});
                    localChainOfRoutes.Clear();
                }

               
            }
            MakeLinks();

            #region Логирование информации о выходе из метода
#if LOGER_ON
            Logger.Output("out from method", typeof(MovementSchedule).FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            #endregion
        }

        /// <summary>
        /// Метод, создающий звенья для построенных цепочек (Chains).
        /// </summary>
        public static void MakeLinks()
        {
            #region Логирование информации о входе в метод
#if LOGER_ON
            Logger.Output("in the method", typeof (MovementSchedule).FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            #endregion

            foreach (var chain in Chains)
                chain.PrepareRoutes();

            #region Логирование информации о звеньях цепочек
#if LOGER_ON
            var separator = ", ";
            var buffer = new StringBuilder("Цепочки разобранные по звеньям:" + Environment.NewLine);
            foreach (var chain in Chains)
            {
                buffer.AppendFormat("\t{0}{1}\t\t", chain, Environment.NewLine);
                if (chain.Links != null)
                {
                    foreach (var link in chain.Links)
                        buffer.AppendFormat("[{0}]{1}", link, separator);
                    buffer.Remove(buffer.Length - separator.Length, separator.Length);
                }
                else
                {
                    buffer.Append("chain.Links == null");
                }
                buffer.AppendLine();
            }
            Logger.Output(buffer.ToString(), typeof(MovementSchedule).FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            buffer.Clear();
#endif
            #endregion

            var chainsRepository = SelectedSortingMode != ChainsSortingModes.NONE 
                ? Chains.Where(chain => chain.NumberOfRepairs > 0).OrderBy(chain => chain.NumberOfRepairs)
                : Chains.Where(chain => chain.NumberOfRepairs > 0);

            foreach (var chain in chainsRepository)
                LinksWhichContainRepair.AddRange(chain.Links.Where(link => link.ContainsRepair()));

            #region Логирование информации о построенной коллекции звеньев, требующих осмотра

            LinksWhichContainRepair.ForEach(link => Logger.Output(link.ToString()));

            #endregion

            #region Логирование информации о выходе из метода
#if LOGER_ON
            Logger.Output("out from method", typeof(MovementSchedule).FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
#endif
            #endregion
        }

        /// <summary>
        /// Заполняем коллекцию времён, в которые можно провести ТО-1 (для всех пунктов).
        /// </summary>
        public static void FillRepairCandidates()
        {
            if (colInspectionPoint == null)
                return;

            var delta = Math.Max(TimeConverter.secondsInHour, TO1.duration);

            //var pointsWithDepot =
            //    mdlData.colInspectionPoint.Where(point => point.Depot != null).OrderBy(point => point.Depot.name).ToList();
            //var otherPoints = mdlData.colInspectionPoint.Where(point => point.Depot == null)
            //    .OrderBy(point => point.name).ToList();
            //var tmpPoints = pointsWithDepot.Concat(otherPoints);

            foreach (var inspectionPoint in colInspectionPoint)
            {
                foreach (var timeInterval in inspectionPoint.schedule.workTimes)
                {
                    for (var currentTime = timeInterval.begin; currentTime <= timeInterval.end; currentTime += delta)
                    {
                        Candidates.Add(new RepairCandidate(currentTime, inspectionPoint));               
                    }
                }
            }

            var sb = new StringBuilder("Время (метросекунды) | Время | Место\n");
            foreach (var repairCandidate in Candidates)
                sb.AppendLine(repairCandidate.ToString());
            Logger.Output(String.Format("Коллекция кандадатов:{0}{1}", Environment.NewLine, sb.ToString()));

            /*
            if (points == null || points.Count == 0)
                return;

            //var delta = Math.Max(TimeConverter.secondsInHour, TO1.Duration);
            foreach (var point in points)
            {
                
                var schedule = point.schedule.Clone();
                
                if (point.depot != null && point.endBreakTime.HasValue && point.endBreakTime.Value <= TimeConverter.EveaningPeak.end)
                {
                    schedule.InsertBreakInterval(point.depot.breakTime);
                    schedule.InsertBreakInterval(TimeConverter.EveaningPeak);
                }
                
                foreach (var interval in schedule.workTimes)
                {
                    var currentTime = interval.begin;
                    Candidates.Add(new RepairCandidate(currentTime, point));
                    currentTime += delta;
                    while (currentTime <= interval.end)
                    {
                        Candidates.Add(new RepairCandidate(currentTime, point));
                        currentTime += delta;
                    }
                }
            }
             */
        }

        /**
          Портфель задач с индексацией "узлов", в которых расположены задачи
         Ветви 
         */

        // можно хранить ответы
        //private static HashSet<RepairCandidate> recursiveCandidates = new HashSet<RepairCandidate>();

        private static Double criterion = 0;

        private static List<Tuple<HashSet<AdvancedRepairCandidate>, Double>> global = new List<Tuple<HashSet<AdvancedRepairCandidate>, Double>>();

        public static List<Chain> preparedChains; 

        private static HashSet<AdvancedRepairCandidate> advancedCandidates = new HashSet<AdvancedRepairCandidate>(new AdvancedRepairCandidateComparer());

        //private static void PrepareChains()
        //{
        //    preparedChains = Chains.Where(chain => chain.Links != null && chain.Links.Any()).ToList();
        //    new ChainsLogger(@"D:\job\_Test", colLine.First()).Write(preparedChains);
        //    Logger.Output(String.Format("preparedChains = {0}", preparedChains.Count));
        //}
        private static void PrepareChains()
        {
            preparedChains = Chains.Where(chain => chain.Links != null && chain.Links.Any()).ToList();
            string binDirectory = @"\bin";
            string dir = @"\Save Directory\Log";
            string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string logDirectory = appDirectory.Substring(0, appDirectory.IndexOf(binDirectory)) + dir + "\\";
            new ChainsLogger(logDirectory, colLine.First()).Write(preparedChains);
            Logger.Output("preparedChains = {0}", preparedChains.Count);
        }

        public static Boolean isBreakByRemove = false;

        private static Boolean RecursiveSolver(Int32 chainIndex)
        {


#if BREAK_ON
            if (isBreak || isBreakByRemove)
                return true;
#endif

            #region Логирование информации о входе в метод
            Logger.Output("in the method ["+chainIndex+"]", typeof(MovementSchedule).FullName + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);
            #endregion

            return GetChainRepairs(preparedChains[chainIndex], chainIndex);
        }

        public static Boolean isBreak = false;

        private static UInt64 maxDepth = 0;

        private static UInt64 diveCounter = 0;

        private static Boolean GetChainRepairs(Chain chain, Int32 chainIndex)
        {
#if BREAK_ON
            if (isBreak || isBreakByRemove)
                return true;
#endif

            #region Логирование информации о входе в метод

            Logger.Output(
                String.Format("in the method  with chainIndex = {0}", chainIndex), 
                String.Format("{0}.{1}", typeof(MovementSchedule).FullName, System.Reflection.MethodBase.GetCurrentMethod().Name)
                );
            #endregion

            var isClearedChain = false;
            var chainFlag = false;
            for (var index = 0; index < Candidates.Count; ++index)
            //foreach (var repairCandidate in Candidates)
            {
                Counters.IncValue("all");

              // но это бред -- не может быть Links == null, нужно это где-то проверять.
                var firstLink = chain.Links.FirstOrDefault();
                if (firstLink == null)
                    // || !firstLink.FirstRoute.Times.Any() && (firstLink.SecondRoute == null || !firstLink.SecondRoute.Times.Any())) 
                {
                    Counters.IncValue("null");
                    continue;
                }
                
                // Если пытаемся назначить осмотр не в своём депо, то пропускаем
                if (Candidates[index].Point.depot != null && !firstLink.FirstRoute.depot.Equals(Candidates[index].Point.depot))
                    continue;
                

                var timeVar = new TimeVariant();
                Boolean? isRepairOnFirstRoute = null;
                Route route = null;
                    
                if (firstLink.FirstRoute.Times.Any() &&
                    //firstLink.FirstRoute.Times.Last.Value.IsInto(repairCandidate.BeginTime)
                    firstLink.FirstRoute.Times.Last.Value.IsInto(Candidates[index].BeginTime))
                {
                    timeVar = firstLink.FirstRoute.Times.Last.Value;
                    isRepairOnFirstRoute = true;
                    route = firstLink.FirstRoute;
                }
                // Полезное: L. R. Ford, Jr., D. R. Fulkerson. Flows in Networks, Princeton University Press, 1962.
                  
                // в том варианте как это написано у второго маршрута приоритет выше!!!
                // Второго маршрута в звене может не быть
                if (firstLink.SecondRoute != null &&
                    firstLink.SecondRoute.Times.Any() &&
                    //firstLink.SecondRoute.Times.First.Value.IsInto(repairCandidate.BeginTime)
                    firstLink.SecondRoute.Times.First.Value.IsInto(Candidates[index].BeginTime))
                {
                    timeVar = firstLink.SecondRoute.Times.First.Value;
                    isRepairOnFirstRoute = false;
                    route = firstLink.SecondRoute;
                }

                // Если не на первый и не на второй, то не внутри, и пропускаем. 
                if (!isRepairOnFirstRoute.HasValue)
                {
                    Counters.IncValue("not_inside");
                    continue;
                }


                // var advancedCandidate = new AdvancedRepairCandidate(repairCandidate, route);
                var advancedCandidate = new AdvancedRepairCandidate(Candidates[index], route, index, (Int32)route.number);
                var isAValidCandidate = false;
                if (!advancedCandidates.Any(advancedCandidate.Equals))
                    isAValidCandidate = advancedCandidates.Add(advancedCandidate);
                if (!isAValidCandidate)
                {
                    Counters.IncValue("bussy");
                    continue;
                }

                #region Логирование информации о цепочке, текущем кандидате и глубине погружения
#if LOGER_ON

                Logger.Output(
                    String.Format("{0} <-> {1} chainIndex = [{2}]", chain, advancedCandidate, chainIndex), 
                    String.Format("{0}.{1}", typeof(MovementSchedule).FullName, System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif
                #endregion

                Int32 componentOfCriterion = 0;
                switch (SelectedCriteraType)
                { case CriteraTypes.EQUALITY:
                        componentOfCriterion = advancedCandidate.BeginTime() - timeVar.Tdes;
                        break;
                  case CriteraTypes.MIN_ADDITIONAL_TIME:
                        componentOfCriterion = advancedCandidate.BeginTime(); //- firstLink.FirstRoute.Times.First
                        break;
                }
                criterion += componentOfCriterion * componentOfCriterion;

                chainFlag = true;

                if (chain.NumberOfRepairs > 1)
                {
                    try
                    {
                        var newChain = chain.TrimAndClone(advancedCandidate);

                        #region Логгирование информации о текущей цепочке и её индексе

#if LOGER_ON
                        Logger.Output(
                            String.Format("{0} chainIndex = [{1}]", newChain, chainIndex),
                            String.Format("{0}.{1}", typeof (MovementSchedule).FullName,
                                System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif

                        #endregion
                                    
                        GetChainRepairs(newChain, chainIndex);
#if BREAK_ON
                        if (isBreak || isBreakByRemove)
                            return true;
#endif
                    }
                    catch (IntervalToPointInMovementException e)
                    {
                        Counters.IncValue("to_point");

                        #region Логирование инфоомации о неудачном построении интервала.

#if LOGER_ON
                        Logger.Output(e.ToString(),
                            String.Format("{0}.{1}", typeof (MovementSchedule).FullName,
                                System.Reflection.MethodBase.GetCurrentMethod().Name));
#endif

                        #endregion
                    }

                    isClearedChain = true;
                }
                else // Сюда попадаем, если число осмотров равно нулю
                {
                    ++chainIndex;
                    if (chainIndex < preparedChains.Count)
                    {
                        RecursiveSolver(chainIndex);
                        --chainIndex;
#if BREAK_ON
                        if (isBreak || isBreakByRemove)
                        {
                            #region Вывод информации о выходе из метода
                            Logger.Output("Выходим по прерыванию. Число осмотров равно нулю, а цепочки пока не все просмотрены.");
                            #endregion
                            return true;
                        }
#endif
                    }
                    else
                    {
                        #region Вывод информации о выходе из метода
                        Logger.Output("Подготовленные цепочки закончились");
                        #endregion
                            
                        MakeAnswerValue(ref chainIndex);
                        
                        Counters.IncValue("success");
#if BREAK_ON
                        UInt64 stopIndex = 10;
                        if (Counters.GetCounterValue("all") > stopIndex)
                        {
                            isBreak = true;
                            MessageBox.Show(
                                String.Format("{0} вариант(а/ов) назначения было построено. Глубина погружения составила {1}.", advancedCandidates.Count, maxDepth), System.Reflection.MethodBase.GetCurrentMethod().Name, MessageBoxButton.OK, MessageBoxImage.Information);
                            
                            #region Вывод информации о выходе из метода
                            Logger.Output(String.Format("Выходим, потому что привысили значение stopIndex {0}", stopIndex));
                            #endregion
                            
                            return true;
                        }
#endif             
                    }
                }


                advancedCandidates.Remove(advancedCandidate);
#if BREAK_ON
                if ((ulong)advancedCandidates.Count > maxDepth)
                {
                    
                    maxDepth = (ulong)advancedCandidates.Count;
                    diveCounter++;
                    Logger.Output(String.Format("maxD = {0}; dCounter = {1}", maxDepth, diveCounter), "Let's Dive!");
                    //isBreakByRemove = true;
                    MakeAnswerValue(ref chainIndex);
                    return true;
                }
#endif

                criterion -= componentOfCriterion*componentOfCriterion;
                            
                // нет глубокого копирования, времена перетераются, поэтому пересчёт
                //if (!isClearedChain) 
                //    continue;
                if (isClearedChain)
                {
                    chain.PrepareTimes();
                    isClearedChain = false;
                }
            }
            return chainFlag;
        }

        private static void MakeAnswerValue(ref int chainIndex)
        {
            var box = new HashSet<AdvancedRepairCandidate>();
#if LOGER_ON
                        var sb = new StringBuilder(String.Format("size = {0}[{1}]{2}", advancedCandidates.Count, global.Count, Environment.NewLine));
#endif
            var buffer = new StringBuilder();
            foreach (var advancedRepairCandidate in advancedCandidates)
            {
#if LOGER_ON
                            sb.AppendLine(advancedRepairCandidate.ToString());
#endif
                buffer.AppendFormat("{0}; ", advancedRepairCandidate.GetPair());
                box.Add(advancedRepairCandidate.Clone() as AdvancedRepairCandidate);
            }

            #region Логгирование информации о составе коллекции

#if LOGER_ON
                        Logger.Output(sb.ToString());
#endif

            #endregion

            var newCr = Math.Sqrt(criterion)/(TimeConverter.secondsInHour*box.Count);
#if STATISTIC_ON
          //  StatisticLogger.Output(global.Count, newCr, buffer.ToString(), "old");
#endif
#if BREAK_ON
            global.Clear();
#endif
            global.Add(new Tuple<HashSet<AdvancedRepairCandidate>, Double>(box, newCr));
#if STATISTIC_ON
        //    StatisticLogger.Output(global.Count, newCr, buffer.ToString(), "new");

            StatisticLogger.Output(global.Count, newCr, buffer.ToString());
#endif
            --chainIndex;
        }


        /// <summary>
        /// Нигде не используется
        /// </summary>
        public static HashSet<AdvancedRepairCandidate> SaveCandidates;

        public static CountersForm Counters;
        

        /// <summary>
        /// Построение назначения осмотров.
        /// </summary>
        /// <returns>Истина, если метод сошёлся, и ложь -- в противном случае.</returns>
        public static Boolean CreateMatrixOfRepairs()
        {
#if STATISTIC_ON
            StatisticLogger.filename = "stat_" + DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".txt";
#endif
            //SelectedAlgorythm = MovementScheduleAlgorythm.RECURSIVE;
            calculationNotStarted = false;

            MakeChains();
            FillRepairCandidates();

            PrepareChains();
            CreateExcel();
            
            #region Логирование предварительной информации о маршрутах и временах
#if LOGER_ON
            var buffer = new StringBuilder(String.Format("before processing:{0}", Environment.NewLine));
            foreach (var route in colRoute)
            {
                buffer.AppendLine(String.Format("{1} -- №{0:D3} -- {2}", 
                    route.number,
                    route.prevRoute.nightStayPoint == null ? "Линия" : "Депо",
                    route.nightStayPoint == null ? "Линия" : "Депо"));
                foreach (var timeVariant in route.Times)
                    buffer.AppendLine(String.Format("\t{0}", timeVariant));
            }
            Logger.Output(buffer.ToString(), String.Format("{0}.{1}", typeof(MovementSchedule).FullName, System.Reflection.MethodBase.GetCurrentMethod().Name));

#endif
            #endregion

            //var answer1 = from global1 in global where global1.Item2 == minimalCriteryValue select global1.Item1;


            var result = System.Windows.MessageBoxResult.No;

            result = MessageBox.Show("Подготовка данных закончена. Проверить условие наличия ресурсов, необходимых для построения графика оборота?", "График оборота",
                 MessageBoxButton.YesNo, MessageBoxImage.Asterisk);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                if (!Verification() || LinksWhichContainRepair.Count > Candidates.Count)
                {
                    result = MessageBox.Show("Ресурсов недостаточно. Пытаемся поправить ситуацию?", "График оборота",
                          MessageBoxButton.YesNo, MessageBoxImage.Asterisk);                    
                    if (result == System.Windows.MessageBoxResult.Yes)
                    {

//                        preparedChains.ForEach(chain => { var t = chain.AdditionalTime; });

                        var chainsRepositoryAboutAdditionalTime =
                           preparedChains.OrderBy(chain => chain.AdditionalTime);

                        foreach (var chain in chainsRepositoryAboutAdditionalTime)
                            { var t = chain.AdditionalTime;}                         
                    }
                   else
                    {
                    return false;                        
                    }

                }
            }

            if (SelectedAlgorythm != MovementScheduleAlgorythm.NONE)
            {
               result = MessageBox.Show("Подготовка данных закончена. Продолжить обсчёт?", "График оборота",
                    MessageBoxButton.YesNo, MessageBoxImage.Asterisk);
            }

            if (result == System.Windows.MessageBoxResult.Yes  )
            {
            var timer = Stopwatch.StartNew();

            switch (SelectedAlgorythm)
            {
                case MovementScheduleAlgorythm.RECURSIVE:
                {
                    // флаг и "все на все"
                    Counters = new CountersForm();
                    Counters.StartTimer();
                    RecursiveSolver(0);
                    Counters.StopTimer();

                    #region Запись результата работы алгоритма в лог-файл


                        foreach (var tuple in global)
                        {
                            Logger.Output(tuple.Item1.Count.ToString("D"), showInfo: false);
                            foreach (var tmp in tuple.Item1)
                            {
                                Logger.Output(
                                    String.Format("\t{0} [{1}]; {2} №{3}", tmp.BeginTime(),
                                        TimeConverter.SecondsToString(tmp.BeginTime()),
                                        tmp.Point().name, tmp.Route().number), showInfo: false);
                            }
                            Logger.Output(String.Format("Коэффициент = {0}", tuple.Item2), "RepairCandidats (global):");
                        }

                        foreach (var tmp in advancedCandidates)
                            Logger.Output(tmp.ToString(), "RepairCandidats:");

                    #endregion

                    try
                    {
                        foreach (var advancedRepairCandidate in global[0].Item1)
                        {
                            var route = advancedRepairCandidate.Route();
                            var repair = new Repair(TO1, route, advancedRepairCandidate.Point(), false)
                            {
                                beginTime = advancedRepairCandidate.BeginTime(),
                                endTime = advancedRepairCandidate.BeginTime() + TO1.duration
                            };
                            route.addRepair(repair);
                        }
                    }
                    catch (ArgumentNullException exception)
                    {
                        MessageBox.Show("Список пуст!");
                    }

                    var answerBuffer = new StringBuilder();
                    var eps = Double.Parse("10e-6");
                    var minimalCriterionValue = global.Min(el => el.Item2);
                    var branchByMinimalCriterion = from element in global
                        where Math.Abs(element.Item2 - minimalCriterionValue) <= eps
                        select element.Item1;

                    #region Запись наилучшего варианта в лог-файл

#if LOGER_ON
                        answerBuffer.AppendLine("Итоговый ответ (минимальное значение критерия)");
                        foreach (var advancedRepairCandidate in branchByMinimalCriterion.ToList().First())
                            answerBuffer.AppendLine("\t" + advancedRepairCandidate);
                       

                        Logger.Output(answerBuffer.ToString(), typeof(MovementSchedule) + "." + System.Reflection.MethodBase.GetCurrentMethod().Name);



                        Logger.Output("-----------------------------------------------");

                        var pl = from r in global
                                 orderby r.Item2
                                 group r by r.Item2 into grp
                                 select new { key = grp.Key, cnt = grp.Count() };

                        foreach (var orderedElement in pl)
                        {
                            Logger.Output(String.Format("{0}    {1}", orderedElement.key,
                                orderedElement.cnt),showInfo: false);
                        }
#endif

                    #endregion

                    break;
                }
                case MovementScheduleAlgorythm.HUNGARIAN:
                    {
                        //var inputData = MakeInputData(out oldArray);
                        //var input = CreateMatrixOfCosts(out oldArray);
                        var input = MakeCosts();
                        var dataStr = String.Format("Маршрутов {0}, Ремонтов {1}", input.Rows(), input.Columns());
#if LOGER_ON
                        Logger.Output(dataStr, "MS");
                        var buf = new StringBuilder("\n");
                        for (uint i = 0; i < input.Rows(); ++i)
                        {
                            for (uint j = 0; j < input.Columns(); ++j)
                                buf.AppendFormat("{0}\t", input.At(i, j));
                            buf.AppendLine();
                        }
                        Logger.Output(buf.ToString(), showInfo: false);

#endif
                        if (input.Rows() > input.Columns())

                        /*
                    var dataStr = String.Format("Маршрутов {0}, Ремонтов {1}", inputData.GetLength(0), inputData.GetLength(1));//repairRoutes.Count, repairTimes.Count);
                    Logger.output(dataStr, "MS");

                    if (inputData.GetLength(0) > inputData.GetLength(1))
                    */
                        //if (repairRoutes.Count > repairTimes.Count)
                        {

                            Messages.Message.showAboutHungarianAlgorithm(HungarianState.NOT_ENOUGH_TIME_AND_SPACE);
                            return false;
                        }

                        var solver = new Solver<Int32>();
                        solver.Solve(input);

                        var answer = solver.GetAnswer();
                        ParseResultLinks(answer);
                        break;
                    }
           timer.Stop();
            var totalTime = timer.Elapsed.ToString("c");
            MessageBox.Show("Всё " + totalTime);

            var resultStr = String.Format("allTime = {0}; nullCnt = {1}; notInsedeCnt = {2}; bussyPointCnt = {3}; succes = {4}", totalTime, Counters.GetCounterValue("null"), Counters.GetCounterValue("not_inside"), Counters.GetCounterValue("bussy"), global.Count);     

            Logger.Output(resultStr);
            }
 
            }
            return true;
        }
        public static void CreateExcel()
        {
            string BinDirectory = @"\bin";
            string DinamicTailsnDirectory = @"\Save Directory";

            string pathToMagicDirectory = AppDomain.CurrentDomain.BaseDirectory;

            int positionBinDirectory = pathToMagicDirectory.IndexOf(BinDirectory);
            if (positionBinDirectory > 0)
            {
                pathToMagicDirectory = pathToMagicDirectory.Substring(0, pathToMagicDirectory.IndexOf(BinDirectory)) + DinamicTailsnDirectory + "\\";
            }

            var xlApp = new Excel.Application();
            if (xlApp == null)
            {
                MessageBox.Show("Excel is not properly installed!!");
                return;
            }

            var xlWorkBook = xlApp.Workbooks.Add();
            var xlWorkSheet3 = xlWorkBook.Worksheets.Item[1] as Excel.Worksheet;
            xlWorkSheet3.Name = "Routes";
            int totalRoutes = 0;
            if (xlWorkSheet3 != null)
                {
                xlWorkSheet3.Cells[1, 1] = "№";
                xlWorkSheet3.Cells[1, 2] = "Цепочка";
                xlWorkSheet3.Cells[1, 3] = "Маршрут в цепочке";
                xlWorkSheet3.Cells[1, 4] = "Элемент ГО";
                xlWorkSheet3.Cells[1, 5] = "tstart";
                xlWorkSheet3.Cells[1, 6] = "tend";
                xlWorkSheet3.Cells[1, 7] = "После ремонта";
                var rowIndex = 1;
                for (int i = 0; i < preparedChains.Count; i++)
                {
                    var tmpRoute = preparedChains[i].rawRoutes.First;

                    while (tmpRoute != null)
                    {
                        var tmpElement = tmpRoute.Value.ElementsOfMovementSchedule.First;

                        while (tmpElement != null)
                        {
                            ++ rowIndex;
                            xlWorkSheet3.Cells[rowIndex, 1] = rowIndex - 1; // №№ п/п
                            xlWorkSheet3.Cells[rowIndex, 2] = i + 1; // preparedChains[i];
                            xlWorkSheet3.Cells[rowIndex, 3] = tmpRoute.Value.ToString() ;
                            xlWorkSheet3.Cells[rowIndex, 4] = tmpElement.Value.ToString();
                            xlWorkSheet3.Cells[rowIndex, 5] = tmpElement.Value.beginTime;
                            xlWorkSheet3.Cells[rowIndex, 6] = tmpElement.Value.endTime;
                            xlWorkSheet3.Cells[rowIndex, 7] = preparedChains[i].IsAfterRepair;
                            tmpElement = tmpElement.Next;
                            totalRoutes ++;
                        }
                        tmpRoute = tmpRoute.Next;
                    }
                }
            }

            var xlWorkSheet2 = xlWorkBook.Sheets.Add();
            xlWorkSheet2.Name = "Candidates";
            if (xlWorkSheet2 != null)
            {
                xlWorkSheet2.Cells[1, 1] = "№";
                xlWorkSheet2.Cells[1, 2] = "tStart";
                xlWorkSheet2.Cells[1, 3] = "Place";
                for (int i = 0; i < Candidates.Count; i++)
                {
                    var rowindex = i + 2;
                    xlWorkSheet2.Cells[rowindex, 1] = i+1;
                    xlWorkSheet2.Cells[rowindex, 2] = Candidates[i].BeginTime;
                    xlWorkSheet2.Cells[rowindex, 3] = Candidates[i].Point.code;
                }
            }

            var xlWorkSheet1 = xlWorkBook.Sheets.Add();
            xlWorkSheet1.Name = "Links";
            if (xlWorkSheet1 != null)
            {
                xlWorkSheet1.Cells[1, 1] = "№";
                xlWorkSheet1.Cells[1, 2] = "Цепочка";
                xlWorkSheet1.Cells[1, 3] = "tstart1";
                xlWorkSheet1.Cells[1, 4] = "tdes1";
                xlWorkSheet1.Cells[1, 5] = "tend1";
                xlWorkSheet1.Cells[1, 6] = "tstart2";
                xlWorkSheet1.Cells[1, 7] = "tdes2";
                xlWorkSheet1.Cells[1, 8] = "tend2";
                xlWorkSheet1.Cells[1, 9] = "Name";
                for (int i = 0; i < LinksWhichContainRepair.Count; i++)
                {
                    var rowIndex = i + 2;
                    xlWorkSheet1.Cells[rowIndex, 1] = i+1;
                    xlWorkSheet1.Cells[rowIndex, 2] = LinksWhichContainRepair[i].ParentChain.ToString();
                ; // preparedChains[i].Links;

                    var currentLink = LinksWhichContainRepair[i];
                    var firstRouteFirstTime = currentLink.FirstRoute.Times.First.Value;
                    xlWorkSheet1.Cells[rowIndex, 3] = firstRouteFirstTime.Tmin;
                    xlWorkSheet1.Cells[rowIndex, 4] = firstRouteFirstTime.Tdes;
                    xlWorkSheet1.Cells[rowIndex, 5] = firstRouteFirstTime.Tmax;

                    if (currentLink.SecondRoute != null)
                    {
                        var secondRouteFirstTime = currentLink.SecondRoute.Times.First.Value;
                        xlWorkSheet1.Cells[rowIndex, 6] = secondRouteFirstTime.Tmin;
                        xlWorkSheet1.Cells[rowIndex, 7] = secondRouteFirstTime.Tdes;
                        xlWorkSheet1.Cells[rowIndex, 8] = secondRouteFirstTime.Tmax;
                    }
                    else
                    {
                        xlWorkSheet1.Cells[rowIndex, 6] = 0;
                        xlWorkSheet1.Cells[rowIndex, 7] = 0;
                        xlWorkSheet1.Cells[rowIndex, 8] = 0;
                    }
                    #region Old Code
                    //xlWorkSheet1.Cells[rowIndex, 3] = LinksWhichContainRepair[i].FirstRoute.time.Tmin;
                    //xlWorkSheet1.Cells[rowIndex, 4] = LinksWhichContainRepair[i].FirstRoute.time.Tdes;
                    //xlWorkSheet1.Cells[rowIndex, 5] = LinksWhichContainRepair[i].FirstRoute.time.Tmax;

                    //xlWorkSheet1.Cells[rowIndex, 6] = LinksWhichContainRepair[i].SecondRoute.time.Tmin;
                    //xlWorkSheet1.Cells[rowIndex, 7] = LinksWhichContainRepair[i].SecondRoute.time.Tdes;
                    //xlWorkSheet1.Cells[rowIndex, 8] = LinksWhichContainRepair[i].SecondRoute.time.Tmax;
                    #endregion

                    xlWorkSheet1.Cells[rowIndex, 9] = LinksWhichContainRepair[i].ToString();
                }
            }

            var xlWorkSheet = xlWorkBook.Sheets.Add();
            xlWorkSheet.Name = "Chains";
            if (xlWorkSheet != null)
            {
                xlWorkSheet.Cells[1, 1] = "№";
                xlWorkSheet.Cells[1, 2] = "T";
                xlWorkSheet.Cells[1, 3] = "Name";
                xlWorkSheet.Cells[1, 4] = "NumberOfRepairs";
                xlWorkSheet.Cells[1, 5] = "TotalChains";
                xlWorkSheet.Cells[1, 6] = "TotalLinks";
                xlWorkSheet.Cells[1, 7] = "TotalCandidates";
                xlWorkSheet.Cells[1, 8] = "TotalRoutes";
                for (int i = 0; i < preparedChains.Count; i++)
                {
                    var rowIndex = i + 2;
                    xlWorkSheet.Cells[rowIndex, 1] = i+1;
                    xlWorkSheet.Cells[rowIndex, 2] = preparedChains[i].TimeWithoutRepair.ToString();
                    xlWorkSheet.Cells[rowIndex, 3] = preparedChains[i].ToString();
                    xlWorkSheet.Cells[rowIndex, 4] = preparedChains[i].NumberOfRepairs.ToString();
                    //totalLinks += preparedChains[i].NumberOfRepairs;
                    xlWorkSheet.Cells[2, 5] = preparedChains.Count;
                    xlWorkSheet.Cells[2, 6] = LinksWhichContainRepair.Count;
                    xlWorkSheet.Cells[2, 7] = Candidates.Count;
                    xlWorkSheet.Cells[2, 8] = totalRoutes;
                }
                
            }
            var address = pathToMagicDirectory + DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss-ffffff") + ".xls";

            xlWorkBook.SaveAs(Filename: address, FileFormat: Excel.XlFileFormat.xlWorkbookNormal, AccessMode: Excel.XlSaveAsAccessMode.xlExclusive);

            xlWorkBook.Close(SaveChanges: true);

            xlApp.Quit();

            MessageBox.Show("Excel file created , you can find the file {address}");
            ExtraTimewindow extraTimewindow=new ExtraTimewindow();
        }
        private static bool Verification()
        {
            // не так, считаем осмотры типа ТО-1
            // var routeDict = mdlData.colDepot.ToDictionary(depot => depot, depot => mdlData.colRoute.Count(r => r.depot == depot));
                   

            var depotsAndLinksCounterDictionary = colDepot.ToDictionary(depot => depot,
                 depot => LinksWhichContainRepair.Count(link => link.FirstRoute.depot == depot));
            // depot => preparedChains.Count(ch => ch.Links[1].FirstRoute.depot == depot));  

            var allLinksToDepot = colDepot.ToDictionary(depot => depot,
                 depot => preparedChains.SelectMany(chain => chain.Links).Count(link => link.FirstRoute.depot == depot));

            // Количество кандидатов (пара место, время начала), у которых нет привязки к депо. Иными словами, составы приписанные к любому депо могут быть осмотрены при помощи этих кандидатов.
            var additionalCounter =
                Candidates.Count(candidate => colInspectionPoint.Where(point => point.depot == null).Contains(candidate.Point));

            var inspectionPointsWithDepot = colInspectionPoint.Where(point => point.depot != null);

            // тут вроде верно дубликат ключей в коллекции!!!

            var depotAndCandidatesCounter = new Dictionary<Depot, Int32>();
            foreach (var inspectionPoint in inspectionPointsWithDepot)
            {
                if (depotAndCandidatesCounter.ContainsKey(inspectionPoint.depot))
                    depotAndCandidatesCounter[inspectionPoint.depot] += Candidates.Count(candidate => candidate.Point == inspectionPoint);
                else
                    depotAndCandidatesCounter[inspectionPoint.depot] = Candidates.Count(candidate => candidate.Point == inspectionPoint) + additionalCounter;
            }

            //var candDict = tmp.ToDictionary(inspectionPoint => inspectionPoint.depot, inspectionPoint => Candidates.Count(cand => cand.Point == inspectionPoint) + additionalCounter);


            return colDepot.Aggregate(true, (current, depot) => current && (depotsAndLinksCounterDictionary[depot] <= depotAndCandidatesCounter[depot]));
        }


        /// <summary>
        /// Разбор результирующих звеньев.
        /// </summary>
        /// <param name="answers"></param>
        public static void ParseResultLinks(List<Answer<Int32>> answers)
        {
            foreach (var ans in answers)
            {
                Logger.Output(String.Format("[{0}, {1}] = {2}", ans.Row, ans.Column, ans.Value));
            }


            // нет 9

            // 

            foreach (var answer in answers)
            {
                var tmpLink = LinksWhichContainRepair[answer.Row];

                var tmpRepairCandidat = Candidates[answer.Column];


                Route tmpRouteCandidat = null;
                var time = tmpLink.FirstRoute.Times.Last.Value;
                if (time.IsInto(tmpRepairCandidat.BeginTime))
                {
                    tmpRouteCandidat = tmpLink.FirstRoute;
                    var linePoint1 = tmpRepairCandidat.Point;
                    var endTime1 = tmpRepairCandidat.BeginTime + TO1.duration;

                    var element1 =
                        tmpRouteCandidat.ElementsOfMovementSchedule.SingleOrDefault(
                            el =>
                                el.beginTime <= tmpRepairCandidat.BeginTime &&
                                tmpRepairCandidat.BeginTime <= el.endTime);

                    if (element1 != null)
                    {
                        var point = element1.endPoint;
                        element1.endPoint = null;
                        var time1 = element1.endTime;
                        element1.endTime = tmpRepairCandidat.BeginTime;
                        new ElementOfMovementSchedule().Initialize(tmpRouteCandidat, endTime1, time1, null, point);

                        tmpRouteCandidat.Repairs.Add(new Repair
                        {
                            beginTime = tmpRepairCandidat.BeginTime,
                            endTime = endTime1,
                            route = tmpRouteCandidat,
                            inspectionPoint = linePoint1,
                            type = TO1
                        });
                    }
                }
                // а если не попали ?!
                else if (tmpLink.SecondRoute != null)
                {
                    var time2 = tmpLink.SecondRoute.Times.First.Value;
                    //var time2 = tmpLink.FirstRoute.Times.Last.Value;
                    if (time2.IsInto(tmpRepairCandidat.BeginTime))
                    {
                        tmpRouteCandidat = tmpLink.SecondRoute;
                        var linePoint1 = tmpRepairCandidat.Point;
                        var endTime1 = tmpRepairCandidat.BeginTime + TO1.duration;

                        var element1 =
                            tmpRouteCandidat.ElementsOfMovementSchedule.SingleOrDefault(
                                el =>
                                    el.beginTime <= tmpRepairCandidat.BeginTime &&
                                    tmpRepairCandidat.BeginTime <= el.endTime);

                        if (element1 != null)
                        {
                            var point = element1.endPoint;
                            element1.endPoint = null;
                            var time1 = element1.endTime;
                            element1.endTime = tmpRepairCandidat.BeginTime;
                            new ElementOfMovementSchedule().Initialize(tmpRouteCandidat, endTime1, time1, null, point);

                            tmpRouteCandidat.Repairs.Add(new Repair
                            {
                                beginTime = tmpRepairCandidat.BeginTime,
                                endTime = endTime1,
                                route = tmpRouteCandidat,
                                inspectionPoint = linePoint1,
                                type = TO1
                            });
                        }
                    }
                }
                //else
                {
                    tmpRouteCandidat = tmpLink.FirstRoute;
                    tmpRouteCandidat.Repairs.Add(new Repair
                    {
                        beginTime = tmpRepairCandidat.BeginTime,
                        endTime = tmpRepairCandidat.BeginTime + TO1.duration,
                        route = tmpRouteCandidat,
                        inspectionPoint = tmpRepairCandidat.Point,
                        type = TO1,
                        IsFake = true
                    });
                }
                /*
                if (tmpLink.TupleRoutes.Item2 == null)
                {
                    var time = item1.Times.First.Value; 

                    if (time.IsInto(tmpRepairCandidat.beginTime))
                        tmpRouteCandidat = item1;
                    else
                        Logger.Output(String.Format("Ошибка в методе!!!\nitem1 = {0}\nitem2 = null\nr.bTime={1}",
                            tmpLink.TupleRoutes.Item1.time, tmpRepairCandidat.beginTime));
                }
                else
                {
                    var time = item1.Times.Last.Value;
                    if (time.IsInto(tmpRepairCandidat.beginTime))
                        tmpRouteCandidat = tmpLink.TupleRoutes.Item1;
                    else if (tmpLink.TupleRoutes.Item2.Times.Last.Value.IsInto(tmpRepairCandidat.beginTime))
                        tmpRouteCandidat = tmpLink.TupleRoutes.Item2;
                    else
                        Logger.Output(String.Format("Ошибка в методе!!!\nitem1 = {0}\nitem2 = {1}\nr.bTime={2}",
                            tmpLink.TupleRoutes.Item1.time, tmpLink.TupleRoutes.Item2.time, tmpRepairCandidat.beginTime));
                }


                var linePoint = tmpRepairCandidat.point;
                var endTime = tmpRepairCandidat.beginTime + TO1.Duration;

                var element =
                    tmpRouteCandidat.ElementsOfMovementSchedule.SingleOrDefault(
                        el => el.beginTime <= tmpRepairCandidat.beginTime && tmpRepairCandidat.beginTime <= el.endTime);

                if (element != null)
                {
                    var point = element.endPoint;
                    element.endPoint = null;
                    var time = element.endTime;
                    element.endTime = tmpRepairCandidat.beginTime;
                    new ElementOfMovementSchedule().Initialize(tmpRouteCandidat, endTime, time, null, point);

                    tmpRouteCandidat.Repairs.Add(new Repair
                    {
                        beginTime = tmpRepairCandidat.beginTime,
                        endTime = endTime,
                        route = tmpRouteCandidat,
                        inspectionPoint = linePoint,
                        type = TO1
                    });
                }


                */
            }

            colRoute.ForEach(route => route.Repairs.ForEach(repair => Logger.Output(route.number.ToString("D3") + " -->" + repair.ToString())));
        }

        /// <summary>
        /// Метод создания матрицы весов (стоимостей, затрат).
        /// </summary>
        /// <returns>Матрица весов (стоимостей, затрат).</returns>
        public static Matrix<Int32> MakeCosts()
        {
#if LOGER_ON
            Logger.Output("In MakeCost method", typeof(MovementSchedule).FullName);
#endif
            var result = new Int32[LinksWhichContainRepair.Count, Candidates.Count];
            // Штраф (вес несуществующего ребра).
            var penalty = Int32.MaxValue;

            var linkCounter = 0;
            foreach (var link in LinksWhichContainRepair)
            {
                var candidateCounter = 0;
                foreach (var candidate in Candidates)
                {
                    result[linkCounter, candidateCounter] = Cost(link, candidate, penalty);
                    ++candidateCounter;
                }
                ++linkCounter;
            }

            return new Matrix<Int32>(result);
        }


        /// <summary>
        /// Метод вычисления веса ребра, соединяющего звено (link) и кандидата (candidate).
        /// </summary>
        /// <param name="link">Вершина из доли звеньев цепочек.</param>
        /// <param name="candidate">Вершина из доли кандидатов (осмотров).</param>
        /// <param name="penalty">Вес несуществующего ребра (штраф).</param>
        /// <returns>Вес ребра, связывающего вершину из доли звеньев цепочек с вершиной из доли кандидатов (осмотров).</returns>
        private static Int32 Cost(Link link, RepairCandidate candidate, Int32 penalty)
        {
            if (!candidate.Point.colDepot.Contains(link.FirstRoute.depot))
                return penalty;

            //link.FirstRoute.Times.Where(t => t.IsInto(candidate.BeginTime));

           
            var firstMin = link.FirstRoute.Times.Min(t => Math.Abs(t.Tdes - candidate.BeginTime));
            
            #region Логирование информации если номер 18
#if LOGER_ON
            if (link.FirstRoute.number == 18)
                Logger.Output("firstMin = " + firstMin);
#endif
            #endregion
           
            var secondMin = link.SecondRoute == null
                ? penalty
                : link.SecondRoute.Times.Min(t => Math.Abs(t.Tdes - candidate.BeginTime));

            #region Логирование информации если номер 18
#if LOGER_ON
            if (link.FirstRoute.number == 18)
                Logger.Output("SecondMin = " + secondMin);
#endif
            #endregion

            return Math.Min(firstMin, secondMin);

            #region old version code
            /*
            if (!(candidate.Point.colDepot.Contains(link.FirstRoute.depot)
                && link.FirstRoute.Times.First.Value.IsInto(candidate.BeginTime)))
                return penalty;

            // Дыра:
            if (link.SecondRoute == null || !candidate.Point.colDepot.Contains(link.SecondRoute.depot))
                return Math.Abs(link.FirstRoute.time.Tdes - candidate.BeginTime);

            return (link.SecondRoute.Times.Last.Value.IsInto(candidate.BeginTime))
                ? Math.Min(
                    Math.Abs(link.FirstRoute.Times.Last.Value.Tdes - candidate.BeginTime),
                    Math.Abs(link.SecondRoute.Times.First.Value.Tdes - candidate.BeginTime))
                : penalty;
             */

            /*
            if (link.SecondRoute == null || !candidat.Point.colDepot.Contains(link.SecondRoute.depot))
                return Math.Abs(link.FirstRoute.time.Tdes - candidat.BeginTime);

            return (link.SecondRoute.Times.Last.Value.IsInto(candidat.BeginTime))
                ? Math.Min(
                    Math.Abs(link.FirstRoute.time.Tdes - candidat.BeginTime),
                    Math.Abs(link.SecondRoute.time.Tdes - candidat.BeginTime))
                : penalty;
             */
            #endregion
        }
    }


}
