using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Core
{
    public class Timetable : IEquatable<Timetable>
    {
        /// <summary>
        /// Идентификатор (из БД) режима движения.
        /// </summary>
        public MovementMode Mode;

        /// <summary>
        /// Идентификатор (из БД) задания.
        /// </summary>
        public Task Task;

        /// <summary>
        /// Время хода по перегону.
        /// </summary>
        public Int32 MovingTime;

        /// <summary>
        /// Время стоянки на станции (прибытия).
        /// </summary>
        public Int32 StaingTime;

        public Timetable(DataRow row)
        { 
            if (!Convert.IsDBNull(row["Время хода"]))
                MovingTime = Converters.TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время хода"].ToString()));

            //if (!Convert.IsDBNull(row["Время стоянки"]))
            //    StaingTime = Converters.TimeConverter.TimeToSeconds(Convert.ToDateTime(row["Время стоянки"].ToString()));

            if (!Convert.IsDBNull(row["Задание"])) 
            { 
                var taskCode = Convert.ToUInt32(row["Задание"].ToString());
                Task = MovementSchedule.colTask.FirstOrDefault(task => task.code == taskCode);
            }

            if (Task == null)
                throw new Exception("Task");

            if (!Convert.IsDBNull(row["Номер варианта"]))
            {
                var modeCode = Convert.ToUInt32(row["Номер варианта"].ToString());
                Mode = MovementSchedule.colMovementMode.FirstOrDefault(mode => mode.id == modeCode);
            }

            if (Mode == null)
                throw new Exception("Mode");


            if (Task.Direction != null && Task.Direction.Tables != null && Task.Direction.Tables.ValidateByTask(this))
                Task.Direction.Tables.TryAddTimetable(this);
        }



        /// <summary>
        /// Метод сравнения двух (разных) объектов на эквивалентность одиного другому. 
        /// </summary>
        /// <param name="otherTimetable">Сравниваемый на эквивалентность данному объект.</param>
        /// <returns>Истина, если текщий объект и объект-параметр эквивалентны, ложь -- в противном случае.</returns>
        public bool Equals(Timetable otherTimetable)
        {
            if (otherTimetable == null)
                return false;

            return Mode.Equals(otherTimetable.Mode) && Task.Equals(otherTimetable.Task);
        }
    }
}
