/** 
 * @organization Departament "Management and Informatics in Technical Systems" (MITS@MIIT)
 * @author Konstantin Filipchenko (konstantin-649@mail.ru)
 * @version 0.1 
 * @date 2013-03-25
 * @description Source code based on clsElementGrOborota.cls from GraphicPL@BASIC.
 */

using System;
using System.Linq;
using System.Data;

using Messages;
using Converters;

namespace Core
{
    /// <summary>
    /// Элемент графика оборота подвижного состава
    /// </summary>
    public class ElementOfMovementSchedule
    {
        /// <summary>
        /// Код элемента графика оборота в базе данных
        /// </summary>
        public UInt32 code;

        /// <summary>
        /// Статическое поле, позволяющее назначать код, для новосозданного элемента ГО.
        /// </summary>
        private static UInt32 maxCode;

        /// <summary>
        /// Маршрут, к которому относится элемент графика оборота
        /// </summary>
        public Route route;

        /// <summary>
        /// Время выхода на линию из ремонта/осмотра (левая граница, начало элемента). Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Int32 beginTime;

        /// <summary>
        /// Место, из которого выходим на линию из ремонта/осмотра.
        /// </summary>
        public NightStayPoint beginPoint;

        /// <summary>
        /// Время захода с линиии в ремонт/осмотр (правая граница, конец элемента). Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Int32 endTime;

        /// <summary>
        /// Место, из которого с линии заходим в ремонт/осмотр/депо. То есть уходим с линии или заканчиваем элемент оборота. 
        /// </summary>
        public NightStayPoint endPoint;

        /// <summary>
        /// Состояние нитки графика (откуда выходим -- из депо, из ночного отстоя и т.п.)
        /// </summary>
        private ThreadState beginFlag;

        /// <summary>
        /// Состояние нитки графика (куда уходим -- в депо, в ночной отстой и т.п.)
        /// </summary>
        private ThreadState endFlag;

        /// <summary>
        /// Флаг, показывающий исполняется ли данный элемент ГО до проведения ремонта/осмотра на линии для данного маршрута или нет. Предназначен для удобства учёта времени проведённого составом на линии (в работе) без осмотра/ремонта (между осмотрами/ремонтами).
        /// </summary>
        public Boolean beforeRepair = true;


        private void setCode()
        {
            if (code == 0)
            {
                if (maxCode == 0)
                {
                    try
                    {
                        maxCode = MovementSchedule.colElementOfSchedule.Max(el => el.code);
                    }
                    catch (Exception exception)
                    {
                        Logger.Output(exception.ToString(), GetType().FullName);
                    }
                }
                code = (++maxCode);
            }
        }

        public ElementOfMovementSchedule()
        {
            setCode();
            //setBeginTime();
        }

        public ElementOfMovementSchedule(DataRow row)
        {
            code = Convert.ToUInt32(row["Код"].ToString());

            beginTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_выхода"].ToString()));

            endTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_захода"].ToString()));

            Logger.Output(
                String.Format("Эл-т ГО №{0} - выход на линию в {1}, уход с линии в {2}",
                    code,
                    TimeConverter.SecondsToString(beginTime), TimeConverter.SecondsToString(endTime)), GetType().FullName);

            /*
              beginFlag = ; // "Признак_Начала_Нитки"
             
              endFlag = ;   // "Признак_Конца_Нитки"
            */
        }



        /*
          <table name="График оборота: элементы">
         * 
         * Всё должно быть!!!
        ?*  <field name="Время_выхода" type="datetime" isPK="false" null="true" />
        ?*  <field name="Время_захода" type="datetime" isPK="false" null="true" />
         *  <field name="Номер_маршутра" type="int" isPK="false" null="false" />
         *  <field name="Точка_выхода" type="int" isPK="false" null="false" />
         *  <field name="Точка_захода" type="int" isPK="false" null="true" />
          </table>
        */

        /// <summary>
        /// Инициализация данного элемента графика оборота посредством строки из таблицы базы данных.
        /// </summary>
        /// <param name="row">Строка виртуальной таблицы базы данных.</param>
        public void Initialize(DataRow row)
        {
            setCode();

            Logger.Output(String.Format("Эл-т ГО №{0} - выход на линию в {1}, уход с линии в {2} [init]", code, TimeConverter.SecondsToString(beginTime), TimeConverter.SecondsToString(endTime)), GetType().FullName);

            var routeCode = Convert.ToUInt32(row["Номер_маршутра"].ToString());
            route = MovementSchedule.colRoute.SingleOrDefault(r => r.number == routeCode);

            if (route != null)
                route.AddElementOfMovementSchedule(this);
            else
                Error.showErrorMessage(new Route { number = routeCode }, this);

            var pointCode = Convert.ToUInt32(row["Точка_захода"].ToString());
            endPoint = MovementSchedule.colNightStayPoint.SingleOrDefault(p => p.code == pointCode);

            if (endPoint == null)
                Error.showErrorMessage(new NightStayPoint(pointCode), this);

            pointCode = Convert.ToUInt32(row["Точка_выхода"].ToString());
            beginPoint = MovementSchedule.colNightStayPoint.SingleOrDefault(p => p.code == pointCode);

            if (beginPoint == null)
                Error.showErrorMessage(new NightStayPoint(pointCode), this);
        }

