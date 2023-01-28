/** 
 * @organization Departament "Management and Informatics in Technical Systems" (MITS@MIIT)
 * @author Konstantin Filipchenko (konstantin-649@mail.ru)
 * @version 0.1 
 * @date 2013-03-25
 * @description Source code based on clsTypRemont.cls from GraphicPL@BASIC.
 */

using System;
using System.Data;

using Converters;
/** 
 * Данный класс является законченным относительно своей функциональности. Полное соответствие с аналогом на VB подтверждаю.
 * 17 июля 2013 года 11:00 Филипченко.
 */
using Messages;

namespace Core
{
    /// <summary>
    /// Класс, описывающий сущность Тип ремонта.
    /// </summary>
    public class RepairType : Entity
    {
        /// <summary>
        /// Код (идентификатор) данного типа ремонта в БД.
        /// </summary>
        public UInt32 code;

        /// <summary>
        /// Название данного типа ремонта.
        /// </summary>
        public String name;

        /// <summary>
        /// Флаг, показывающий является ли депо местом проведения данного типа ремонта.
        /// </summary>
        public Boolean isIntoDepot;

        /// <summary>
        /// Флаг, показывающий является ли данный тип ремонта периодическим.
        /// </summary>
        public Boolean isPeriodical;

        /// <summary>
        /// Периодичность, с которой должен проводиться данный тип ремонта.  Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Int32 period;

        /// <summary>
        /// Длительность (продолжительность по времени) данного типа ремонта.  Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
        public Int32 duration;

        /// <summary>
        /// Рекомендуемое время начала данного типа ремонта.  Измеряется в количестве временных интервалов ("метрополитеновских" секунд), выбранных согласно масштабу времени (в данный момент временной интервал равен 5 (пяти) секундам, см. Converters.TimeConverter.timeScale - константное (статическое) поле статического класса TimeConverter, размещённого в пространстве имён Converters).
        /// </summary>
         public Int32 recomendedBeginTime;

        /*
         * Все поля должны быть!!!
         * Но по итогам работы с соответствующей таблицей БД и кодом на VB можно придти к выводу, что поле Периодичность не является обязательным и может иметь значение DBNull.
         * 17 июля 2013 года 11:00 Филипченко.
          <table name="Таблица типов ремонта">
         *  <field name="Длительность"               type="datetime" isPK="false" null="true" />
         *  <field name="Код"                        type="int"      isPK="true"  null="false" />
         *  <field name="Место_проведения_депо"      type="bit"      isPK="false" null="false" />
         *  <field name="Название"                   type="text"     isPK="false" null="true" />
         *  <field name="Периодичность"              type="datetime" isPK="false" null="true" />
         *  <field name="Признак_Периодического"     type="bit"      isPK="false" null="false" />
         *  <field name="Рекомендуемое время начала" type="datetime" isPK="false" null="true" />
          </table>
        */

        public RepairType() { }

        public RepairType(DataRow row)
        {
            code = Convert.ToUInt32(row["Код"].ToString());

            name = row["Название"].ToString();

            isIntoDepot = Convert.ToBoolean(row["Место_проведения_депо"].ToString());

            isPeriodical = Convert.ToBoolean(row["Признак_Периодического"].ToString());

            if (!Convert.IsDBNull(row["Периодичность"]))
                period = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Периодичность"].ToString()));

            duration = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Длительность"].ToString()));
            
            if (!Convert.IsDBNull(row["Рекомендуемое время начала"]))
                recomendedBeginTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Рекомендуемое время начала"].ToString()));
        }

        public override String ToString()
        {
            return String.Format("Тип ремонта '{1}' (код в БД = {2}) {4}проводится в депо[{3}] - периодичный [{0}] время начала = {5} ({6})", 
                isPeriodical ? "X" : " ", name, code, isIntoDepot ? "X" : " ", isPeriodical ? "периодически " : "", recomendedBeginTime, TimeConverter.SecondsToString((recomendedBeginTime)));
        }
    }
}
