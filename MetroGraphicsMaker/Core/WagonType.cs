using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Core
{
    /// <summary>
    /// Класс-обёртка для строк из таблицы БД "Таблица типов подвижного состава".
    /// </summary>
    public class WagonType
    {
        /// <summary>
        /// Код (идентификатор) в БД.
        /// </summary>
        public UInt32 code;

        /// <summary>
        /// Название типа вагона.
        /// </summary>
        public String typeName;

        /// <summary>
        /// Нормы по времени на ремонты/осмотры данного типа подвижного состава.
        /// </summary>
        public List<TimeNorm> timeNorms; 

        public WagonType() { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="row"></param>
        public WagonType(DataRow row)
        {
            code = Convert.ToUInt32(row["Код"].ToString());

            typeName = row["Название"].ToString();
        }

        public void Initialize(DataRow row)
        {
            timeNorms = MovementSchedule.colTimeNorm.Where(tn => tn.wagonType == this).ToList();
        }
    }
}