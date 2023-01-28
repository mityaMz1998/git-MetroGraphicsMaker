using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Core
{
    /// <summary>
    /// Режим движения
    /// </summary>
    public class MovementMode : IEquatable<MovementMode>
    {
        /// <summary>
        /// Идентификатор режима движения.
        /// </summary>
        public Int32 id;

        /// <summary>
        /// Название режима движения.
        /// </summary>
        public String name;

        /// <summary>
        /// Выбранное направление
        /// </summary>
        public Direction direction;

        public MovementMode(DataRow row)
        {
            if (!Convert.IsDBNull(row["Код"]))
                id = Convert.ToInt32(row["Код"].ToString());

            if (!Convert.IsDBNull(row["Название"]))
                name = row["Название"].ToString();
        }


        //public void Initialize(DataRow row)
        //{
        //    if (!Convert.IsDBNull(row["Направление"]))
        //    {
        //        var localDirection = Convert.ToInt32(row["Направление"].ToString());
        //    }
        //}

        public bool Equals(MovementMode otherMode)
        {
            if (otherMode == null)
                return false;

            return id == otherMode.id;
        }
    }
}
