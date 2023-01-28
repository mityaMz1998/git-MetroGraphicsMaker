/** 
 * @organization Departament "Management and Informatics in Technical Systems" (MITS@MIIT)
 * @author Konstantin Filipchenko (konstantin-649@mail.ru)
 * @version 0.1 
 * @date 2013-03-25
 * @description Source code based on clsPointNight.cls from GraphicPL@BASIC.
 */

using System;
using System.Linq;
using System.Data;
using Messages;
using Converters;

namespace Core
{
    /// <summary>
    /// Точка ночной расстановки
    /// </summary>
    public class NightStayPoint
    {
        /// <summary>
        /// Код данной точки ночной расстановки.
        /// </summary>
        public UInt32 code;

        /// <summary>
        /// Название данной точки ночной расстановки.
        /// </summary>
        public String name;

        /// <summary>
        /// Станция, которой принадлежит данная точка ночной расстановки.
        /// </summary>
        public Station station;

        /// <summary>
        /// Перегон, на котором находится данная точка ночной расстановки.
        /// </summary>
        public Task task = null;

        // 'Пункт осмотра
        public InspectionPoint inspectionPoint;

        // 'Точка, парная текущей по принадлежности пункту осмотра
        public NightStayPoint pairPoint;

        /// <summary>
        /// Депо, в котором находится данная точка ночной расстановки.
        /// </summary>
        public Depot depot = null;


        /// <summary>
        /// Линия, на которой находится данная точка ночной расстановки.
        /// </summary>
        public Line line;

        /// <summary>
        /// Ёмкость точки.
        /// </summary>
        public Int32 Capacity;

        /// <summary>
        /// Пикетаж. 
        /// </summary>
        public Double Picket;

        /// <summary>
        /// Координата утро.
        /// </summary>
        public Int32 MorningCoordinate;

        /// <summary>
        /// Координата вечер.
        /// </summary>
        public Int32 EveningCoordinate;

        /// <summary>
        /// Путь.
        /// </summary>
        public Int32 Track;

        /// <summary>
        /// Резервная.
        /// </summary>
        public Boolean flgReserve;

        /// <summary>
        /// Время движения до выхода на линию
        /// </summary>
        public Int32 TimeToExitOnLine; //переделать, учитывая тип dataTime

        /// <summary>
        /// Принадлежность расстановке.
        /// </summary>
        public Int32 Arrangement;




        /// <summary>
        /// Начало выпуска из точки ночного отстоя четное
        /// </summary>
        public Int32 StartEven; //переделать, учитывая тип dataTime

        /// <summary>
        /// Начало выпуска из точки ночного отстоя нечетное
        /// </summary>
        public Int32 StartUneven; //переделать, учитывая тип dataTime

        /// <summary>
        /// Конец выпуска из точки четное
        /// </summary>
        public Int32 EndEven; //переделать, учитывая тип dataTime

        /// <summary>
        /// Конец выпуска из точки нечетное
        /// </summary>
        public Int32 EndUneven; //переделать, учитывая тип dataTime

        /// <summary>
        /// Направление выхода четное
        /// </summary>
        public Int32 ExitDirectionEven;

        /// <summary>
        /// Направление выхода нечетное
        /// </summary>
        public Int32 ExitDirectionUneven;

        /// <summary>
        /// Выйти до первого пассажирского поезда четное
        /// </summary>
        public Boolean flgGoBeforeFirstTrainEven;

        /// <summary>
        /// Выйти до первого пассажирского поезда нечетное
        /// </summary>
        public Boolean flgGoBeforeFirstTrainUneven;

        /// <summary>
        /// Зайти после последнего пассажирского поезда четное
        /// </summary>
        public Boolean flgGoAfterLastTrainEven;

        /// <summary>
        /// Зайти после последнего пассажирского поезда нечетное
        /// </summary>
        public Boolean flgGoAfterLastTrainUneven;



        /*
          <table name="Точки ночного отстоя">
        ?   <field name="Время_движения_до_выхода_на_линию" type="datetime" isPK="false" null="true" />
            <field name="Выйти_до_первого_пассажирского_нечетное" type="bit" isPK="false" null="false" />
            <field name="Выйти_до_первого_пассажирского_четное" type="bit" isPK="false" null="false" />
         ?  <field name="Депо" type="int" isPK="false" null="true" />
         *  <field name="Емкость_точки" type="int" isPK="false" null="true" />
            <field name="Зайти_после_последнего_пассажирского_нечетное" type="bit" isPK="false" null="false" />
            <field name="Зайти_после_последнего_пассажирского_четное" type="bit" isPK="false" null="false" />
         *  <field name="Код" type="int" isPK="true" null="true" />
        ?   <field name="Конец_выпуска_из точки_нечетное" type="datetime" isPK="false" null="true" />
        ?   <field name="Конец_выпуска_из точки_четное" type="datetime" isPK="false" null="true" />
         *  <field name="Координата_вечер" type="int" isPK="false" null="false" />
         *  <field name="Координата_утро" type="int" isPK="false" null="false" />
         *  <field name="Линия" type="int" isPK="false" null="true" />
         *  <field name="Название" type="text" isPK="false" null="false" />
            <field name="Направление_выхода_нечетное" type="int" isPK="false" null="true" />
            <field name="Направление_выхода_четное" type="int" isPK="false" null="true" />
        ?   <field name="Начало_выпуска_из точки_нечетное" type="datetime" isPK="false" null="true" />
        ?   <field name="Начало_выпуска_из точки_четное" type="datetime" isPK="false" null="true" />
         *  <field name="Перегон" type="int" isPK="false" null="true" />
         *  <field name="Пикетаж" type="float" isPK="false" null="true" />
            <field name="Предыдущая_точка_нечетная" type="int" isPK="false" null="true" />
            <field name="Предыдущая_точка_четная" type="int" isPK="false" null="true" />
         ?  <field name="Принадлежность_депо" type="int" isPK="false" null="true" />
         *  <field name="Принадлежность_расстановке" type="int" isPK="false" null="true" />
         *  <field name="Путь" type="int" isPK="false" null="true" />
         *  <field name="Резервная" type="bit" isPK="false" null="false" />
         *  <field name="Станция" type="int" isPK="false" null="false" />
          </table>
         */

