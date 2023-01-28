namespace Schedule
{
    /// <summary>
    /// Перечисление, описывающее тип ПГД -- на рабочие или выходные дни.
    /// </summary>
    public enum ScheduleDayType
    {   
        /// <summary>
        /// Значение по умолчанию (ошибка)
        /// </summary>
        NONE = 0,
        /// <summary>
        /// График на рабочие дни
        /// </summary>
        WORK = 1,
        /// <summary>
        /// График на выходные дни
        /// </summary>
        WEEKEND = 2
    }
}