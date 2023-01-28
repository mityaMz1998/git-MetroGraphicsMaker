/** 
 * @organization Departament "Management and Informatics in Technical Systems" (MITS@MIIT)
 * @author Konstantin Filipchenko (konstantin-649@mail.ru)
 * @version 0.1 
 * @date 2013-03-25
 * @description Source code based on clsRemont.cls from GraphicPL@BASIC.
 */

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Messages;
using Converters;

namespace Core
{
    /// <summary>
    /// Ремонт подвижного состава.
    /// </summary>
    public class Repair : Entity
    {
        /// <summary>
        /// Тип ремонта подвижного состава.
        /// </summary>
        public RepairType type;

        /// <summary>
        /// Ремонтируемый маршрут.
        /// </summary>
        public Route route;

        /// <summary>
        /// "Нитка", с которой маршрут ушёл в ремонт.
        /// </summary>
        public TrainPath lastThread;

        /// <summary>
        /// Место проведения данного ремонта.
        /// </summary>
        public InspectionPoint inspectionPoint = null;

        /// <summary>
        /// Время начала данного ремонта. Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
         public Int32 beginTime;

        /// <summary>
         /// Время конца данного ремонта. Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
         public Int32 endTime;

        /// <summary>
        /// Временной интервал проведения ремонта.
        /// </summary>
        public TimeInterval time;

        /// <summary>
        /// Флаг продолжения ремонта.
        /// </summary>
        public Boolean isContinue;

        /// <summary>
        /// Состояние
        /// </summary>
        public Int32 state = 0;

        /// <summary>
        /// Ложный ремонт
        /// </summary>
        public Boolean IsFake = false;
        public override String ToString()
        {
            return String.Format("Ремонт [{1} ({2}) -- {3} ({4})] -продолжение: [{0}]", isContinue ? "X" : " ", TimeConverter.SecondsToString(beginTime), beginTime, TimeConverter.SecondsToString(endTime), endTime);
        }

        public Repair() { }

        public Repair(DataRow row)
        {
            // ---- Старый вариант --
            beginTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_начала"].ToString()));

            endTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_окончания"].ToString()));
            // ----------------------

            // ---- Новый вариант ---
            time = new TimeInterval(beginTime, endTime);
            // ----------------------

            isContinue = Convert.ToBoolean(row["Продолжение"].ToString());

            if (!Convert.IsDBNull(row["Состояние"]))
                state = Convert.ToInt32(row["Состояние"].ToString());
        }

        /// <summary>
        /// Создание ремонта по параметрам.
        /// </summary>
        /// <param name="type">Тип реионта.</param>
        /// <param name="route">Маршрут, которому назначается ремонт.</param>
        /// <param name="point">Место ремонта.</param>
        /// <param name="isContinue">Флаг продолжения ремонта (если не успели доделать ремонт и выпустить состав на линию до окончания движения, то переносим ремонт на следующий маршрут и выставляем флаг).</param>
        public Repair(RepairType type, Route route, InspectionPoint point, Boolean isContinue)
        {
            this.type = type;
            this.route = route;
            inspectionPoint = point;
            this.isContinue = isContinue;
        }