        /// <summary>
        /// Сохраняем в базу данных информацию о данном элементе графика оборота.
        /// </summary>
        /// <param name="row">Строка виртуальной таблицы базы данных, в которую будет помещена запись о данном элементе графика оборота.</param>
        public void SaveToDatabase(ref DataRow row)
        {
            row.SetField("Номер_маршрута", route.number);
            row.SetField("Точка_захода", endPoint.code);
            row.SetField("Точка_выхода", beginPoint.code);
            row.SetField("Время_захода", TimeConverter.SecondsToTime(endTime));
            row.SetField("Время_выхода", TimeConverter.SecondsToTime(beginTime));
        }


        /// <summary>
        /// Длительность данного элемента (движение от осмотра/ремонта до осмотра/ремонта). 
        /// </summary>
        /// <returns>Колличество "метрополитеновсикх" секунд (число временнЫх интервалов).</returns>
        public Int32 duration()
        {
            //   Dlit = TimeZahod - TimeVyhod
            return endTime - beginTime;
        }

        /// <summary>
        /// Метод, инициализирующий для некоторого маршрута элемент его графика оборота, с учётом уже существующих элементов графика оборота, относящихся к этому маршруту. Элементы сортируются по времени начала оборота (движения по линии). Выставляется флаг, описывающий состояние начала и конца "нитки графика", связанной с этим маршрутом.
        /// </summary>
        /// <param name="route">Маршрут, для которого инициируется элемент оборота (движения по линии). Значение null не допустимо.</param>
        /// <param name="beginTime">Время начала оборота (движения по линии) - колличество "метрополитеновсикх" секунд (число временнЫх интервалов).</param>
        /// <param name="endTime">Время конца оборота (движения по линии) -  колличество "метрополитеновсикх" секунд (число временнЫх интервалов).</param>
        /// <param name="beginPoint">Точка, из которой выходим на оборот (на линию из депо/осмотра/ремонта). Допустимо значение null.</param>
        /// <param name="endPoint">Точка, в которую уходим с оборота (с линии в депо/на осмотр/ремонт). Допустимо значение null.</param>
        public void Initialize(Route route, Int32 beginTime, Int32 endTime, NightStayPoint beginPoint, NightStayPoint endPoint)
        {
            Logger.Output("In Initialize()", GetType().FullName);

            if (route == null)
                throw new ArgumentNullException("route");

            if (route.ElementsOfMovementSchedule == null)
                throw new NullReferenceException("Коллекция route.ElementsOfMovementSchedule не была создана.");


            /* Маршрут уходит... */
            if (endPoint != null)
            {
                this.endPoint = endPoint;
                /* Если есть депо, то в депо, иначе - в ремонт (отстой). */
                endFlag = (endPoint.depot != null) ? ThreadState.TO_DEPOT : ThreadState.REPAIR;
            }
            else
            {
                // ... в ночную расстановку
                endFlag = ThreadState.TO_NIGHTSTAY;
            }

            /* Маршрут выходит... */
            if (beginPoint != null)
            {
                this.beginPoint = beginPoint;
                /* Если есть депо, то из депо, иначе - из ремонта (отстоя). */
                beginFlag = (beginPoint.depot != null) ? ThreadState.FROM_DEPOT : ThreadState.REPAIR;
            }
            else
            {
                // ... из ночной расстановки
                beginFlag = ThreadState.FROM_NIGHTSTAY;
            }

            //if (route.repair != null && route.repair.endTime <= beginTime)
            //    beforeRepair = false;

            this.beginTime = beginTime;
            this.endTime = endTime;
            this.route = route;
            this.route.AddElementOfMovementSchedule(this);

            Logger.Output(String.Format("Эл-т ГО №{0} ({3} [{1} - {2}] {4})", code, TimeConverter.SecondsToString(this.beginTime), TimeConverter.SecondsToString(this.endTime), beginFlag, endFlag), GetType().FullName);
        }

        public override string ToString()
        {
            return String.Format("<td>{0}</td><td>[{2} -- {3}]</td><td>{1}</td>", beginFlag, endFlag, TimeConverter.SecondsToString(beginTime), TimeConverter.SecondsToString(endTime));
        }
    }
}