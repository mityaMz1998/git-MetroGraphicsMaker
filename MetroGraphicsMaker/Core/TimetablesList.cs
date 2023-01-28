using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    public class TimetablesList
    {
        protected List<Timetable> schedulesTable = new List<Timetable>();

        public void AddTimetable(Timetable item)
        {
            if (schedulesTable.Contains(item))
                throw new ArgumentException("An element with the same key already exists in the collection.");

            schedulesTable.Add(item);
        }

        public void AddRange(IEnumerable<Timetable> range)
        {
            foreach (var item in range)
                AddTimetable(item);
        }

        //public bool ContainsTableWithMode(MovementMode mode)
        //{
        //    if (mode == null)
        //        return false;

        //    schedulesTable.FirstOrDefault()
        //}

        /// <summary>
        /// Метод, пытающийся добавить элемент-расписание в коллекцию.
        /// </summary>
        /// <param name="item"></param>
        /// <returns>В случае удачи -- истина, ложь -- в противном случае.</returns>
        public bool TryAddTimetable(Timetable item)
        {
            if (schedulesTable.Contains(item))
                return false;

            schedulesTable.Add(item);
            return true;
        }

        public bool TryAddRange(IEnumerable<Timetable> range)
        {
            return range.Select(item => TryAddTimetable(item)).Aggregate((answer, tmpResult) => answer &= tmpResult);
        }

        public IEnumerable<Timetable> GetTimetablesByMode(Int32 id)
        {
            return schedulesTable.Where(tt => tt.Mode.id == id);
        }

        public IEnumerable<Timetable> GetTimetablesByMode(MovementMode mode)
        {
            if (mode == null)
                return Enumerable.Empty<Timetable>();

            return GetTimetablesByMode(mode.id);
        }

        public IEnumerable<Timetable> GetTimetablesByTask(UInt32 id)
        {
            return schedulesTable.Where(tt => tt.Task.code == id);
        }

        public IEnumerable<Timetable> GetTimetablesByTask(Task task)
        {
            if (task == null)
                return Enumerable.Empty<Timetable>();

            return GetTimetablesByTask(task.code);
        }

        public Timetable GetTimetable(Int32 modeId, UInt32 taskId)
        {
            return schedulesTable.FirstOrDefault(tt => tt.Mode.id == modeId && tt.Task.code == taskId);
        }

        public Timetable GetTimetable(MovementMode mode, Task task)
        {
            if (mode == null || task == null)
                return null;

            return GetTimetable(mode.id, task.code);
        }

        public bool ValidateByTask(Timetable arg)
        {
            return GetTimetablesByMode(arg.Mode).All(timetable => timetable.Task.Direction.Equals(arg.Task.Direction));
        }
    }
}