        /// <summary>
        /// Инициализация объекта по строке данных.
        /// </summary>
        /// <param name="row">Строка данных из соответствующей (классу) таблицы БД.</param>
        public void Initialize(DataRow row)
        {
            /*
             * Всё кроме "Место_ремонта" обязано быть!!!
              <table name="График оборота: Ремонты">
             *  <field name="Время_начала"      type="datetime" isPK="false" null="true" />
             *  <field name="Время_окончания"   type="datetime" isPK="false" null="true" />
             *  <field name="Место_ремонта"     type="int"      isPK="false" null="true" />
             *  <field name="Номер_маршрута"    type="int"      isPK="true"  null="true" />
             *  <field name="Продолжение"       type="bit"      isPK="false" null="false" />
             *  <field name="Состояние"         type="int"      isPK="false" null="true" />
             *  <field name="Тип_ремонта"       type="int"      isPK="false" null="true" />
              </table>
             */

            var typeCode = Convert.ToUInt32(row["Тип_ремонта"].ToString());
            type = MovementSchedule.colRepairType.SingleOrDefault(t => t.code == typeCode);

            if (type == null)
                Error.showErrorMessage(new RepairType {code = typeCode }, this);

            var routeCode = Convert.ToUInt32(row["Номер_маршрута"].ToString());
            route = MovementSchedule.colRoute.SingleOrDefault(r => r.number == routeCode);
            if (route == null)
            {
                Error.showErrorMessage(new Route {number = routeCode}, this);
            }
            else
            {
                foreach (var element in route.ElementsOfMovementSchedule.Where(element => element.beginTime >= time.end))
                    element.beforeRepair = false;
            }

            // TODO: Откуда это получать?
             // lastThread = mdlData.colThread.FirstOrDefault(t => t.code == Convert.ToInt32(row["?"].ToString()));

            if (!Convert.IsDBNull(row["Место_ремонта"]))
            {
                var pointCode = Convert.ToUInt32(row["Место_ремонта"].ToString());
                inspectionPoint = MovementSchedule.colInspectionPoint.FirstOrDefault(p => p.code == pointCode);

                if (inspectionPoint == null)
                    Error.showErrorMessage(new InspectionPoint() { code = pointCode }, this);
                //else if (type != null)
                //    inspectionPoint.colRepair[type.code].

                try
                {
                    inspectionPoint.colRepair[type.code].AddLast(this);
                }
                catch (NullReferenceException nre)
                {
                    //Logger.output();
                }
                catch (KeyNotFoundException knfe)
                {

                }

            }
        }

        /*
         '__________________ Включение ремонта в коллекцию ремонтов для заданного пункта
            Public Sub AddColRemont(p As ClsPunktOsmotra)
                Dim index As Integer
                Dim r As Collection
    
                Set r = p.clRemont(CStr(Typ.Kod))
                If r.Count > 0 Then
                For index = 1 To r.Count
                    If Tb < r(index).Tb Then
                    r.Add Me, CStr(Marshrut.NomMarsh), index
                    Exit For
                    Else
                    If index = r.Count Then r.Add Me, CStr(Marshrut.NomMarsh)
                    End If
                Next
                Else
                r.Add Me, CStr(Marshrut.NomMarsh)
                End If
            End Sub
         */

        /// <summary>
        /// Включение ремонта в коллекцию ремонтов для заданного пункта
        /// </summary>
        /// <param name="point">Некоторый пункт осмотра, на котором должен быть проведён данный ремонт.</param>
        public void addRepair(InspectionPoint point)
        {
            if (point == null)
                throw new ArgumentNullException("point");
            
            try
            {
                var repairs = point.colRepair[type.code];
                // @NOTE: Аналог см. в ElementOfMovementSchedule.Initialize(...)
                var current = repairs.First;
                var isElementInserted = false;
                while (current != null && !isElementInserted)
                {
                    if (current.Value.beginTime > beginTime)
                    {
                        repairs.AddBefore(current, this);
                        isElementInserted = true;
                    }
                    current = current.Next;
                }
                
                if (!isElementInserted)
                    repairs.AddLast(this);

                Logger.Output(
                    String.Format("Для ПТО №{0} был добавлен ремонт {1} маршрута №{2}.", point.code, time, route.number), 
                    GetType().FullName);
            }
            catch (Exception e) 
            {
                Logger.Output(
                    String.Format("Для ПТО №{0} НЕ был добавлен ремонт {1} маршрута №{2:D3}, так как произошла исключительная ситуация:{3}{4}", point.code, time, route.number, Environment.NewLine, e), 
                    GetType().FullName);
            }
        }
            

        /// <summary>
        /// Обёртка для подготовки интерфейса.
        /// </summary>
        /// <param name="variant"></param>
        /// <returns></returns>
        public Boolean insert(TimeVariant variant)
        {
            return insert(variant.Tmin, variant.Tdes, variant.Tmax);
        }

