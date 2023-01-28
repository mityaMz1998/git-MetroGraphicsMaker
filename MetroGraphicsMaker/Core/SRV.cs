using System;

namespace Core
{
    /// <summary>
    /// Класс сверхрежимной выдержки
    /// </summary>
    public class clsSRV
    {
        /// <summary>
        /// Длительность сверхрежимной выдержки, изменяющей время отправления со станции                                
        /// </summary>
        public Int32 dTStopDeparture; //durationStopForDeparture
        /// <summary>
        /// Длительность сверхрежимной выдержки, изменяющей время прибытия на станцию
        /// </summary>
        public Int32 dTStopArrival; //durationStopForArrival

        /// <summary>
        /// 
        /// </summary>
        /// <param name="isDeparture"></param>
        /// <param name="time"></param>
        public void toInc(Boolean isDeparture, Int32 time)
        {
            if (isDeparture)
                dTStopDeparture += time;
            else
                dTStopArrival += time;
        }
    }
}
