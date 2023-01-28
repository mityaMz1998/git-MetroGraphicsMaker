/** 
 * @organization Departament "Control and Informatics in Technical Systems" (CITS@MIIT)
 * @author Konstantin Filipchenko (konstantin-649@mail.ru)
 * @version 0.1 
 * @date 2013-03-25
 * @description Source code based on clsDepo.cls from GraphicPL@BASIC and clsDepo.cs from GraphicPL@C#.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml;
using Messages;
using Converters;

namespace Core
{
    /// <summary>
    /// Депо
    /// </summary>
    public class Depot : Entity, IEquatable<Depot>
    {
        /// <summary>
        /// The code of the depot.
        /// </summary>
        public UInt32 code;

        /// <summary>
        /// The name of the depot.
        /// </summary>
        public String name;

        /// <summary>
        /// Пункт осмотра в депо.
        /// </summary>
        public InspectionPoint inspectionPoint;

        /// <summary>
        /// The station which connected with depot on first way.
        /// </summary>
        public Station stationOnFirstWay = null;

        /// <summary>
        /// The station which connected with depot on second way.
        /// </summary>
        public Station stationOnSecondWay = null;

        /// <summary>
        /// The collection of routes which belong to a depot.
        /// </summary>
        public IList<Route> colOwnRoutes;

        /// <summary>
        /// The collection of routes which stay into depot.
        /// </summary>
        public IList<Route> colRoutes;

        /// <summary>
        /// Коллекция пунктов осмотра, где осмотриваются составы из депо
        /// </summary>
        public List<InspectionPoint> inspectionPoints;

        /// <summary>
        /// The amount of routes which stay in line points at the night time.
        /// </summary>
        public UInt32 amountOfRoutesInLinePoints;

        /// <summary>
        /// The amount of routes which stay in their own rest points at the night time.
        /// </summary>
        public UInt32 amountOfRoutesInTheirOwnPoints;

        /// <summary>
        /// The amount of routes which stay in the common rest points at the night time.
        /// </summary>
        public UInt32 amountOfRoutesInCommonPoints;

        /// <summary>
        /// Время движения от главных путей линии до светофора Е. Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Int32 timeFromLineToE = TimeConverter.zeroSeconds;

        /// <summary>
        /// Время движения от светофора Е до главных путей линии. Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Int32 timeFromEToLine;

        /// <summary>
        /// Время начала перерыва. Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Int32 beginBreakTime;

        /// <summary>
        /// Время конца перерыва. Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Int32 endBreakTime;

        /// <summary>
        /// 
        /// </summary>
        public TimeInterval breakTime;

        /// <summary>
        /// Признак парного
        /// </summary>
        public Boolean isPair;

        /// <summary>
        /// Парное депо
        /// </summary>
        public Depot pairDepot;

        /// <summary>
        /// Признак выхода из периодического
        /// </summary>
        public Boolean isPeriodic;

        /// <summary>
        /// Длина задания
        /// </summary>
        public Nullable<Int32> taskLength;

        /// <summary>
        /// Задание ухода в депо
        /// </summary>
        public Task task;

        /// <summary>
        /// Название светофора Е (Может быть null)
        /// </summary>
        public String EName;

        /// <summary>
        /// Сдвиг линии Е (Может быть null)
        /// </summary>
        public Nullable<Int32> ShiftELine;

        /// <summary>
        /// Рисовать поле
        /// </summary>
        public Boolean drawField;
        // ... 

        /*
          <table name="Таблица депо">
        ?*  <field name="Время_движения_до_светофора_Е"     type="datetime" isPK="false" null="true" />
            <field name="Длина_Задания"                     type="int"      isPK="false" null="true" />
            <field name="Длина_ССВ"                         type="int"      isPK="false" null="true" />
            <field name="Задание_Ухода_в_Депо"              type="int"      isPK="false" null="true" />
         *  <field name="Код"                               type="int"      isPK="true"  null="false" />
         *  <field name="Код_Станции1"                      type="int"      isPK="false" null="false" />
         *  <field name="Код_Станции2"                      type="int"      isPK="false" null="false" />
        ?*  <field name="Конец_Перерыва"                    type="datetime" isPK="false" null="true" />
         *  <field name="Название"                          type="text"     isPK="false" null="false" />
        ?*  <field name="Начало_Перерыва"                   type="datetime" isPK="false" null="true" />
            <field name="Парное"                            type="int"      isPK="false" null="true" />
            <field name="Положение"                         type="int"      isPK="false" null="true" />
            <field name="Признак_выхода_из_периодического"  type="bit"      isPK="false" null="false" />
            <field name="Признак_парного"                   type="bit"      isPK="false" null="false" />
          </table>
         */

        private Boolean oldDB = false;

        public Depot() { }

        public Depot(DataRow row)
        {
            code = Convert.ToUInt32(row["Код"].ToString());

            name = row["Название"].ToString();

            if (!Convert.IsDBNull(row["Начало_Перерыва"]))
                beginBreakTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Начало_Перерыва"].ToString()));

            if (!Convert.IsDBNull(row["Конец_Перерыва"]))
                endBreakTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Конец_Перерыва"].ToString()));

            if (beginBreakTime != endBreakTime)
                breakTime = new TimeInterval(beginBreakTime, endBreakTime);

            isPeriodic = Convert.ToBoolean(row["Признак_выхода_из_периодического"].ToString());

            isPair = Convert.ToBoolean(row["Признак_парного"].ToString());

            if (!Convert.IsDBNull(row["Время_движения_до_светофора_Е"]))
                timeFromLineToE = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_движения_до_светофора_Е"].ToString()));

            try
            {
                if (!Convert.IsDBNull(row["Время_движения_от_светофора_Е"]))
                    timeFromEToLine = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_движения_от_светофора_Е"].ToString()));

                if (!Convert.IsDBNull(row["Название_Е"]))
                    EName = row["Название_Е"].ToString();

                if (!Convert.IsDBNull(row["Сдвиг_линии_Е"]))
                    ShiftELine = Convert.ToInt32(row["Сдвиг_линии_Е"].ToString());

                if (!Convert.IsDBNull(row["Рисовать_поле"]))
                    drawField = Convert.ToBoolean(row["Рисовать_поле"].ToString());
            }
            catch
            {
            }

            try
            {
                if (oldDB = !Convert.IsDBNull(row["Длина_задания"]))
                    taskLength = Convert.ToInt32(row["Длина_задания"].ToString());
            }
            catch
            {
#if LOGER_ON
                Logger.Output("Беда", GetType().FullName);
#endif
            }

            colRoutes = new List<Route>();

            colOwnRoutes = new List<Route>();

            inspectionPoints = new List<InspectionPoint>();
        }


        public void LoadToDatabase(ref DataTable table)
        {
            //var row = Loader.m_syncData["Депо"].NewRow();

            var row = table.NewRow();
            row["Код"] = code;
            row["Название"] = name;

            row["Начало_Перерыва"] = DBNull.Value;
            if (beginBreakTime != 0)
                row["Начало_Перерыва"] = beginBreakTime;

            row["Конец_Перерыва"] = DBNull.Value;
            if (beginBreakTime != 0)
                row["Конец_Перерыва"] = endBreakTime;

            /*
            row["Начало_Перерыва"] = DBNull.Value;
            row["Конец_Перерыва"]  = DBNull.Value;
            if (breakTime != null)
            {
                row["Начало_Перерыва"] = breakTime.begin;
                row["Конец_Перерыва"] = breakTime.end;
            }
            */

            row["Признак_выхода_из_периодического"] = isPeriodic;
            row["Признак_парного"] = isPair;

            row["Код_Станции1"] = DBNull.Value;
            if (stationOnFirstWay != null)
                row["Код_Станции1"] = stationOnFirstWay.code;

            row["Код_Станции2"] = DBNull.Value;
            if (stationOnSecondWay != null)
                row["Код_Станции2"] = stationOnSecondWay.code;

            row["Задание_ухода_в_депо"] = DBNull.Value;
            if (task != null)
                row["Задание_ухода_в_депо"] = task.code;

            row["Время_движения_до_светофора_Е"] = TimeConverter.SecondsToTime(timeFromLineToE);

            row["Длина_задания"] = DBNull.Value;
            if (taskLength.HasValue)
                row["Длина_задания"] = TimeConverter.SecondsToTime(taskLength.Value);

            table.Rows.Add(row);
            table.AcceptChanges();
        }

        /// <summary>
        /// Инициалазация объекта по строке данных.
        /// </summary>
        /// <param name="row">Строка данных из соответствующей (классу) виртуальной таблицы БД.</param>
        public void Initialize(DataRow row)
        {
            var stationCode = Convert.ToUInt32(row["Код_Станции1"].ToString());
            stationOnFirstWay = MovementSchedule.colStation.SingleOrDefault(s => s.code == stationCode);

            if (stationOnFirstWay != null)
                stationOnFirstWay.depotOnFirstWay = this;
            else
                Error.showErrorMessage(new Station { code = stationCode }, this, "1");

            stationCode = Convert.ToUInt32(row["Код_Станции2"].ToString());
            stationOnSecondWay = MovementSchedule.colStation.SingleOrDefault(s => s.code == stationCode);

            if (stationOnSecondWay != null)
                stationOnSecondWay.depotOnSecondWay = this;
            else
                Error.showErrorMessage(new Station { code = stationCode }, this, "2");

            if (!Convert.IsDBNull(row["Задание_ухода_в_депо"].ToString()))
            {
                var taskCode = Convert.ToUInt32(row["Задание_ухода_в_депо"].ToString());
                task = MovementSchedule.colTask.SingleOrDefault(t => t.code == taskCode);
                if (task == null)
                    Error.showErrorMessage(new Task { code = taskCode }, this);
                else if (oldDB)
                    task.Length = taskLength ?? 0;
            }

            /*

                                         'Формируем коллекцию маршрутов,
                                             'приписанных к этому депо
                  Set colPzd = New Collection
                  With RecMarshDepo
                    .FindFirst "Депо_привязки = " & CStr(Kod)
                    If .NoMatch Then Exit Sub
                    Coo = hMarsh - 1
                    While .Fields("Депо_привязки") = Kod
                      Set Pzd = colMarshrut(.Fields("Код"))
                      Pzd.CooY = Coo
                      colPzd.Add Pzd
                      .MoveNext
                      If .EOF Then Exit Sub
                      Coo = Coo + hMarsh
                    Wend
                  End With
                                             'Если это первое депо в коллекции, то делаем
                                             'его выбранным для отображения графика оборота
                  If NumSelDepo = 0 Then NewActiveDepo True ': frmMain.mnuNextDepo.Enabled = True
                  LastBeforePereryv = TimeNull
                  colMyPoint = 0
                  colLinePoint = 0
             * */


        }

        public bool Equals(Depot other)
        {
            return other != null && code == other.code;
        }

        public override String ToString()
        {
            return String.Format("Депо {0} (Код {1}) по первому пути на станцию {2}, по второму -- {3}", name, code, stationOnFirstWay.name, stationOnSecondWay.name);
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(code);
        }
    }
}