        /// <summary>
        /// Определение места проведения данного ремонта
        /// </summary>
        /// <param name="Tmin">Минимально допустимое время начала ремонта (левая граница временнОго окна).</param>
        /// <param name="Tdes">Желаемое время начала ремонта.</param>
        /// <param name="Tmax">Максимально допустимое время начала ремонта (правая граница временнОго окна).</param>
        /// <returns></returns>
        public Boolean insert(Int32 Tmin, Int32 Tdes, Int32 Tmax)
        {
            var colTime = new List<Repair>();
            var colVar = new List<Repair>();
            var TO1 = MovementSchedule.colRepairType.FirstOrDefault(r => r.name == "ТО1");

            if (TO1 == null)
                return false;

            /* Исключаем из рассмотрения часы-пик */
            var r1 = new Repair();
            var repair = new Repair { beginTime = Tmin, endTime = Tmax + TO1.duration };

            time = new TimeInterval(Tmin, Tmax + TO1.duration);

            var point = route.depot.inspectionPoints.SingleOrDefault(p => p.code == 1);
            addRepair(point);
            colTime.Add(repair);
            return true;


            /*
            var schedule = new WorkSchedule(TimeConverter.MovementTime);
            schedule.InsertBreakInterval(TimeConverter.MorningPeak);
            schedule.InsertBreakInterval(TimeConverter.EveaningPeak);

            var interval = new TimeInterval(Tmin, Tmax);
            */




            /*
            if (Tmin < mdlData.beginTimePeak1)
            {
                if (Tmax < mdlData.beginTimePeak1)
                {
                    colTime.Add(new Repair() {beginTime = Tmin, endTime = Tmax});
                }
                else
                {
                    colTime.Add(new Repair() {beginTime = Tmin, endTime = mdlData.beginTimePeak1});
                    if (Tmax > mdlData.endTimePeak1)
                    {
                        if (Tmax < mdlData.beginTimePeak2)
                        {
                            colTime.Add(new Repair() {beginTime = mdlData.endTimePeak1, endTime = Tmax});
                        }
                        else 
                        {
                            colTime.Add(new Repair() {beginTime = mdlData.endTimePeak1, endTime = mdlData.beginTimePeak2});
                            if (Tmax > mdlData.endTimePeak2)
                            {
                                colTime.Add(new Repair() {beginTime = mdlData.endTimePeak2, endTime = Tmax});
                            }
                        }
                    }
                }
            }
            else
            {
                if (Tmin < mdlData.beginTimePeak2)
                {
                    if (Tmax < mdlData.beginTimePeak2)
                    {
                        colTime.Add(new Repair() { beginTime = Math.Max(Tmin, mdlData.endTimePeak1), endTime = Tmax });
                    }
                    else
                    {
                        colTime.Add(new Repair() { beginTime = Math.Max(Tmin, mdlData.endTimePeak1), endTime = mdlData.beginTimePeak2 });
                        if (Tmax > mdlData.endTimePeak2)
                        {
                            colTime.Add(new Repair() { beginTime = mdlData.endTimePeak2, endTime = Tmax });
                        }
                    }
                }
                else
                {
                    if (Tmax > mdlData.endTimePeak2)
                    {
                        colTime.Add(new Repair() { beginTime = Math.Max(Tmin, mdlData.endTimePeak2), endTime = Tmax });
                    }
                }
            }
            */

            //Int32 det = 0;
            /* 
            // Ищем возможные варианты ремонтов для заданных интервалов
            //foreach (var repair in colTime)
            {
                // Ищем возможные варианты ремонтов по всем доступным пунктам 
                foreach (var point in route.depot.inspectionPoints)
                {
                    // Если рассматриваемый интревал не входит во время работы пункта, то не рассматриваем этот пункт
                    if (point.beginWorkTime <= repair.endTime && repair.beginTime < point.endWorkTime)
                    {
                        // Если в работе пункта есть перерыв, то рассматриваем два интервала, а с учетом вечернего пика - три
                        if (point.endBreakTime > TimeConverter.zeroSeconds)
                        {
                            //det = Math.Max(point.beginWorkTime, mdlData.endTimePeak2);
                            //if (repair.endTime > det || repair.beginTime < point.beginBreakTime)
                            //{
                            if (point.endBreakTime >= TimeConverter.endTimePeak2)
                            {
                                if (Math.Max(point.beginWorkTime, repair.beginTime) < Math.Min(point.beginBreakTime, repair.endTime))
                                {
                                    colVar.AddRange(point.searchVariants(point.beginWorkTime, point.beginBreakTime, Math.Max(point.beginWorkTime, repair.beginTime), Math.Min(point.beginBreakTime, repair.endTime)));
                                }
                                if (Math.Max(point.endBreakTime, repair.beginTime) < Math.Min(point.endWorkTime, repair.endTime))
                                {
                                    colVar.AddRange(point.searchVariants(point.endBreakTime, point.endWorkTime, Math.Max(point.endBreakTime, repair.beginTime), Math.Min(point.endWorkTime, repair.endTime)));
                                }
                                // Если пункт осмотра в депо, а время осмотра попадает в вечерний пик, то рассматриваем два интервала
                            }
                            else
                            {
                                if (Math.Max(point.beginWorkTime, repair.beginTime) < Math.Min(point.beginBreakTime, repair.endTime))
                                {
                                    colVar.AddRange(point.searchVariants(point.beginWorkTime, point.beginBreakTime, Math.Max(point.beginWorkTime, repair.beginTime), Math.Min(point.beginBreakTime, repair.endTime)));
                                }
                                if (Math.Max(point.endBreakTime, repair.beginTime) < Math.Min(TimeConverter.beginTimePeak2 - TO1.duration, repair.endTime))
                                {
                                    colVar.AddRange(point.searchVariants(point.endBreakTime, TimeConverter.beginTimePeak2 - TO1.duration, Math.Max(point.endBreakTime, repair.beginTime), Math.Min(TimeConverter.beginTimePeak2 - TO1.duration, repair.endTime)));
                                }
                                if (Math.Max(TimeConverter.endTimePeak2, repair.beginTime) < Math.Min(TimeConverter.endTimePeak2, repair.beginTime))
                                {
                                    colVar.AddRange(point.searchVariants(TimeConverter.endTimePeak2, point.endWorkTime, Math.Max(TimeConverter.endTimePeak2, repair.beginTime), Math.Min(point.endWorkTime, repair.endTime)));
                                }
                            }
                            //}
                        }
                        else
                        {
                            colVar.AddRange(point.searchVariants(point.beginWorkTime, point.endWorkTime, repair.beginTime, repair.endTime));
                        }
                    }
                    else Logger.output(String.Format("repair: [{0} -- {1}]; point.workTime [{2} -- {3}]", TimeConverter.SecondsToString(repair.beginTime), TimeConverter.SecondsToString(repair.endTime), TimeConverter.SecondsToString(point.beginWorkTime), TimeConverter.SecondsToString(point.endWorkTime)), GetType().FullName);
                }
            }


            var flag = false;
            Int32 det = 23 * TimeConverter.secondsInHour, 
                  det1 = det;

            // Из возможных вариантов выбираем наилучший
            foreach (var variant in colVar)
            {
                if (mdlAutoCorrection.isTimeInInterval(variant.beginTime, Tdes, variant.endTime) && variant.inspectionPoint.nightStayPoint_1.depot != null)
                {
                    // Выбираем лучший вариант из доступных - ремонт в желаемое время на линии
                    flag = true;
                    inspectionPoint = variant.inspectionPoint;
                    beginTime = variant.beginTime + Convert.ToInt32(Convert.ToSingle(Tdes - variant.beginTime) / Convert.ToSingle(TO1.duration + TimeConverter.secondsInMinute)) * TO1.duration + TimeConverter.secondsInMinute;
                    endTime = beginTime + TO1.duration;
                    break;
                }
                else
                {
                    // Если лучшего варианта нет, то ищем ближайший из доступных
                    if (Tdes < variant.beginTime)
                    {
                        if (det > (variant.beginTime - Tdes))
                        {
                            det = (variant.beginTime - Tdes);
                            repair = variant;
                        }
                        else if (Tdes > variant.endTime)
                        {
                            if (det > (variant.endTime - Tdes))
                            {
                                det = (variant.endTime - Tdes);
                                repair = variant;
                            }
                        }
                        else 
                        {
                            det = TimeConverter.zeroSeconds;
                            repair = variant;
                        }

                        if (variant.inspectionPoint.depot != null && Tdes < variant.beginTime)
                        {
                            if (det1 > (variant.beginTime - Tdes))
                            {
                                det1 = (variant.beginTime - Tdes);
                                r1 = variant;
                            }
                            else if (Tdes > variant.endTime && det1 > (Tdes - variant.endTime))
                            {
                                    det = Tdes - variant.endTime;
                                    r1 = variant;
                            }
                        }
                    }
                }
            }


            // Если не смогли вставиться в желаемое время, то вставляемся как можно ближе к нему 
            if (!flag && r1 != null && repair != null)
            {
                // If Not ((r.Tb > t) Or (t < r.Te) And (r1.Tb < t)) Then
                if (repair.beginTime <= Tdes && Tdes >= repair.endTime || r1.beginTime >= Tdes)
                {
                    if (r1.beginTime > Tdes)
                    {
                        beginTime = r1.beginTime;
                        endTime = beginTime + TO1.duration;
                    }
                    else 
                    {
                        endTime = r1.endTime;
                        beginTime = endTime - TO1.duration;
                    }
                    inspectionPoint = r1.inspectionPoint;
                    flag = true;
                }
            }

            if (!flag && repair != null)
            {
                if (repair.beginTime > Tdes)
                {
                    beginTime = repair.beginTime;
                    endTime = beginTime + TO1.duration;
                }
                else if (Tdes > repair.endTime)
                {
                    endTime = repair.endTime;
                    beginTime = endTime - TO1.duration;
                }
                else 
                {
                    beginTime = repair.beginTime + Convert.ToInt32(Convert.ToSingle(Tdes - repair.beginTime) / Convert.ToSingle(TO1.duration + TimeConverter.secondsInMinute)) * TO1.duration + TimeConverter.secondsInMinute;
                    endTime = beginTime + TO1.duration;
                }
                inspectionPoint = repair.inspectionPoint;
                flag = true;
            }

            if (flag)
            {
                try
                {
                    addRepair(inspectionPoint);
                }
                catch(Exception e)
                {
                    Logger.output(e.ToString(), GetType().FullName);
                    return false;
                }
            }
            
           return flag;
             */
        }

