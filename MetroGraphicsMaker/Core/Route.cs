/** 
 * @organization Departament "Control and Informatics in Technical Systems" (CITS@MIIT)
 * @author Konstantin Filipchenko (@e-mail konstantin-649@mail.ru)
 * @version 0.1 
 * @date 2013-03-25
 * @description Source code based on clsMarshrut.cls from GraphicPL@BASIC.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Converters;
using Messages;


namespace Core
{
    [Flags]
    public enum RouteState
    {
        /// <summary>
        /// Состояние по умолчанию. С маршрутом не производилось никаких операций.
        /// </summary>
        DEFAULT,

        /// <summary>
        /// Маршрут посещался (был просмотрен), то есть у данного маршрута есть осмотр/ремонт большего объёма и часть до осмотра/ремонта была учтена, осталось учесть часть после осмотра/ремонта.
        /// </summary>
        VISITED,

        /// <summary>
        /// Маршрут полностью просмотрен (обработан).
        /// </summary>
        CHECKED
    };


    /// <summary>
    /// Маршрут
    /// </summary>
    public class Route : IDisposable, ICloneable
    {
        /// <summary>
        /// Состояние, используемое при построении "цепочек" маршрутов для назначения осмотров (ТО-1).
        /// </summary>
        public RouteState State = RouteState.DEFAULT;

        /// <summary>
        /// Флаг, указывающий "приклеивается" ли этот маршрут к следующему при назначении осмотра (ТО-1).
        /// </summary>
        public Boolean isGluedToNext = false;

        /// <summary>
        /// Номер данного маршрута.
        /// </summary>
        public UInt32 number = 0;

        /// <summary>
        /// Коллекция (связный список) элементов графика оборота (ГО) для данного маршрута.
        /// </summary>
        public LinkedList<ElementOfMovementSchedule> ElementsOfMovementSchedule;

        /// <summary>
        /// Депо приписки данного маршрута.
        /// </summary>
        public Depot depot = null;

        /// <summary>
        /// Точка, в которую уходит ночевать данный маршрут.
        /// </summary>
        public NightStayPoint nightStayPoint = null;

        /// <summary>
        /// Маршрут, следующий за данным. То есть тот, в который перейдёт данный, после ухода-выхода на линию. 
        /// </summary>
        public Route nextRoute = null;

        /// <summary>
        /// Предыдущий для данного маршрута.
        /// </summary>
        public Route prevRoute = null;

        /// <summary>
        /// Варианты времени проведения осмотра
        /// </summary>
        public LinkedList<TimeVariant> Times = new LinkedList<TimeVariant>();  

        /// <summary>
        /// Коллекция ремонтов
        /// </summary>
        public List<Repair> Repairs = new List<Repair>();

        /// <summary>
        ///  Флаг невыхода из депо перед ремонтом (не выходит == true)
        /// </summary>
        public Boolean isNotOutFromDepotBeforeRepair;

        /// <summary>
        /// Физический состав (из вагонов), с которым отождествляется данный маршрут.
        /// </summary>
        public Train train = null;

        /// <summary>
        /// Минимальное, желаемое и максимально времена для ремонта состава.
        /// </summary>
        public TimeVariant time;

        public Route()
        {
        }

        public Route(DataRow row)
        {
            number = Convert.ToUInt32(row["Код"].ToString());

            isNotOutFromDepotBeforeRepair = Convert.ToBoolean(row["Признак_невыхода_из_депо_перед_ремонтом"].ToString());

            ElementsOfMovementSchedule = new LinkedList<ElementOfMovementSchedule>();

            time = new TimeVariant();
        }

        public void Initialize(DataRow row)
        {
            /*
             * Всё кроме "Ремонт" и "Точка_ночёвки" должно быть!!!
              <table name="Таблица маршрутов">
             *  <field name="Депо_привязки" type="int" isPK="false" null="true" />
             *  <field name="Код" type="int" isPK="true" null="true" />
             *  <field name="Признак_невыхода_из_депо_перед_ремонтом" type="bit" isPK="false" null="false" />
             *  <field name="Ремонт" type="int" isPK="false" null="true" />
             *  <field name="Следующий_маршрут" type="int" isPK="false" null="true" />
             *  <field name="Точка_ночевки" type="int" isPK="false" null="true" />
              </table>
            */

            var depotCode = Convert.ToUInt32(row["Депо_привязки"].ToString());
            depot = MovementSchedule.colDepot.SingleOrDefault(d => d.code == depotCode);

            if (depot == null)
                Error.showErrorMessage(new Depot {code = depotCode}, this);
            else
                depot.colOwnRoutes.Add(this);

            if (!Convert.IsDBNull(row["Ремонт"]))
            {
                var repairTypeCode = Convert.ToUInt32(row["Ремонт"].ToString());
                var repairType = MovementSchedule.colRepairType.SingleOrDefault(t => t.code == repairTypeCode);

                if (depot != null && repairType != null) // && depot.inspectionPoint != null)
                {
#if IS_REPAIR_COLLECTION
                    Logger.Output(repairType.ToString(), GetType().FullName);
                    var r = new Repair(repairType, this, depot.inspectionPoint, isContinue: false);
                    try
                    {
                        Logger.Output(
                            String.Format(
                                "Для ПТО №{2} попытка включения ремонта (маршрута №{0:D3}) {1} в коллекцию ремонтов.",
                                number, 
                                r.time,
                                r.inspectionPoint != null ? r.inspectionPoint.code.ToString("000") : "NULL"),
                            GetType().FullName);
                        r.addRepair(r.inspectionPoint);
                        Repairs.Add(r);
                    }
                    catch (Exception e)
                    {
                        Logger.Output(e.ToString(), GetType().FullName);
                    }
#else
                    // иначе NullPointerException
                    repair = new Repair(repairType, this, depot.inspectionPoint, isContinue: false);
                    try
                    {
                        Logger.output(
                            String.Format(
                                "Для ПТО №{2} попытка включения ремонта (маршрута №{0:D3}) {1} в коллекцию ремонтов.",
                                number,
                                repair.time,
                                repair.inspectionPoint != null ? repair.inspectionPoint.code.ToString("000") : "NULL"),
                            GetType().FullName);
                        repair.addRepair(repair.inspectionPoint);
                    }
                    catch (Exception e)
                    {
                        Logger.output(e.ToString(), GetType().FullName);
                    }
#endif


                }
                else
                {
                    // repair == null
                    Error.showErrorMessage(new Repair {type = repairType}, this,
                        String.Format("with typeCode = {0}", repairTypeCode));
#if IS_REPAIR_COLLECTION
                    var sb = new StringBuilder();
                    sb.AppendFormat("depot is {0};{1}", depot != null ? depot.name : "null", Environment.NewLine);
                    foreach(var r in Repairs)
                        sb.AppendFormat("repair is {0}{1}", r, Environment.NewLine);
                    Logger.Output(sb.ToString(), GetType().FullName);
#else
                    Logger.output(
                        String.Format("depot is {0}; repair is {1}", depot != null ? depot.name : "null",
                            repair != null ? repair.ToString() : "null"), GetType().FullName);
#endif

                }
            } // иначе repair == null

            var nextRouteNumber = Convert.ToUInt32(row["Следующий_маршрут"].ToString());
            nextRoute = MovementSchedule.colRoute.SingleOrDefault(r => r.number == nextRouteNumber);
            if (nextRoute != null)
                nextRoute.prevRoute = this;
            else
                Error.showErrorMessage(new Route {number = nextRouteNumber}, this);

            if (!Convert.IsDBNull(row["Точка_ночевки"]))
            {
                var pointCode = Convert.ToUInt32(row["Точка_ночевки"].ToString());
                nightStayPoint = MovementSchedule.colNightStayPoint.SingleOrDefault(p => p.code == pointCode);

                if (nightStayPoint == null)
                    Error.showErrorMessage(new NightStayPoint(pointCode), this);
            } /*
            else
            {
                Logger.output(
                    String.Format("Для маршрута №{0:D3} НЕ задана точка ночёвки (та, в которую уходим)", number),
                    GetType().FullName);
            }
            */
            /*
            try
            {
                var trainCode = Convert.ToUInt32(row["Состав"].ToString());
                train = mdlData.colTrain.SingleOrDefault(t => t.code == trainCode);
                if (train != null)
                    train.route = this;
                else
                    Error.showErrorMessage(new Train {code = trainCode}, this);
            }
            catch
            {
                Logger.output("Беда c составом", GetType().FullName);
            }
            */

        }

        /// <summary>
        /// Метод, создающий элемент графика оборота для данного маршрута.
        /// </summary>
        public void CreateMovementSchedule()
        {
            Logger.Output("in Route.CreateMovementSchedule()", GetType().FullName);
#if IS_REPAIR_COLLECTION
            if (Repairs.Count == 0)
            {
                new ElementOfMovementSchedule().Initialize(this, MovementSchedule.MovementTime.begin, MovementSchedule.MovementTime.end, prevRoute.nightStayPoint, nightStayPoint);
            }
            else
            {
                foreach (var r in Repairs)
                {
                    if (!r.isContinue)
                        r.getTimes();
                    
                    if (prevRoute.nightStayPoint == null || !isNotOutFromDepotBeforeRepair)
                    {
                        Logger.Output(r.ToString(), GetType().FullName);
                        new ElementOfMovementSchedule().Initialize(this, MovementSchedule.MovementTime.begin, r.beginTime, prevRoute.nightStayPoint, r.inspectionPoint.nightStayPoint_1);
                    }

                    if (r.endTime < MovementSchedule.MovementTime.end)
                    {
                        new ElementOfMovementSchedule().Initialize(this, r.endTime, MovementSchedule.MovementTime.end, r.inspectionPoint.nightStayPoint_1, nightStayPoint);
                    }
                }
            }
#else
            // Если маршрут работает без ремонтов (ремонт не назначен), то первоначально будет только один элемент графика оборота. Вышли на линию утром (время начала движения - 05:20) с ночёвки (где вчерашнюю ночь провели) -- ушли вечером (время конца движения - 02:00) на будущую ночёвку (где проведём сегодняшнюю ночь), всё больше никаких выходов-уходов нет.
            if (repair == null)
            {
                new ElementOfMovementSchedule().Initialize(this, mdlData.timeStartMovement, mdlData.timeFinishMovement,
                    prevRoute.nightStayPoint, nightStayPoint);
            }
            else
            {
                // Если маршрут работает с ремонтами в депо, то первоначально может быть один или два элемента графика оборота- это зависит от места ночёвки перед ремонтом. Определяем время ухода на ремонт и выхода из него, если это не продолжение уже начатого ремонта.
                if (!repair.isContinue)
                    repair.getTimes();

                /* Если состав ночевал на линии или в депо и должен перед ремонтом выходить на линию, то он выйдет на линию дважды за эти сутки. То есть сначала с утра, потом уйдёт на ремонт, потом выйдет из ремонта, потом уйдёт на ночёвку. Иными словами - (начало движения, начало ремонта) и (конец ремонта, конец движения). 
                 * prevRoute.nightStayPoint == null -- ночевал на линии
                 * !isNotOutFromDepotBeforeRepair -- должен выходить из депо перед ремонтом (а значит, он точно ночевал в депо)
                 * Таким образом или на линии или в депо и при этом должен выйти на линию до ремонта. */
                if (prevRoute.nightStayPoint == null || !isNotOutFromDepotBeforeRepair)
                {
                    // Создаём анонимный объект класса и вызываем на нём метод. Параметры метода -- маршрут, время начала движения (элемента оборота), время конца движения (элемента оборота), место начала движения (элемента оборота), место конца движения (элемента обората). Иными словами кто, когда вышел на линию, когда ушёл, откуда вышел, куда ушёл.
                    new ElementOfMovementSchedule().Initialize(this, mdlData.timeStartMovement, repair.beginTime,
                        prevRoute.nightStayPoint, repair.inspectionPoint.nightStayPoint_1);
                }

                // Состав не выйдет на линию, если ремонт не закончен до окончания движения. Иными словами оборот для данного маршрута будет добавлен, только если ремонт закончится раньше конца движения. */
                if (repair.endTime < mdlData.timeFinishMovement)
                {
                    // Создаём анонимный объект класса и вызываем на нём метод. Параметры метода -- маршрут, время начала движения (элемента оборота), время конца движения (элемента оборота), место начала движения (элемента оборота), место конца движения (элемента обората). Иными словами кто, когда вышел на линию, когда ушёл, откуда вышел, куда ушёл.
                    new ElementOfMovementSchedule().Initialize(this, repair.endTime, mdlData.timeFinishMovement,
                        repair.inspectionPoint.nightStayPoint_1, nightStayPoint);
                }
            }
#endif
            // Если состав ночевал в депо и не должен перед ремонтом выходить на линию, то вносим его в список составов, находящихся в депо.
            if (prevRoute.nightStayPoint != null && isNotOutFromDepotBeforeRepair)
                depot.colRoutes.Add(this);


            // Корректируем список доступных составов.
       //     CorrectTrainList();




            /*
            if (Repairs.Count == 0)
            //if (repair == null)
            {
                // Создаём анонимный объект класса и вызываем на нём метод. Параметры метода -- маршрут, время начала движения (элемента оборота), время конца движения (элемента оборота), место начала движения (элемента оборота), место конца движения (элемента обората). Иными словами кто, когда вышел на линию, когда ушёл, откуда вышел, куда ушёл.
                // @NOTE: Может возникнуть проблема prevRoute == null 
                new ElementOfMovementSchedule().Initialize(this, mdlData.timeStartMovement, mdlData.timeFinishMovement,
                    prevRoute.nightStayPoint, nightStayPoint);
            }
            else
            {
                foreach (var r in Repairs)
                {
                    if (r.isContinue)
                        r.getTimes();
                    
                    if (prevRoute.nightStayPoint == null || !isNotOutFromDepotBeforeRepair)
                    {
                        new ElementOfMovementSchedule().Initialize(this, mdlData.timeStartMovement, r.beginTime, prevRoute.nightStayPoint, r.inspectionPoint.nightStayPoint_1);
                    }

                    if (r.endTime < mdlData.timeFinishMovement)
                    {
                        new ElementOfMovementSchedule().Initialize(this, r.endTime, mdlData.timeFinishMovement,
                            r.inspectionPoint.nightStayPoint_1, nightStayPoint);
                    }
                }

                // Если маршрут работает с ремонтами в депо, то первоначально может быть один или два элемента графика оборота- это зависит от места ночёвки перед ремонтом. Определяем время ухода на ремонт и выхода из него, если это не продолжение уже начатого ремонта. 
                
                //if (!repair.isContinue)
                //    repair.getTimes();

                // Если состав ночевал на линии или в депо и должен перед ремонтом выходить на линию, то он выйдет на линию дважды за эти сутки. То есть сначала с утра, потом уйдёт на ремонт, потом выйдет из ремонта, потом уйдёт на ночёвку. Иными словами - (начало движения, начало ремонта) и (конец ремонта, конец движения). 
                // prevRoute.nightStayPoint == null -- ночевал на линии
                // !isNotOutFromDepotBeforeRepair -- должен выходить из депо перед ремонтом (а значит, он точно ночевал в депо)
                // Таким образом или на линии или в депо и при этом должен выйти на линию до ремонта.
                if (prevRoute.nightStayPoint == null || !isNotOutFromDepotBeforeRepair)
                {
                    // Создаём анонимный объект класса и вызываем на нём метод. Параметры метода -- маршрут, время начала движения (элемента оборота), время конца движения (элемента оборота), место начала движения (элемента оборота), место конца движения (элемента обората). Иными словами кто, когда вышел на линию, когда ушёл, откуда вышел, куда ушёл.
                     
                    new ElementOfMovementSchedule().Initialize(this, mdlData.timeStartMovement, repair.beginTime,
                        prevRoute.nightStayPoint, repair.inspectionPoint.nightStayPoint_1);
                }

                // Состав не выйдет на линию, если ремонт не закончен до окончания движения. Иными словами оборот для данного маршрута будет добавлен, только если ремонт закончится раньше конца движения. 
                if (repair.endTime < mdlData.timeFinishMovement)
                {
                    // Создаём анонимный объект класса и вызываем на нём метод. Параметры метода -- маршрут, время начала движения (элемента оборота), время конца движения (элемента оборота), место начала движения (элемента оборота), место конца движения (элемента обората). Иными словами кто, когда вышел на линию, когда ушёл, откуда вышел, куда ушёл.
                    new ElementOfMovementSchedule().Initialize(this, repair.endTime, mdlData.timeFinishMovement,
                        repair.inspectionPoint.nightStayPoint_1, nightStayPoint);
                }
            }

            // Если состав ночевал в депо и не должен перед ремонтом выходить на линию, то вносим его в список составов, находящихся в депо.
            if (prevRoute.nightStayPoint != null && isNotOutFromDepotBeforeRepair)
                depot.colRoutes.Add(this);


            // Корректируем список доступных составов.
            CorrectTrainList();
             */
        }

        /// <summary>
        /// Корректируем список доступных составов. В коде на VB6 == Public Sub CorrectMDostup()
        /// </summary>
   //     public void CorrectTrainList()
   //     {
   //         Logger.Output("in Route.correctTrainList()", GetType().FullName);

            /* Удаляем маршрут из списка доступных по всем размерам движения. */
    //        foreach (var razmer in MovementSchedule.masRazmerDvigeniya)
    //        {
     //           var route = razmer.colRoute.SingleOrDefault(r => r.number == number);
     //           if (route != null)
    //                razmer.colRoute.Remove(route);
     //       }

            /* Для каждого размера движения проверяем, есть ли элемент графика оборота, входящий в этот размер. */
     //       foreach (var razmer in MovementSchedule.masRazmerDvigeniya)
     //       {
    //            foreach (var element in MovementSchedule.colElementsOfRepairs)
    //            {
                    /* Если время выхода элемента графика оборота лежит между временем начала и временем конца данного элемента массива размеров движения ... */
    //                if (razmer.beginTime < element.beginTime && element.beginTime < razmer.endTime
                        /* ... или если  время захода элемента графика оборота лежит между временем начала и временем конца данного элемента массива размеров двидения ... */
     //                   || (element.endTime > razmer.beginTime)
    //                    && (element.endTime < razmer.endTime)
                        /* ... или если время выхода элемента графика оборота меньше времени начала данного элемента массива размеров движения и время захода элемента графика оборота больше времени конца данного элемента массива размеров движения, то ... */
     //                   || (element.beginTime < razmer.beginTime)
     //                   && (element.endTime > razmer.endTime)
     //                   )
     //               {
      //                  var route = razmer.colRoute.SingleOrDefault(r => r.number == number);
     //                   if (route == null)
      //                      razmer.colRoute.Add(this);
     //               }
     //           }
     //       }

     //       Logger.Output("out Route.correctTrainList()", GetType().FullName);
    //    }

        public Boolean addRepair(Repair repair)
        {
            var breakElement =
                ElementsOfMovementSchedule.SingleOrDefault(
                    el => el.beginTime <= repair.beginTime && repair.endTime <= el.endTime);
            if (breakElement == null)
                return false;

            new ElementOfMovementSchedule().Initialize(this, repair.endTime, breakElement.endTime, null,
                breakElement.endPoint);

            breakElement.endTime = repair.beginTime;
            breakElement.endPoint = null;   //repair.inspectionPoint;

            /*
            var cloneElement = breakElement.Clone() as ElementOfMovementSchedule;
            if (cloneElement == null)
                return false;
            cloneElement.beginTime = repair.endTime;
            cloneElement.beginPoint = null; // repair.inspectionPoint;

            breakElement.endTime = repair.beginTime;
            breakElement.endPoint = null;   //repair.inspectionPoint;

            AddElementOfMovementSchedule(cloneElement);
             */

            Repairs.Add(repair);

            return true;
        }

        public override int GetHashCode()
        {
            return (int)number;
        }

        public override bool Equals(object obj)
        {
            var other = obj as Route;
            
            if (other == null)
                return false;

            return number == other.number;
        }

        /// <summary>
        /// Метод подсчёта времени прошедшего, либо от начала движения данного маршрута до осмотра/осмотра (флаг -- истина), либо от момента осмотра/ремонта до конца движения данного маршрута (флаг -- ложь).
        /// </summary>
        /// <param name="beforeRepair">Флаг, указывающий какой именно промежуток (до или после осмотра/ремонта) учитывать.</param>
        /// <returns>Время в "метрополитеновских" секундах.</returns>
        public Int32 TimeWithoutRepair(Boolean beforeRepair = true)
        {
            return beforeRepair
                ? ElementsOfMovementSchedule.TakeWhile(el => el.beforeRepair).Sum(el => el.duration())
                : ElementsOfMovementSchedule.SkipWhile(el => el.beforeRepair).Sum(el => el.duration())
                ;
        }

        public void AddElementOfMovementSchedule(ElementOfMovementSchedule element)
        {
            var isElementInserted = false;
            var current = ElementsOfMovementSchedule.First; // если список пуст, то current == null
            while (current != null && !isElementInserted)
            {
                if (current.Value.beginTime > element.beginTime)
                {
                    ElementsOfMovementSchedule.AddBefore(current, element);
                    Logger.Output(
                        String.Format("Эл-т ГО №{0} добавлен перед эт-том ГО №{1} для маршрута №{2:D3}", element.code,
                            current.Value.code, number), GetType().FullName);
                    isElementInserted = true;
                }
                current = current.Next;
            }
            if (!isElementInserted)
            {
                ElementsOfMovementSchedule.AddLast(element);
                Logger.Output(
                    String.Format("Эл-т ГО №{0} добавлен последним для маршрута №{1:D3}", element.code, number),
                    GetType().FullName);
            }

            if (Repairs.Any(r => r.endTime <= element.beginTime))
                element.beforeRepair = false;
        }


