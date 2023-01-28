using System;
using System.Data;
using System.Linq;

using Converters;
using Messages;

namespace Core
{
    /// <summary>
    /// Класс-обёртка для строк из таблицы БД "Таблица типов подвижного состава".
    /// </summary>
    public class TimeNorm
    {
        /// <summary>
        /// Код (идентификатор) в БД.
        /// </summary>
        public UInt32 code;

        /// <summary>
        /// Типа вагона. Является внешним ключом (для таблицы Типы подвижного состава).
        /// </summary>
        public WagonType wagonType;

        /// <summary>
        /// Тип (вид) ремонта. Является внешним ключом (для таблицы Типы ремонта).
        /// </summary>
        public RepairType repairType;

        /// <summary>
        /// Время, затрачиваемое для ремонта (осомтра) на один вагон.
        /// </summary>
        public Int32 time;

        /// <summary>
        /// 
        /// </summary>
        public Int32 period;

        /// <summary>
        /// 
        /// </summary>
        public Boolean isPeriodic = false;

        public TimeNorm(DataRow row)
        {
            code = Convert.ToUInt32(row["Код"].ToString());

            time = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время"].ToString()));

            if (!Convert.IsDBNull(row["Периодичность"]))
            {
                period = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Периодичность"].ToString()));
                isPeriodic = true;
            }
        }

        public void Initialize(DataRow row)
        { 
            var wagonTypeCode = Convert.ToUInt32(row["Тип вагонов"].ToString());
            wagonType = MovementSchedule.colWagonType.SingleOrDefault(t => t.code == wagonTypeCode);
            if (wagonType == null)
                Error.showErrorMessage(new WagonType() { code = wagonTypeCode }, this);

            var repairTypeCode = Convert.ToUInt32(row["Вид ремонта"].ToString());
            repairType = MovementSchedule.colRepairType.SingleOrDefault(t => t.code == repairTypeCode);
            if (repairType == null)
                Error.showErrorMessage(new RepairType() { code = repairTypeCode }, this);
        }
    }
}