        /// <summary>
        /// Опеределение времени входа и выхода из ремонта, проводимого в депо. Время измеряется в количестве временных интервалов, выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public void getTimes()
        {
            /* Если начало ремонта в депо назначено на время перерыва в работе депо, то переносим начало ремонта на время до перерыва. */
            /* Если рекомендуемое время ремонта попадает на перерыв в работе депо, то... */
            if (type.recomendedBeginTime > route.depot.beginBreakTime && type.recomendedBeginTime < route.depot.endBreakTime)
                /* ... время начала ремонта = времени начала перерыва (спихнули маршрут в депо на осмотр и радуемся -- нам-то всё равно), а иначе... */
                beginTime = route.depot.beginBreakTime;
            else
                /* ... рекомендуемому времени начала ремонта. */
                beginTime = type.recomendedBeginTime;
            
            endTime = beginTime + type.duration;

            /*Если время выхода из ремонта превышает время окончания движения, то переносим окончание ремонта на следующий маршрут.*/
            if (endTime > MovementSchedule.MovementTime.end) //mdlData.timeFinishMovement)
            {

                var next = new Repair
                {
                    type = type,
                    route = route.nextRoute,
                    inspectionPoint = inspectionPoint,
                    isContinue = true,
                    beginTime =  MovementSchedule.MovementTime.begin,//mdlData.timeStartMovement,
                    endTime = endTime - MovementSchedule.MovementTime.duration
                };
                route.nextRoute.isNotOutFromDepotBeforeRepair = true;
#if IS_REPAIR_COLLECTION
                route.nextRoute.Repairs.Add(next);
#else
                route.nextRoute.repair = next;
#endif
                endTime = MovementSchedule.MovementTime.end;
                next.correctTimes();
            }
            else
            {
                correctTimes();
            }
        }

        /// <summary>
        /// Коррекция времени выхода из ремонта в депо.
        /// </summary>
        public void correctTimes()
        {
            /* Если выход из ремонта приходится на перерыв, то выводим поезд из депо после перерыва. */
            //TODO: А что если endTime < route.depot.beginBreakTime?
            if (endTime < route.depot.endBreakTime) 
            {
                endTime = route.depot.endBreakTime;
                /* Если при этом в ремонт можно войти позже, но до перерыва, то корректируем время входа в ремонт. */
                beginTime = endTime - type.duration;
                if (beginTime > route.depot.beginBreakTime)
                    beginTime = route.depot.beginBreakTime;
            }
        }
    }
}
