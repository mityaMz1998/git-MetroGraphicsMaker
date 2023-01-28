/** 
 * @organization Departament "Management and Informatics in Technical Systems" (MITS@MIIT)
 * @author Konstantin Filipchenko (konstantin-649@mail.ru)
 * @version 0.1 
 * @date 2013-04-25
 * @description Source code based on clsZadanie.cls from GraphicPL@BASIC and Task.cs from GraphicPL@C#.
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
    /// Класс заданий.
    /// </summary>
    public class Task : IEquatable<Task>
    {
        /// <summary>
        /// Код задания в базе данных.
        /// </summary>
        public UInt32 code;

        /// <summary>
        /// Станция отправления.
        /// </summary>
        public Station departureStation;

        /// <summary>
        /// Станция назначения.
        /// </summary>
        public Station destinationStation;

        /// <summary>
        /// Направление задания
        /// </summary>
        public Direction Direction;

        /// <summary>
        /// Время хода для непараллельного графика
        /// </summary>
        public List<int?> ColTh = new List<int?>();

        /// <summary>
        /// Время стоянки для непараллельного графика
        /// </summary>
        public List<int?> ColTs = new List<int?>();

        /// <summary>
        /// Увеличение времени хода для первых поездов
        /// </summary>   

        /// <summary>
        /// Увеличение времени хода для первых поездов
        /// </summary>                       
        public int ThFirst;

        /// <summary>
        /// Минимальное время хода
        /// </summary>
        public int MinTh;

        /// <summary>
        /// Время хода по заданию в режимах "пик"
        /// </summary>
        public Int32 ThPeak;

        /// <summary>
        /// Время хода по заданию в режимах "не пик"
        /// </summary>
        public Int32 ThNonpeak;

        /// <summary>
        /// Время стоянки поезда на станции отправления в режиме "пик"
        /// </summary>
        public Int32 StatPeak;

        /// <summary>
        /// Время стоянки поезда на станции отправления в режиме "непик"
        /// </summary>
        public Int32 StatNonpeak;

        /// <summary>
        /// Время стоянки поезда на станции отправления в режиме "резерв"
        /// </summary>
        public int StatRez;

        /// <summary>
        /// Ссылка на следующее задание
        /// </summary>
        public Task nextTask;

        /// <summary>
        /// Ссылка на следующее задание, которое может быть оборотным
        /// </summary>
        public Task NextOborotZadanie;

        /// <summary>
        /// Длина задания
        /// </summary>
        public Single Length;

        public bool Equals(Task otherTask)
        {
            if (otherTask == null)
                return false;

            return code == otherTask.code;
        }

        /*
        <table name="Задания">
		  * <field name="Min_Time" type="Дата и время (datetime)" isPK="false"/>
		  * <field name="Время_Хода_Непик" type="Дата и время (datetime)" isPK="false"/>
		  * <field name="Время_Хода_Пик" type="Дата и время (datetime)" isPK="false"/>
		  * <field name="Длина_Задания" type="Одинарное с плавающей точкой (float)" isPK="false"/>
		  * <field name="Добавочное_время_для_первых_поездов" type="Дата и время (datetime)" isPK="false"/>
		  * <field name="Код" type="Длинное целое (long)" isPK="false"/>
		  * <field name="Код_Задания" type="Длинное целое (long)" isPK="true"/>
		  * <field name="Код_Следующего_Задания" type="Целое (int)" isPK="false"/>
		  * <field name="Код_Станции_Отпр" type="Длинное целое (long)" isPK="false"/>
		  * <field name="Код_Станции_Приб" type="Длинное целое (long)" isPK="false"/>
		  * <field name="Направление" type="Длинное целое (long)" isPK="false"/>
		    <field name="Примечание" type="Текст (text)" isPK="false"/>
		    <field name="Станция отпр" type="Длинное целое (long)" isPK="false"/>
		    <field name="Станция приб" type="Длинное целое (long)" isPK="false"/>
		  * <field name="Стоянка_Непик" type="Дата и время (datetime)" isPK="false"/>
		  * <field name="Стоянка_Пик" type="Дата и время (datetime)" isPK="false"/>
		  * <field name="Стоянка_Резерв" type="Целое (int)" isPK="false"/>
	    </table>
        */

        public Task() { }

        public Task(DataRow row)
        {
            code = Convert.ToUInt32(row["Код_Задания"].ToString());

            ThNonpeak = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_Хода_Непик"].ToString()));
            StatNonpeak = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Стоянка_Непик"].ToString()));
            ThPeak = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_Хода_Пик"].ToString()));
            StatPeak = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Стоянка_Пик"].ToString()));

            ThFirst = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Добавочное_время_для_первых_поездов"].ToString()));


            Length = Convert.ToSingle(row["Длина_Задания"].ToString());
            MinTh = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Min_Time"].ToString()));

            //примечание          
            //если стоянка резерв равна ноль, а не 0:0:0, то присваиваю 0
            StatRez = (row["Стоянка_Резерв"].ToString().Equals("0")) ? 0 : TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Стоянка_Резерв"].ToString()));

        }

        public void Initialize(DataRow row)
        {
            var stationCode = Convert.ToUInt32(row["Код_Станции_Отпр"].ToString());
            departureStation = MovementSchedule.colStation.SingleOrDefault(s => s.code == stationCode);

            if (departureStation == null)
                Error.showErrorMessage(new Station() { code = stationCode }, this);

            stationCode = Convert.ToUInt32(row["Код_Станции_Приб"].ToString());
            destinationStation = MovementSchedule.colStation.SingleOrDefault(s => s.code == stationCode);

            if (destinationStation == null)
                Error.showErrorMessage(new Station() { code = stationCode }, this);

            UInt32 taskCode;
            if (!Convert.IsDBNull(row["Код_Следующего_Задания"]))
            {
                taskCode = Convert.ToUInt32(row["Код_Следующего_Задания"].ToString());
                nextTask = MovementSchedule.colTask.SingleOrDefault(t => t.code == taskCode);
                if (nextTask == null)
                    Error.showErrorMessage(new Task { code = taskCode }, this);
            }

            if (!Convert.IsDBNull(row["Код_Оборотного_Задания"]))
            {
                taskCode = Convert.ToUInt32(row["Код_Оборотного_Задания"].ToString());
                NextOborotZadanie = MovementSchedule.colTask.SingleOrDefault(t => t.code == taskCode);
                if (NextOborotZadanie == null)
                    Error.showErrorMessage(new Task { code = taskCode }, this);
            }


            switch ((DirectionValue)(Convert.ToInt32(row["Направление"].ToString())))
            {
                case DirectionValue.EVEN: Direction = departureStation.line.evenDirection;
                    break;
                case DirectionValue.NONE: Direction = departureStation.line.noneDirection;
                    break;
                case DirectionValue.ODD: Direction = departureStation.line.oddDirection;
                    break;
            }

        }

        /// <summary>
        /// Определение времени хода по перегону
        /// </summary>
        /// <param name="isPeak">Флаг, указывающий размер движения -- пик/непик.</param>
        /// <param name="isFirst">Флаг, указывающий является ли данный маршрут одним из первых (необходимо ли увеличение времени хода).</param>
        /// <param name="i">Вроде вообще не нужна</param>
        /// <returns>Время в "метрополитеновских секундах".</returns>
        public int toTH(Boolean isPeak, Boolean isFirst, int i = 0)
        {
            return (isPeak ? ThPeak : ThNonpeak) + (isFirst ? ThFirst : 0);
        }

        /// <summary>
        /// Определение времени стоянки на станции
        /// </summary>
        /// <param name="isPeak">Флаг, указывающий размер движения -- пик/непик</param>
        /// <returns>Время в "метрополитеновских секундах".</returns>
        public int toTStat(Boolean isPeak)
        {
            return isPeak ? StatPeak : StatNonpeak;
        }

        public override string ToString()
        {
            return String.Format("{0} ---> {1}", departureStation.name, destinationStation.name);
        }
    }
}
