/** 
 * @organization Departament "Control and Informatics in Technical Systems" (CITS@MIIT)
 * @author Konstantin Filipchenko (konstantin-649@mail.ru)
 * @version 0.1 
 * @date 2013-03-25
 * @description Source code based on clsPunktOsmotra.cls from GraphicPL@BASIC.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

using Messages;
using Converters;

namespace Core
{
    /// <summary>
    /// Пункт осмотра
    /// </summary>
    public class InspectionPoint : IEquatable<InspectionPoint>
    {
        /// <summary>
        /// Код (идентификатор) данного пункта осмотра в БД.
        /// </summary>
        public UInt32 code;

        /// <summary>
        /// Название данного пункта осмотра.
        /// </summary>
        public String name;

        // Депо
        public Depot depot = null;

        // Коллекция депо, составы приписанные к которым могут проходить осмотры в данном пункте осмотра.
        public List<Depot> colDepot;

        /// <summary>
        /// Коллекция коллекций (как с линейной индексацией, так и по номеру маршрута) (никаких == 0, ТО1 == 1, ТО2, ТО3 и периодических) ремонтов планируемых для данного пункта осмотра. Доступ к элементам -- первый индекс тип ремонта, например ТО-1, второй -- номер маршрута или номер записи в порядке добавления. Сохранение очерёдности (порядка добавления) элементов крайне важно, так как она задаёт очерёдность исполнения ремонтов на данном пункте осмотра.
        /// На самом деле теперь это словарь. 29.10.2013 14:35
        /// </summary>
        public Dictionary<UInt32, LinkedList<Repair>> colRepair;
        // public List<List<Repair>> repairMatrix;

        /// <summary>
        /// Точка ночной расстановки №1
        /// </summary>
        public NightStayPoint nightStayPoint_1;

        /// <summary>
        /// Точка ночной расстановки №2
        /// </summary>
        public NightStayPoint nightStayPoint_2 = null;

        /// <summary>
        /// Время конца работы. Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Int32 endWorkTime;

        /// <summary>
        /// Время конца перерыва. Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Nullable<Int32> endBreakTime;

        /// <summary>
        /// Время начала работы. Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Int32 beginWorkTime;

        /// <summary>
        /// Время начала перерыва. Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Nullable<Int32> beginBreakTime;

        public Boolean isBreakExist = false;
        /// <summary>
        /// Время смены канавы. Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Nullable<Int32> changeTime;

        /// <summary>
        /// Расписание работы пункта осмотра в виде списка интервалов работы.
        /// </summary>
        public WorkSchedule schedule;

        /// <summary>
        /// Заглушка вместо расписания работы!!!
        /// </summary>
        public List<Int32> times = new List<Int32>();

        /*
          <table name="Таблица пунктов осмотра">
         * Могут быть DBNull -- "Время_начала_перерыва", "Время_конца_перерыва", "Время_смены_канавы", "Депо" и  "Точка_ночной_расстановки_2". Остальные обязаны быть!!!
         *  <field name="Время_конца_перерыва" type="datetime" isPK="false" null="true" />
         *  <field name="Время_конца_работы" type="datetime" isPK="false" null="true" />
         *  <field name="Время_начала_перерыва" type="datetime" isPK="false" null="true" />
         *  <field name="Время_начала_работы" type="datetime" isPK="false" null="true" />
         *  <field name="Время_смены_канавы" type="datetime" isPK="false" null="true" />
         *  <field name="Депо" type="int" isPK="false" null="true" />
         *  <field name="Код" type="int" isPK="true" null="false" />
         *  <field name="Название" type="text" isPK="false" null="false" />
         *  <field name="Точка_ночной_расстановки_1" type="int" isPK="false" null="true" />
         *  <field name="Точка_ночной_расстановки_2" type="int" isPK="false" null="true" />
          </table>
         */

        public InspectionPoint() { }

        public InspectionPoint(DataRow row)
        {
            code = Convert.ToUInt32(row["Код"].ToString());

            name = row["Название"].ToString();

            if (!Convert.IsDBNull(row["Время_смены_канавы"]))
                changeTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_смены_канавы"].ToString()));

            // TODO: В структуре БД это поле необязательное!!!
            beginWorkTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_начала_работы"].ToString()));
            // TODO: В структуре БД это поле необязательное!!!
            endWorkTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_конца_работы"].ToString()));

            Logger.Output(String.Format("ПТО {0}: Время начала работы {1}, Время конца работы {2}. Наличие перерыва в работе {3}", name, beginWorkTime, endWorkTime, isBreakExist ? "ЕСТЬ" : "НЕТ"), GetType().FullName);

            schedule = new WorkSchedule(beginWorkTime, endWorkTime);

            if (!Convert.IsDBNull(row["Время_начала_перерыва"]))
                beginBreakTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_начала_перерыва"].ToString()));

            if (!Convert.IsDBNull(row["Время_конца_перерыва"]))
                endBreakTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_конца_перерыва"].ToString()));



            if (isBreakExist = (beginBreakTime.HasValue && endBreakTime.HasValue))
                schedule.InsertBreakInterval(beginBreakTime.Value, endBreakTime.Value);

            colDepot = new List<Depot>();

            // (никаких == 0, ТО1 == 1, ТО2, ТО3 и периодических) ремонтов планируемых для данного пункта осмотра
            colRepair = new Dictionary<UInt32, LinkedList<Repair>>();/* { 
                new LinkedList<Repair>(), // никаких == 0
                new LinkedList<Repair>(), // ТО1 == 1
                new LinkedList<Repair>(), // ТО2
                new LinkedList<Repair>(), // ТО3
                new LinkedList<Repair>() }; // периодический
            //repairMatrix = new List<List<Repair>>();
        */
            foreach (var type in MovementSchedule.colRepairType)
                colRepair.Add(type.code, new LinkedList<Repair>());
        }

        public bool Equals(InspectionPoint other)
        {
            return other != null && code == other.code;
        }

        public override String ToString()
        {
            return String.Format("{0} {1}", code, name);
        }

        public override int GetHashCode()
        {
            return Convert.ToInt32(code);
        }

        public int CompareTo(object obj)
        {
            var other = obj as InspectionPoint;
            return other == null ? 0 : Convert.ToInt32(code - other.code);
        }

        public void WriteToDatabase()
        {
            var table = Loader.m_syncData["Пункт осмотра"];
            var row = table.NewRow();
            row["Код"] = code;
            row["Название"] = name;
            row["Время_начала_ремонта"] = beginWorkTime;
            row["Время_конца_ремонта"] = endWorkTime;
            //row["Время_начала_ремонта"] = schedule.Begin;
            //row["Время_конца_ремонта"] = schedule.End;

            //if (isBreakExist)
            if (beginBreakTime.HasValue && endBreakTime.HasValue)
            {
                row["Время_начала_перерыва"] = beginBreakTime.Value;
                row["Время_конца_перерыва"] = endBreakTime.Value;
            }
            else
            {
                row["Время_начала_перерыва"] = DBNull.Value;
                row["Время_конца_перерыва"] = DBNull.Value;
            }

            if (changeTime.HasValue)
                row["Время_смены_канавы"] = changeTime.Value;
            else
                row["Время_смены_канавы"] = DBNull.Value;

            table.Rows.Add(row);
            table.AcceptChanges();
        }

        public void Initialize(DataRow row)
        {
            // If Not (IsNull(.Fields("Депо")) Or (.Fields("Депо") = 0)) Then
            if (!Convert.IsDBNull(row["Депо"]) || !row["Депо"].ToString().Equals(""))
            {
                var depotCode = Convert.ToUInt32(row["Депо"].ToString());
                depot = MovementSchedule.colDepot.FirstOrDefault(d => d.code == depotCode);

                if (depot != null)
                {
                    // TODO: Внимательно переписать!!!
                    colDepot.Add(depot);
#if LOGER_ON
                    Logger.Output(String.Format("Для ПТО <{1}> (№{2}) было добавлено в коллекцию депо (colDepot) депо <{0}>", depot.name, name, code), GetType().FullName);
#endif
                    if (depot.inspectionPoint == null)
                    {
                        depot.inspectionPoint = this;
#if LOGER_ON
                        Logger.Output(
                            String.Format("Для депо <{0}> был присвоен ПТО (inspectionPoint) <{1}> (№{2})", depot.name,
                                name, code), GetType().FullName);
#endif
                    }
                    else
                    {
#if LOGER_ON
                        Logger.Output(
                            String.Format("Для депо <{0}> уже существует ПТО (inspectionPoint) <{1}> (№{2})! ПТО <{3}> (№{4}) не был добавлен.", depot.name,
                                depot.inspectionPoint.name, depot.inspectionPoint.code, name, code), GetType().FullName);
#endif
                    }
                    depot.inspectionPoints.Add(this);
#if LOGER_ON
                    Logger.Output(String.Format("Для депо <{0}> был добавлен в коллекцию ПТО (inspectionPoints) ПТО <{1}> (№{2})", depot.name, name, code), GetType().FullName);
#endif
                }
                else
                {
                    Error.showErrorMessage(new Depot { code = depotCode }, this);
                }
            }


            // TODO: ["Точка_ночной_расстановки_1"] -- необязательное поле в структуре БД, переписать код 
            var pointCode = Convert.ToUInt32(row["Точка_ночной_расстановки_1"].ToString());
            nightStayPoint_1 = MovementSchedule.colNightStayPoint.FirstOrDefault(p => p.code == pointCode);

            if (nightStayPoint_1 != null)
            {
                nightStayPoint_1.inspectionPoint = this;
                nightStayPoint_1.station.inspectionPoint = this;
            }
            else
            {
                Error.showErrorMessage(new NightStayPoint(pointCode), this, "№1");
            }

            if (!Convert.IsDBNull(row["Точка_ночной_расстановки_2"]))
            {
                pointCode = Convert.ToUInt32(row["Точка_ночной_расстановки_2"].ToString());
                nightStayPoint_2 = MovementSchedule.colNightStayPoint.FirstOrDefault(p => p.code == pointCode);

                if (nightStayPoint_2 != null)
                {
                    nightStayPoint_2.inspectionPoint = this;
                    nightStayPoint_2.pairPoint = nightStayPoint_1;
                    if (nightStayPoint_1 != null)
                    {
                        nightStayPoint_1.pairPoint = nightStayPoint_2;
                        // nightStayPoint_2.pairPoint = nightStayPoint_1;
                    }
                }
                else
                {
                    Error.showErrorMessage(new NightStayPoint(pointCode), this, "№2");
                }
            }
        }

        /*  '__________________ Инициализация объекта _____________________________________
         Public Sub Init(rst As Recordset)
             Dim D As clsDepo
             Dim T As clsTypRemont
             Dim R As New Collection
             Dim I As Integer
                                         'Считывание информации из базы данных
             With rst
             Kod = .Fields("Код")
             Nazv = .Fields("Название")
             Set TochkaRasstanovki1 = colNightStayPoints(.Fields("Точка_ночной_расстановки_1"))
             Set TochkaRasstanovki1.PunktOsmotra = Me
             Set TochkaRasstanovki1.Station.PunktOsmotra = Me
    
             If Not IsNull(.Fields("Точка_ночной_расстановки_2")) Then
                 Set TochkaRasstanovki2 = colNightStayPoints(.Fields("Точка_ночной_расстановки_2"))
                 Set TochkaRasstanovki2.PunktOsmotra = Me
                 Set TochkaRasstanovki2.ParnayaTochka = TochkaRasstanovki1
                 Set TochkaRasstanovki1.ParnayaTochka = TochkaRasstanovki2
             Else
                 Set TochkaRasstanovki2 = Nothing
         '      Set TochkaRasstanovki2.PunktOsmotra = Nothing
             End If
                                         'Ссылка на депо, чьи поезда могут осматриваться
             If Not (IsNull(.Fields("Депо")) Or (.Fields("Депо") = 0)) Then
                 Set D = colDepo(.Fields("Депо"))
                 colDepoPrivyazki.Add D, CStr(D.Kod)
                 D.clPunktOsmotra.Add Me, CStr(Kod)
                 Set D.PunktOsmotra = Me
                 Set Depo = D
             Else
                 For Each D In colDepo
                 colDepoPrivyazki.Add D, CStr(D.Kod)
                 D.clPunktOsmotra.Add Me, CStr(Kod)
                 Next
             End If
    
             Set clRemont = New Collection
             For Each T In colTypRemont
                 I = I + 1
                 Set R = New Collection
                 clRemont.Add R, CStr(I)
                 Set R = Nothing
             Next
                                
                                         'Время начала и конца работы пункта
             WorkTimeBegin = TimeToSec(.Fields("Время_начала_работы"))
             WorkTimeEnd = TimeToSec(.Fields("Время_конца_работы"))
                                         'Время начала и конца перерыва в работе пункта
             If Not IsNull(.Fields("Время_начала_перерыва")) Then
                 PereryvTimeBegin = TimeToSec(.Fields("Время_начала_перерыва"))
                 PereryvTimeEnd = TimeToSec(.Fields("Время_конца_перерыва"))
             Else
                 PereryvTimeBegin = TimeNull
                 PereryvTimeEnd = TimeNull
             End If
             If Not IsNull(.Fields("Время_смены_канавы")) Then
                 TSmenaKanavy = TimeToSec(.Fields("Время_смены_канавы"))
             Else
                 TSmenaKanavy = TimeNull
             End If
             End With
  
             TimeEndLastRemont = TimeNull
             iAfterPereryv = 0
             flgNevypuschennyi = False
             flgSmenaKanavy = False
         End Sub
          */

        /// <summary>
        /// Поиск вариантов ремонтов для заданного пункта.
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <param name="Tmin"></param>
        /// <param name="Tmax"></param>
        /// <returns>Список полученных вариантов.</returns>
        public List<Repair> searchVariants(Int32 beginTime, Int32 endTime, Int32 Tmin, Int32 Tmax)
        {
#if LOGER_ON
            Logger.Output(message: "in searchVariants(...)", source: GetType().FullName);
#endif
            var colVar = new List<Repair>();

            var TO1 = MovementSchedule.colRepairType.SingleOrDefault(t => t.name.Equals("ТО1"));


            if (TO1 == null || colRepair.Count == 0)
            {
#if LOGER_ON
                Logger.Output("Беда!!!", GetType().FullName);
#endif
                return colVar;
            }

            // var repairs = colRepair.First().Value;

            Int32 bt, et;

            var lllist = colRepair.First().Value;
            if (lllist.Count == 0)
            {
                bt = Math.Max(beginTime, Tmin);
                et = Math.Min(endTime, Tmax);
                if (et >= bt + TO1.duration)
                    colVar.Add(new Repair { beginTime = bt, endTime = et, inspectionPoint = this });
            }
            else
            {
                var current = lllist.First;
                // Если до первого ремонта есть место для вставки, то добавляем вариант
                if (Tmin + TO1.duration <= current.Value.beginTime)
                {
                    bt = Math.Max(beginTime, Tmin);
                    et = Math.Min(current.Value.beginTime, Tmax);
                    if (et >= bt + TO1.duration)
                        colVar.Add(new Repair { beginTime = bt, endTime = et, inspectionPoint = this });
                }

                if (current.Next != null)
                {
                    var previous = current.Value;
                    current = current.Next;
                    while (current.Next != null)
                    {
                        // Если между двумя ремонтами есть место для вставки, то добавляем вариант
                        bt = Math.Max(previous.endTime, Tmin);
                        et = Math.Min(current.Value.beginTime, Tmax);
                        if (et >= bt + TO1.duration)
                        // && bt + TO1.duration > p.repairMatrix.ElementAt(index).beginTime)
                        {
                            previous = new Repair { beginTime = bt, endTime = et, inspectionPoint = this };
                            colVar.Add(previous);
                        }
                        else
                        {
                            previous = current.Value;
                        }
                        current = current.Next;
                    }
                }

                // Если после последнего ремонта есть место для вставки, то добавляем вариант
                if (current != null && Tmax > current.Value.endTime)
                {
                    bt = Math.Max(current.Value.endTime, Tmin);
                    et = Math.Max(endTime, Tmax);
                    if (et >= bt + TO1.duration)
                        colVar.Add(new Repair { beginTime = bt, endTime = et, inspectionPoint = this });
                }
            }

            /*
            // Если это первый вставляемый ремонт, то добавляем вариант 
            if (repairs.Count == 0)
            {
                bt = Math.Max(beginTime, Tmin);
                et = Math.Max(endTime, Tmax);
                if (et >= bt + TO1.duration)
                {
                    colVar.Add(new Repair { beginTime = bt, endTime = et, inspectionPoint = this });
                }
            }
            else
            {
                var first = repairs.First();
                var last  = repairs.Last();
                
                // Если до первого ремонта есть место для вставки, то добавляем вариант 
                if (Tmin + TO1.duration <= first.beginTime)
                {
                    bt = Math.Max(beginTime, Tmin);
                    et = Math.Max(first.beginTime, Tmax);
                    if (et >= bt + TO1.duration)
                    {
                        colVar.Add(new Repair { beginTime = bt, endTime = et, inspectionPoint = this });
                    }
                }

                var previous = first;
                var elements = new List<Repair> { first, last };

                repairs.Except(elements).Where(repair =>
                { // Если между двумя ремонтами есть место для вставки, то добавляем вариант 
                    bt = Math.Max(previous.endTime, Tmin);
                    et = Math.Max(repair.beginTime, Tmax);
                    return (et >= bt + TO1.duration); // && bt + TO1.duration > p.repairMatrix.ElementAt(index).beginTime)
                }).Select(t => 
                { 
                    previous = new Repair { beginTime = bt, endTime = et, inspectionPoint = this }; 
                    colVar.Add(previous);
                    return t;
                });

                // Если после последнего ремонта есть место для вставки, то добавляем вариант 
                if (Tmax > last.endTime)
                {
                    bt = Math.Max(last.endTime, Tmin);
                    et = Math.Max(endTime, Tmax);
                    if (et >= bt + TO1.duration)
                    {
                        colVar.Add(new Repair { beginTime = bt, endTime = et, inspectionPoint = this });
                    }
                }
            }
            */
            return colVar;
        }
    }
}
