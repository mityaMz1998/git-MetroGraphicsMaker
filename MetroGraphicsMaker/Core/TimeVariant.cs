using System;
using Converters;

namespace Core
{
    [Flags]
    public enum TimeVariantType
    {
        TOTAL,

        LEFT,

        RIGHT
    }

    public class IntervalToPointInMovementException : Exception
    {

    }

    public struct TimeVariant
    {
        /// <summary>
        /// Минимально допустимое время (левая граница).
        /// </summary>
        public Int32 Tmin;
        
        /// <summary>
        /// Желаемое время (приблизительно то, в которое должен быть ремонт).
        /// </summary>
        public Int32 Tdes;

        /// <summary>
        /// Максимально допустимое время (правая граница).
        /// </summary>
        public Int32 Tmax;

        /// <summary>
        /// Флаг, показывающий стянут ли данный интервал в точку (првая граница равна левой).
        /// </summary>
        public Boolean IsPoint;

        /// <summary>
        /// 
        /// </summary>
        public TimeVariantType TypeOfInterval;

        public TimeVariant(Int32 minimalTime = -1, Int32 desiredTime = -1, Int32 maximalTime = -1, TimeVariantType type = TimeVariantType.TOTAL)
        {
            Tmin = minimalTime < 0 ? 0 : minimalTime;
            Tdes = desiredTime < 0 ? 0 : desiredTime;
            Tmax = maximalTime < 0 ? 0 : maximalTime;
            IsPoint = (Tmin == Tdes && Tdes == Tmax);

            TypeOfInterval = type;

            if (IsPoint && TimeConverter.MovementTime.isInto(Tdes, true))
                throw new IntervalToPointInMovementException();


            //if (IsPoint)
            //    throw new ArgumentException("Empty timespan");

            if (Tmax < Tdes || Tdes < Tmin || Tmax < Tmin)
                throw new ArgumentException(String.Format("Unposible timespan. Minimal time can not be greater then Desired and Desired time can not be greater then Maximal. {0}", this));
        }

        public Boolean IsInto(Int32 timeValue)
        {
            return Tmin <= timeValue && timeValue <= Tmax;
        }


        public override string ToString()
        {
            return String.Format("Tmin = {0:D5} [{1}]; Tdes = {2:D5} [{3}]; Tmax = {4:D5} [{5}];", Tmin, TimeConverter.SecondsToString(Tmin), Tdes, TimeConverter.SecondsToString(Tdes), Tmax, TimeConverter.SecondsToString(Tmax));
        }

       
    }
}