/** 
 * @organization Departament "Management and Informatics in Technical Systems" (MITS@MIIT)
 * @author Konstantin Filipchenko (konstantin-649@mail.ru)
 * @version 0.1 
 * @date 2013-03-25
 * @description Source code based on clsElementOfSchedule.cls from GraphicPL@BASIC and clsElementOfSchedule.cs from GraphicPL@C#.
 */

using System;
using System.Data;
using System.Linq;

using Converters;

namespace Core
{
    public class clsElementOfSchedule
    {
        /// <summary>
        /// Код элемента расписания в базе данных
        /// </summary>
        public UInt32 code = 0;

        /// <summary>
        /// Ссылка на нитку, которой принадлежит данный элемент расписания
        /// </summary>
        public String CodeNitki = "";

        /// <summary>
        /// Ссылка на задание в этом элементе расписания
        /// </summary>
        public Task task;

        /// <summary>
        /// Время отправления
        /// </summary>
        public Int32 departureTime = 0;

        /// <summary>
        /// Время прибытия. Только для оборота, когда направление NONE (едем в обе стороны).
        /// </summary>
        public Int32 arrivalTime;

        /// <summary>
        /// Сверхнормативная стоянка на станции, введенная по отправлению
        /// </summary>
        public Int32 TimeStoyanOtprav;

        /// <summary>
        /// Сверхнормативная стоянка на станции, введенная по прибытию
        /// </summary>
        public Int32 TimeStoyanPribyt;

        /// <summary>
        /// Ссылка на элемент расписания, который будет выполняться поездом после выполнения текущего
        /// </summary>
        public Int32 CodeNext;

        /// <summary>
        /// Примечание, в случае надобности указывается, что этот элемент расписания не брать
        /// </summary>
        public String note;

        /// <summary>
        /// Cверхрежимная выдержка на станции отправления
        /// </summary>
        public clsSRV SRV = null;//new clsSRV();
        //Public SRV As clsSRV  

        public Boolean flgChangeInterval = false;

        public clsElementOfSchedule() { }

        public clsElementOfSchedule(Int32 _arrivalTime, int _TimeStoyanOtprav, int _departureTime, Task _Task)
        {
            task = _Task;
            arrivalTime = _arrivalTime;
            TimeStoyanOtprav = _TimeStoyanOtprav;
            departureTime = _departureTime;
        }

        public clsElementOfSchedule(clsElementOfSchedule element, Int32 DepartureTimeFromFirstStation)
        {
            task = element.task;
            arrivalTime = DepartureTimeFromFirstStation + element.arrivalTime;
            TimeStoyanOtprav = element.TimeStoyanOtprav;
            departureTime = DepartureTimeFromFirstStation + element.departureTime;
        }

        public clsElementOfSchedule(DataRow row)
        {
            code = Convert.ToUInt32(row["Код_Элемента_Расписания"].ToString());

            CodeNitki = row["Код_Нитки"].ToString();

            note = row["Примечание"].ToString();
        }

        /// <summary>
        /// Инициализация объекта (построение связей с другими оюъектами) по строке данных виртуальной таблицы БД.
        /// </summary>
        /// <param name="row">Строка виртуальной таблицы БД.</param>
        public void Initialize(DataRow row)
        {
            //В этой таблице если какого-то парамтра нет, ему присваивается значение null

            //task = mdlData.colTask[Convert.ToInt32(row["Код_Задания"].ToString()) - 1];
            task = MovementSchedule.colTask.SingleOrDefault(t => t.code == Convert.ToInt32(row["Код_Задания"].ToString()));

            //if (row["Время_Отправления"] is DBNull)
            //{
            //    TimeOtprav = null;
            //}
            //else
            //{
            //    TimeOtprav = mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(row["Время_Отправления"].ToString()));
            //}   

            if (task == null)
                return;

            if (!Convert.IsDBNull(row["Время_Отправления"]))
            {
                departureTime =
                    TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_Отправления"].ToString())); /* - 
                    (
                        (Convert.ToInt32(CodeNitki)
                        + (int)code
                        + (int)(task.departureStation.code)
                        + (int)(task.destinationStation.code)
                        )
                    % 100);
                                                                                                           * */
            }

            //if (!(row["Время_Прибытия"] is DBNull))
            //{
            //    TimePribyt = null;
            //}
            //else
            //{
            //    TimePribyt = mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(row["Время_Прибытия"].ToString()));
            //}

            if (!Convert.IsDBNull(row["Время_Прибытия"]))
            {
                arrivalTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время_Прибытия"].ToString())) - ((Convert.ToInt32(CodeNitki) + (int)code + (int)(task.departureStation.code) + (int)(task.destinationStation.code)) % 100);
            }
            else
            {
                arrivalTime = 0;
            }

            //if (row["Стоянка_Отправления"] is DBNull)
            //{
            //    TimeStoyanOtprav = null;
            //}
            //else
            //{
            //    TimeStoyanOtprav = mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(row["Стоянка_Отправления"].ToString()));
            //}

            //if (row["Cтоянка_Прибытия"] is DBNull)
            //{
            //    TimeStoyanPribyt = null;
            //}
            //else
            //{
            //    TimeStoyanPribyt = mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(row["Cтоянка_Прибытия"].ToString()));
            //}

            /*
			if (!(row["Cтоянка_Прибытия"] is DBNull))
			{
				if (SRV == null)
					SRV = new clsSRV();
				SRV.toInc(false, (mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(row["Cтоянка_Прибытия"].ToString()))));
			}
			if (!(row["Стоянка_Отправления"] is DBNull))
			{
				if (SRV == null)
					SRV = new clsSRV();
				SRV.toInc(true, (mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(row["Стоянка_Отправления"].ToString()))));
			}
            */
            //if (row["Код_Следующего_Элемента_Расписания"] is DBNull)
            //{
            //    CodeNext = null;
            //}
            //else
            //{
            //    CodeNext = Convert.ToInt32(row["Код_Следующего_Элемента_Расписания"].ToString());
            //}                      
        }