        public NightStayPoint(UInt32 id)
        {
            code = id;
        }

        public NightStayPoint(DataRow row)
        {
            code = Convert.ToUInt32(row["Код"].ToString());

            name = row["Название"].ToString();

            if (!Convert.IsDBNull(row["Емкость_точки"]))
                Capacity = Convert.ToInt32(row["Емкость_точки"].ToString());

            if (!Convert.IsDBNull(row["Пикетаж"]))
                Picket = Convert.ToDouble(row["Пикетаж"].ToString());

            if (!Convert.IsDBNull(row["Путь"]))
                Track = Convert.ToInt32(row["Путь"].ToString());

            if (!Convert.IsDBNull(row["Время_движения_до_выхода_на_линию"]))
                TimeToExitOnLine = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_движения_до_выхода_на_линию"].ToString()));

            if (!Convert.IsDBNull(row["Принадлежность_расстановке"]))
                Arrangement = Convert.ToInt32(row["Принадлежность_расстановке"].ToString());



            if (!Convert.IsDBNull(row["Начало_выпуска_из точки_четное"]))
                StartEven = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Начало_выпуска_из точки_четное"].ToString()));

            if (!Convert.IsDBNull(row["Начало_выпуска_из точки_нечетное"]))
                StartUneven = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Начало_выпуска_из точки_нечетное"].ToString()));

            if (!Convert.IsDBNull(row["Конец_выпуска_из точки_четное"]))
                EndEven = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Конец_выпуска_из точки_четное"].ToString()));

            if (!Convert.IsDBNull(row["Конец_выпуска_из точки_нечетное"]))
                EndUneven = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Конец_выпуска_из точки_нечетное"].ToString()));


            if (!Convert.IsDBNull(row["Направление_выхода_четное"]))
                ExitDirectionEven = Convert.ToInt32(row["Направление_выхода_четное"].ToString());

            if (!Convert.IsDBNull(row["Направление_выхода_нечетное"]))
                ExitDirectionUneven = Convert.ToInt32(row["Направление_выхода_нечетное"].ToString());


            flgGoBeforeFirstTrainEven = Convert.ToBoolean(row["Выйти_до_первого_пассажирского_четное"].ToString());
            flgGoBeforeFirstTrainUneven = Convert.ToBoolean(row["Выйти_до_первого_пассажирского_нечетное"].ToString());


            flgGoAfterLastTrainEven = Convert.ToBoolean(row["Зайти_после_последнего_пассажирского_четное"].ToString());
            flgGoAfterLastTrainUneven = Convert.ToBoolean(row["Зайти_после_последнего_пассажирского_нечетное"].ToString());


            MorningCoordinate = Convert.ToInt32(row["Координата_утро"].ToString());
            EveningCoordinate = Convert.ToInt32(row["Координата_вечер"].ToString());


            flgReserve = Convert.ToBoolean(row["Резервная"].ToString());


        }

        public NightStayPoint()
        {
        }

        public void Initialize(DataRow row)
        {
            var stationCode = Convert.ToUInt32(row["Станция"].ToString());
            station = MovementSchedule.colStation.SingleOrDefault(s => s.code == stationCode);
            
            if (station != null && station.colNightStayPoints != null)
                station.colNightStayPoints.Add(this);
            // TODO: а иначе? 

            if (!Convert.IsDBNull(row["Депо"]))
            {
                var depotCode = Convert.ToUInt32(row["Депо"].ToString());
                depot = MovementSchedule.colDepot.SingleOrDefault(d => d.code == depotCode);

                if (depot == null)
                    Error.showErrorMessage(new Depot { code = depotCode }, this);
            }

            var lineCode = Convert.ToUInt32(row["Линия"].ToString());
            line = MovementSchedule.colLine.SingleOrDefault(l => l.code == lineCode);

            if (line == null)
                Error.showErrorMessage(new Line() { code = lineCode }, this);

            if (!Convert.IsDBNull(row["Перегон"]))
            {
                var taskCode = Convert.ToUInt32(row["Перегон"].ToString());
                task = MovementSchedule.colTask.SingleOrDefault(t => t.code == taskCode);

                if (task == null)
                    Error.showErrorMessage(new Task() { code = taskCode }, this);
            }
        }

        public override String ToString()
        {
			return String.Concat(name," ",station.name);
           // return String.Format(name,station.name);
        }

    }
}
