namespace Schedule
{
    /// <summary>
    /// Перечисление, описывающее тип линии -- обычная, монорельс или кольцевая.
    /// </summary>
    public enum LineType
    {
        /// <summary>
        /// Обычная линия
        /// </summary>
        DEFAULT = 0,
        /// <summary>
        /// Монорельс
        /// </summary>
        MONO = 1,
        /// <summary>
        /// Кольцевая линия
        /// </summary>
        RING = 2
    }
}