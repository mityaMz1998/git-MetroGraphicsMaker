/** 
 * @organization Departament "Management and Informatics in Technical Systems" (MITS@MIIT)
 * @author Konstantin Filipchenko (@e-mail konstantin-649@mail.ru)
 * @version 1.0
 * @date 2013-05-28
 * @description
 */

using System;

namespace Converters
{
    /// <summary>
    /// Структура, описывающая интервал времени.
    /// </summary>
    public class TimeInterval
    {
        /// <summary>
        /// Начало временного интервала в "метрополитеновских" секундах (число временнЫх интервалов).
        /// </summary>
        private Int32 _begin;
        
        /// <summary>
        /// Конец временного интервала в "метрополитеновских" секундах (число временнЫх интервалов).
        /// </summary>
        private Int32 _end;

        /// <summary>
        /// Шаг, с которым "режем" верменной интервал на целочисленные куски. По умолчанию - 1 "метрополитеновская" секунда.
        /// </summary>
        private Int32 _step = 1;

        /// <summary>
        /// Длительность временного интервала в (полном) количестве шагов.
        /// </summary>
        private Int32 _duration;

        /// <summary>
        /// Пересчитывает длительность временного интервала в (полном) количестве шагов.
        /// </summary>
        private void _recalculateDuration() { _duration = _end - _begin; }

        /// <summary>
        /// Начало временного интервала в "метрополитеновских" секундах (число временнЫх интервалов).
        /// </summary>
        public Int32 begin { get { return _begin; } set { _begin = value; _recalculateDuration(); } }

        /// <summary>
        /// Конец временного интервала в "метрополитеновских" секундах (число временнЫх интервалов).
        /// </summary>
        public Int32 end { get { return _end; } set { _end = value; _recalculateDuration(); } }

        /// <summary>
        /// Шаг, с которым "режем" верменной интервал целочисленные куски.
        /// </summary>
        public Int32 step { get { return _step; } set { _step = value; _recalculateDuration(); } }

        /// <summary>
        /// Длительность временного интервала.
        /// </summary>
        public Int32 duration { get { return _duration; } }


        public TimeInterval Clone()
        {
            return  new TimeInterval(_begin, _end);
        }

        public TimeInterval(Int32 start, Int32 finish)
        {
            begin = Math.Min(start, finish);
            end   = Math.Max(start, finish);
            /*
            if (start <= finish)
            {
                begin = start;
                end = finish;
            }
            else 
            {
                begin = finish;
                end = start;
            }
            */
        }


        /// <summary>
        /// Длительность временного интервала в (полном) количестве шагов.
        /// </summary>
        public Int32 durationByStep(Int32 step = 0) 
        { return (step != 0) ? (_duration / step) : (_duration / _step); }
        //TODO: Добавить метод сравнения временных интервалов и проверки на ложения и пересечения!!!

        /// <summary>
        /// Метод, проверяющий попадает ли указанное значение (value) внуть данного интевала.
        /// </summary>
        /// <param name="value">Некоторое интересующее нас значение.</param>
        /// <param name="withoutBounds">Флаг, показывающий учитываются границы интервалов или нет. По умолчанию - нет.</param>>
        /// <returns>Попадает или нет внутрь данного интервала.</returns>
        public Boolean isInto(Int32 value, Boolean withoutBounds = false)
        {
            if (withoutBounds) 
                return (_begin < value && value < _end);
            return (_begin <= value && value <= _end);
        }


        public Boolean isIntoLeftClosedInterval(Int32 value)
        {
            return (_begin <= value && value < _end);
        }

        public Boolean isIntoRightClosedInterval(Int32 value)
        {
            return (_begin < value && value <= _end);
        }

        public Boolean isIntoBothClosedInterval(Int32 value)
        {
            return (_begin <= value && value <= _end);
        }

        public Boolean isIntoBothOpenedInterval(Int32 value)
        {
            return (_begin < value && value < _end);
        }

        /// <summary>
        /// Метод, проверяющий пересекаются ли данные интервалы или нет.
        /// </summary>
        /// <param name="other">Второй интервал, для которого проверяем наличие пересечения.</param>
        /// <returns>Пересекаются или нет.</returns>
        public Boolean isIntersect(TimeInterval other)
        {
            return isIntoBothOpenedInterval(other.begin) || isIntoBothOpenedInterval(other.end);
            // return (isInto(other.begin) || isInto(other.end));
        }

        /// <summary>
        /// Метод, проверяющий содержится ли интервал other в данном интервале.
        /// </summary>
        /// <param name="other">Другой интервал, для которого проверяем вхождение в данный.</param>
        /// <returns>Содержится или нет.</returns>
        public Boolean isContain(TimeInterval other)
        {
           // return (_begin <= other.begin && other.end <= _end);
            return (isInto(other.begin) && isInto(other.end));
        }

        public override String ToString()
        {
            return String.Format("[{0} : {1}]", TimeConverter.SecondsToString(_begin), TimeConverter.SecondsToString(_end));
        }
    }
}
