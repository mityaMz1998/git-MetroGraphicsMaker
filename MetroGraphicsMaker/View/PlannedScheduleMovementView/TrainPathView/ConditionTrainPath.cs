using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace View
{
    //Список состояний ниток в плане работы с ними пользователя.
    public enum ConditionTrainPath
    {
        /// <summary>
        /// Нитка БЕЗ выделения
        /// </summary>
        Free,

        /// <summary>
        /// Нитка с пользовательским выделением
        /// </summary>
        PrimarySelected,

        /// <summary>
        /// Нитка с связанная с выделенной ниткой.
        /// </summary>
        SecondarySelected,
        
    }
}
