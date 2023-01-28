namespace Messages
{
    public enum RepairState
    {
        /// <summary>
        /// Пустое значение (по умолчанию).
        /// </summary>
        NONE = 0,

        /// <summary>
        /// У маршрута уже предусмотрен ремонт.
        /// </summary>
        ALREADY_EXIST = 1,

        /// <summary>
        /// Нет возможности вставить ремонт для маршрута.
        /// </summary>
        CANNOT_INSERT = 2,

        /// <summary>
        /// Ремонт не является ТО-1.
        /// </summary>
        NOT_TO1 = 4,
    };
}