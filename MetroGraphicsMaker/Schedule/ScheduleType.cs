namespace Schedule
{
    /// <summary>
    /// Флаг, указывающий на основной/дополнительный график
    /// </summary>
    public enum ScheduleType
    {
        /// <summary>
        /// 0 - значение по умолчанию
        /// </summary>
        NONE = 0,

        /// <summary>
        /// 1 - оcновной
        /// </summary>
        BASE = 1,

        /// <summary>
        /// 2 - дополнительный
        /// </summary>
        ADDITIONAL = 2,

        /// <summary>
        /// 3 - вставка
        /// </summary>
        INSERT = 3
    }
}