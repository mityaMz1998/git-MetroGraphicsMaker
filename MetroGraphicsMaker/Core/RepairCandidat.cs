using System;
using Converters;

namespace Core
{
    /// <summary>
    /// Описание кандидата для назначения на маршрут.
    /// </summary>
    public class RepairCandidate : ICloneable
    {
        /// <summary>
        /// 
        /// </summary>
        protected readonly UInt32 code;

        /// <summary>
        /// 
        /// </summary>
        protected static UInt32 maxCode;

        /// <summary>
        /// Время начала осмотра/ремонта в метросекундах.
        /// </summary>
        public readonly Int32 BeginTime;

        /// <summary>
        /// Место проведения осмотра/ремонта.
        /// </summary>
        public readonly InspectionPoint Point;

        /// <summary>
        /// 
        /// </summary>
        public UInt32 Code {
            get { return code; }
        }

        public RepairCandidate(Int32 beginTime, InspectionPoint point)
        {
            code = ++maxCode;

            BeginTime = beginTime;
            Point = point;

            if (Point != null) 
                Point.times.Add(BeginTime);
        }

        public override bool Equals(object obj)
        {
            var tmp = obj as RepairCandidate;
            if (tmp == null) return false;
            //return GetHashCode() == tmp.GetHashCode();

            return Point.Equals(tmp.Point) && (BeginTime == tmp.BeginTime);
        }

        public override int GetHashCode()
        {
            return Point.GetHashCode() + BeginTime;
        }

        public override string ToString()
        {
            return String.Format("{0} [{1}] / {2}", BeginTime, TimeConverter.SecondsToString(BeginTime), Point.name);
        }

        public object Clone()
        {
            return new RepairCandidate(BeginTime, Point);
        }
    }
}