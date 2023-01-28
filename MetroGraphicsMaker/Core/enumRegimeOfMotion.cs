using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core
{
    /// <summary>
    /// 
    /// </summary>
    public enum RegimeOfMotion
    {
        /// <summary>
        /// По умолчанию (никакой).
        /// </summary>
        None = 0,

        /// <summary>
        /// Пик (режим движения в часы-"пик").
        /// </summary>
        Peak = 1,

        /// <summary>
        /// Непик (режим движения в часы-"долина").
        /// </summary>
        NonPeak,

        /// <summary>
        /// Переходные режимы движения.
        /// </summary>
        Transient
    }
}
