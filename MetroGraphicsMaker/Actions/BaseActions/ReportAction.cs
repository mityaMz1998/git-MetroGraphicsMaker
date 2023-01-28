using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Actions
{
    //Список статусов-результатов, совершения операций.
    public enum ReportAction
    {
        /// <summary>
        /// Операция выполнена успешно
        /// </summary>
        Good,
        /// <summary>
        /// Операция закончилась ошибкой
        /// </summary>
        Bad
    }
}
