using System;
using System.Collections.Generic;
using System.Data;
using Converters;
using Messages;
using Core;
using System.Windows;


namespace Schedule
{
    // TODO: Make class Scedule as Singleton!!!

    //  Шаблоны проектирования. Новый подход к объектно-ориентированному анализу и проектированию

    /// <summary>
    /// Параметры ПГД
    /// </summary>
    class Schedule
    {
        // Коллекции основных классов -----------------------------------------

        /// <summary>
        /// Коллекция надписей на плановом графике движения (ПГД). 
        /// </summary>
        public static List<Legend> Legends; 

        /// <summary>
        /// Коллекция линий, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<Line> Lines;

        /// <summary>
        /// Коллекция станций принадлежащих линиям, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<Station> Stations;

        // Коллекции необходимые для построения ГО (в том числе) --------------

        /// <summary>
        /// Коллекция депо, обслуживающих линии, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<Depot> Depots; 

        /// <summary>
        /// Коллекция маршрутов работающих на линиях, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<Route> Routes;

        /// <summary>
        /// Коллекция осмотров/ремонтов маршрутов работающих на линиях, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<Repair> Repairs;

        /// <summary>
        /// Коллекция типов осмотров/ремонтов маршрутов работающих на линиях, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<RepairType> RepairTypes;

        /// <summary>
        /// Коллекция пунктов технического обслуживания (ПТО, причём как линейных, так и тех, что в депо), обслуживающих составы, работающие на линиях, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<InspectionPoint> InspectionPoints;

        /// <summary>
        /// Коллекция точек ночной расстановки (расположены как на линии, так и в депо), в которых "ночуют" составы, работающие на линиях, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<NightStayPoint> NightStayPoints;

        /// <summary>
        /// Коллекция элементов ГО для маршрутов, работающих на линиях, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<ElementOfMovementSchedule> ElementsOfMovementSchedule; 

        // Коллекции необходимые для построения ГО (в том числе) --------------

        // Магия и классы на рефакторинг --------------------------------------

        /// <summary>
        /// Коллекция направлений.
        /// </summary>
        public static List<Direction> Directions;

        /// <summary>
        /// Коллекция заданий (перегонов?).
        /// </summary>
        public static List<Task> Tasks;

        /// <summary>
        /// 
        /// </summary>
        public static List<clsElementOfSchedule> ElementsOfSchedule;

        /// <summary>
        /// 
        /// </summary>
        public static List<clsRazmerDvizheniya> RazmeryDvizheniya; 

        // ...

        // Магия и классы на рефакторинг --------------------------------------

        // Задел на будущее ---------------------------------------------------

        /// <summary>
        /// Коллекция составов, работающих на линиях, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<Train> Trains;

        /// <summary>
        /// Коллекция вагонов составов, работающих на линиях, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<Wagon> Wagons;

        /// <summary>
        /// Коллекция типов электроподвижного состава (ЭПС), работающего на линиях, для которых строятся графики (ПГД, ГО). 
        /// </summary>
        public static List<WagonType> WagonTypes;

        // Задел на будущее ---------------------------------------------------

        // Коллекции основных классов -----------------------------------------

        /// <summary>
        /// Время начала и конца оборота подвжиного состава
        /// </summary>
        public TimeInterval Movement;

        /// <summary>
        /// Сезон
        /// </summary>
        private Season _season;

        /// <summary>
        /// Метод, возвращающий сезон.
        /// </summary>
        /// <returns>Строковое представление названия сезона для текущего графика.</returns>
        public String GetSeason()
        {
            var result = String.Empty;
            switch (_season)
            {
                case Season.SUMMER:
                    result = "Лето";
                    break;
                case Season.WINTER:
                    result = "Зима";
                    break;
            }
            return result;
        }

        /// <summary>
        /// Дата
        /// </summary>
        private DateTime _date;

        /// <summary>
        /// Метод, возвращающий день и месяц.
        /// </summary>
        /// <returns>Строковое представление дня и месяца для текущего грфика.</returns>
        public String GetDate()
        {
            return _date.ToString("d MMMM");
        }

        /// <summary>
        /// Метод, возвращающий последние две цифры года.
        /// </summary>
        /// <returns>Строковое представление последних двух цифр года для текущего графика.</returns>
        public String GetYear()
        {
            return _date.ToString("yy");
        }

        /// <summary>
        /// Тип линии (обычная, кольцо или монорельс).
        /// </summary>
        public LineType LineType;

        /// <summary>
        /// Тип графика (на рабочие или выходные дни).
        /// </summary>
        private ScheduleDayType _scheduleType;

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String GetScheduleType()
        {
            var result = String.Empty;
            switch (_scheduleType)
            {
                case ScheduleDayType.WORK:
                    result = "Рабочие";
                    break;
                case ScheduleDayType.WEEKEND:
                    result = "Выходные";
                    break;
            }
            return result;
        }

        public void Init(DataRow row)
        {
            if (!Convert.IsDBNull(row["Время_начала_оборота"]) && !Convert.IsDBNull(row["Время_конца_оборота"]))
            {
                // TODO: try-catch section
                // TODO: найти метод реализующий DateTimeToSeconds!!!
                var start = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_начала_оборота"].ToString()));
                var finish = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_конца_оборота"].ToString()));
                Movement = new TimeInterval(start, finish);
            }

            if (!Convert.IsDBNull(row["Сезон"]))
            {
                switch (row["Сезон"].ToString().ToUpperInvariant())
                {
                    case "ЛЕТО":
                        _season = Season.SUMMER;
                        break;
                    case "ЗИМА":
                        _season = Season.WINTER;
                        break;
                    default:
                        _season = Season.NONE;
                        break;
                }
            }

            if (!Convert.IsDBNull(row["Число"]) && !Convert.IsDBNull("Год"))
            {
                _date = Convert.ToDateTime(String.Format("{0} 20{1}", row["Число"], row["Год"]));
            }

            // TODO: Изменить проверку сезона по месяцу, убрать хардкод!!!
            var dateSeason = _date.Month > 2 && _date.Month < 9 ? Season.SUMMER : Season.WINTER;
            if (_season != dateSeason)
            {
                var message = "Несовпадение сезонов!";
                var source = GetType().FullName;
                MessageBox.Show(message, source, MessageBoxButton.OK, MessageBoxImage.Error);
#if LOGER_ON
                Logger.Output(message, source);
#endif
            }

            if (!Convert.IsDBNull(row["Тип линии"]))
            {
                switch (Convert.ToInt32(row["Тип линии"].ToString()))
                {
                    case 1: LineType = LineType.MONO;
                        break;
                    case 2: LineType = LineType.RING;
                        break;
                    default: LineType = LineType.DEFAULT;
                        break;
                }   
            }

            if (!Convert.IsDBNull(row["Дни"]))
            {
                switch (row["Дни"].ToString().ToUpperInvariant())
                {
                    case "РАБОЧИЕ": 
                        _scheduleType = ScheduleDayType.WORK;
                        break;
                    case "ВЫХОДНЫЕ": 
                        _scheduleType = ScheduleDayType.WEEKEND;
                        break;
                    default: 
                        _scheduleType = ScheduleDayType.NONE;
                        break;
                }
            }
        }
    }
}