#if IS_REPAIR_COLLECTION
        public String GetRepairsInfo(String separator)
        {
            var sb = new StringBuilder("Ремонты:" + separator);
            foreach (var r in Repairs)
                 sb.AppendFormat(" {0} с {1} по {2}{3}",
                    r.inspectionPoint != null ? "на осмотре в " + r.inspectionPoint.name : @"/ПТО == null/",
                    r.beginTime, r.endTime, separator);
            sb.Remove(sb.Length - separator.Length, separator.Length);
            return sb.ToString();
        }
#endif
        public override String ToString()
        {
            return String.Format("Маршрут №{0:D3} выход: [{1}] {2} --> Маршрут №{3}", number,
                isNotOutFromDepotBeforeRepair ? " " : "X", Repairs.Any() ? "X" : " ",
                nextRoute != null ? nextRoute.number.ToString("000") : "XXX");
        }

        public String GetWindows()
        {
            var sb = new StringBuilder();
            foreach (var timeVariant in Times)
                sb.AppendLine(timeVariant.ToString());
            return sb.ToString();
        }

        public object Clone()
        {
            var newRoute = new Route()
            {
                nextRoute = this.nextRoute, 
                prevRoute = this.prevRoute, 
                Times = new LinkedList<TimeVariant>()
             };

            foreach (var localTime in Times)
                newRoute.Times.AddLast(localTime);   

            return newRoute;
        }

        public void Dispose()
        {
#if LOGER_ON
            Logger.Output(String.Format("Маршрут №{0:D3} удалён (объект был разрушен).", number), GetType().FullName);
#endif
        }

        public String PrintToHtml()
        {
            var sb = new StringBuilder();
            foreach (var element in ElementsOfMovementSchedule)
            {
                sb.AppendFormat("<tr bgcolor='{5}'><td>{0:D3}</td><td>{1}</td>{4}<td>{2}</td><td>{3:D3}</td></tr>", 
                    number,
                    prevRoute.nightStayPoint != null ? prevRoute.nightStayPoint.name : "НЕ ЗАДАНО",
                    nightStayPoint != null ? nightStayPoint.name : "НЕ ЗАДАНО", 
                    nextRoute.number, 
                    element,
                    ElementsOfMovementSchedule.Count > 1 ? "#eee" : "#fff");
            }
            return sb.ToString();
        }
    }
}
