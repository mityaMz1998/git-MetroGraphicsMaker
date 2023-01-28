namespace Messages
{
    public enum HungarianState
    {
        /// <summary>
        /// Пустое состояние (по умолчанию).
        /// </summary>
        NONE = 0,

        /// <summary>
        /// Не хватает времени и места для проведения всех ремонтов.
        /// </summary>
        NOT_ENOUGH_TIME_AND_SPACE = 1,

        /// <summary>
        /// Всё путём!
        /// </summary>
        ALL_GOOD = 2
    }
}