        /*
        public void Initialize(DataTable Tab, int id)
        {//В этой таблице если какого-то парамтра нет, ему присваивается значение null
            this.code = Convert.ToInt32(Tab.Rows[id]["Код_Элемента_Расписания"].ToString());           
            this.CodeNitki = (Tab.Rows[id]["Код_Нитки"].ToString());
            this.Zadanie = mdlData.colTask[Convert.ToInt32(Tab.Rows[id]["Код_Задания"].ToString()) - 1];
            //if (Tab.Rows[id]["Время_Отправления"] is DBNull)
            //{
            //    this.TimeOtprav = null;
            //}
            //else
            //{
            //    this.TimeOtprav = mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(Tab.Rows[id]["Время_Отправления"].ToString()));
            //}                       
            if (!(Tab.Rows[id]["Время_Отправления"] is DBNull))
            {
                this.TimeOtprav = mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(Tab.Rows[id]["Время_Отправления"].ToString())) -
                    ((Convert.ToInt32(this.CodeNitki) + this.code + this.Zadanie.StanOtpr.code + this.Zadanie.StanPrib.code) % 100);
            }

            //if (!(Tab.Rows[id]["Время_Прибытия"] is DBNull))
            //{
            //    this.TimePribyt = null;
            //}
            //else
            //{
            //    this.TimePribyt = mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(Tab.Rows[id]["Время_Прибытия"].ToString()));
            //}
            if (!(Tab.Rows[id]["Время_Прибытия"] is DBNull))
            {
                this.TimePribyt = mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(Tab.Rows[id]["Время_Прибытия"].ToString())) -
                    ((Convert.ToInt32(this.CodeNitki) + this.code + this.Zadanie.StanOtpr.code +
                    this.Zadanie.StanPrib.code) % 100);
            }
            else
                this.TimePribyt = null;

            //if (Tab.Rows[id]["Стоянка_Отправления"] is DBNull)
            //{
            //    this.TimeStoyanOtprav = null;
            //}
            //else
            //{
            //    this.TimeStoyanOtprav = mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(Tab.Rows[id]["Стоянка_Отправления"].ToString()));
            //}
            //if (Tab.Rows[id]["Cтоянка_Прибытия"] is DBNull)
            //{
            //    this.TimeStoyanPribyt = null;
            //}
            //else
            //{
            //    this.TimeStoyanPribyt = mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(Tab.Rows[id]["Cтоянка_Прибытия"].ToString()));
            //}
            if (!(Tab.Rows[id]["Cтоянка_Прибытия"] is DBNull))
            {
                if (this.SRV == null)
                    SRV = new clsSRV();
                this.SRV.toInc(false, (mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(Tab.Rows[id]["Cтоянка_Прибытия"].ToString()))));
            }
            if (!(Tab.Rows[id]["Стоянка_Отправления"] is DBNull))
            {
                if (this.SRV == null)
                    SRV = new clsSRV();
                this.SRV.toInc(true, (mdlLocalGraphicProcedures.TimeToSecond(Convert.ToDateTime(Tab.Rows[id]["Стоянка_Отправления"].ToString()))));
            }
            
            //if (Tab.Rows[id]["Код_Следующего_Элемента_Расписания"] is DBNull)
            //{
            //    this.CodeNext = null;
            //}
            //else
            //{
            //    this.CodeNext = Convert.ToInt32(Tab.Rows[id]["Код_Следующего_Элемента_Расписания"].ToString());
            //}          
            //this.note = Tab.Rows[id]["Примечание"].ToString();                           
        }
         */

        /// <summary>
        /// Длительность сверхрежимной выдержки на станции отправления (никогда не будет null)
        /// </summary>
        /// <returns></returns>
        public int GetTimeStop()
        {
            return (SRV != null) ? (SRV.dTStopDeparture + SRV.dTStopArrival) : 0;

            /* 
            // Использование new int()
            // http://stackoverflow.com/questions/5746873/where-and-why-use-int-a-new-int
            // Общий смысл -- new int() и default(int) тождествены.
            // Если учесть, что значение по умолчанию 0, то и присвоение нуля
            // также будет иметь эквивалентный эффект -- в переменную будет помещено
            // значение нуль.
            // Всё это из-за того, что int в отличие от int? не является Nullable<T> типом.
             */
        }

    }
}
