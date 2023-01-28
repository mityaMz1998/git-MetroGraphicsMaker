using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;

namespace Core
{
    //Генерирует абстрактный хвост.
    public static class AbstractTailGiver
    {
        public enum NamesTails
        {
            /// <summary>
            /// Оборот поезда.
            /// </summary>
            LinkedTail,
            /// <summary>
            /// Оборот поезда пользовательский.
            /// </summary>
            LinkedTailCustom,

            /// <summary>
            /// Уход поезда.
            /// </summary>
            SingleTail,

            /// <summary>
            /// Уход поезда пользовательский.
            /// </summary>
            SingleTailCustom,

            /// <summary>
            /// Уход поезда на ночную расстановку.
            /// </summary>
            NightTail
        }

        public static AbstractTail CreateAbstractTail(NamesTails NameTail, string SecondaryName , TrainPath LeftLogicalTrainPath, int TimeBeginTail)
        {
            AbstractTail Abstract;

            switch (NameTail)
            {
                case NamesTails.LinkedTail:
                    Abstract = new LinkedTail();
                    break;
                case NamesTails.LinkedTailCustom:
                    Abstract = new LinkedTail();
                    break;
                case NamesTails.SingleTail:
                    Abstract = new SingleTail();
                    break;
                case NamesTails.SingleTailCustom:
                    Abstract = new SingleTail();
                    break;
                case NamesTails.NightTail:
                    Abstract = new NightTail();
                    break;
                default:
                    throw new System.ArgumentException("Unknown NamesTails (Неизвестное имя хвоста) in AbstractTailGiver class (CreateAbstractTail)", "original");
            }

            Abstract.NameTail = NameTail;
            Abstract.LeftLogicalTrainPath = LeftLogicalTrainPath;
            Abstract.TimeBeginTail = TimeBeginTail;
            Abstract.SecondName = SecondaryName;
            return Abstract;
        }
    }
}
