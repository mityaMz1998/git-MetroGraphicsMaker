using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using View;

namespace Actions
{
    //Реализует логику выделения нитки, изменения ее цвета.
    static class DeselectSecondaryTrainPathEditor
    {
        /// <summary>
        /// Девыделяет вторично выделенную нитку.
        /// </summary>
        /// <param name="SecondaryTrainPath">Вторично выделенная нитка.</param>
        private static void DeselectSecondaryTrainPath(TrainPath SecondaryTrainPath)
        {
            //Проверяет действительно ли данная нитка выделена вторично.
            if (SecondaryTrainPath.Condition == ConditionTrainPath.SecondarySelected)
            {
                //Если нитка выделена вторично, то задает у нитки FreeTrainPath параметры как у свободной.
                SecondaryTrainPath.Condition = ConditionTrainPath.Free;
                SecondaryTrainPath.MasterTrainPaths.SecondarySelectedTrainPaths.Remove(SecondaryTrainPath);
                SecondaryTrainPath.InvalidateVisual();

                //Задает параметры у хвоста нитки FreeTrainPath как у свободной.
                if (SecondaryTrainPath.TailTrainPath != null)
                {
                    //Если нитка не выделена первично, то происходит исключение.
                    DeselectSecondaryTailTrainPath(SecondaryTrainPath.TailTrainPath);
                }
            }
            else
            {
                //Если нитка не выделена вторично, то происходит исключение.
                throw new System.ArgumentException("Для девыделения нитки ее параметр FreeTrainPath.Condition должен быть равен SecondarySelected", "original");
            }
        }

        /// <summary>
        /// Девыделяет выделенный вторично хвост.
        /// </summary>
        /// <param name="SecondaryTailTrainPath">Выделенный вторично хвост</param>
        private static void DeselectSecondaryTailTrainPath(ViewTail SecondaryTailTrainPath)
        {
            //Проверяет выделен ли вторично ли хвост.
            if (SecondaryTailTrainPath.Condition == ConditionTrainPath.SecondarySelected)
            {
                //Если выделен вторично то задает его параметры как у свободного.
                SecondaryTailTrainPath.Condition = ConditionTrainPath.Free;

                SecondaryTailTrainPath.InvalidateVisual();
            }
            else
            {
                //Если не выделен вторично, то генерирует исключение.
                throw new System.ArgumentException("Для девыделения хвоста нитки ее параметр FreeTrainPath.Condition должен быть равен SecondarySelected", "original");
            }
        }
        
        /// <summary>
        /// Девыделяет предыдущеие нитки, относительно данной свободной.
        /// </summary>
        /// <param name="FreeTrainPath">Свободная нитка.</param>
        private static void DeselectSecondaryBackTrainPath(TrainPath FreeTrainPath)
        {
            //Инициирует цикл для продвижения по всем Back ниткам, связанным с данной.
            TrainPath thTrainPath = FreeTrainPath;
            while (thTrainPath.LogicalTrainPath.BackThread != null)
            {
                //Переход к новой выделенной вторично нитке.
                thTrainPath = thTrainPath.LogicalTrainPath.BackThread.ViewTrainPath;
                //Девыделение выделенной вторично нитки и ее хвоста.
                DeselectSecondaryTrainPath(thTrainPath);
            }
        }

        /// <summary>
        /// Девыделяет последующие нитки, относительно данной свободной.
        /// </summary>
        /// <param name="FreeTrainPath">Свободная нитка.</param>
        private static void DeselectSecondaryNextTrainPath(TrainPath FreeTrainPath)
        {
            //Инициирует цикл для продвижения по всем Next ниткам, связанным с данной.
            TrainPath thTrainPath = FreeTrainPath;
            while (thTrainPath.LogicalTrainPath.NextThread != null)
            {
                //Переход к новой выделенной вторично нитке.
                thTrainPath = thTrainPath.LogicalTrainPath.NextThread.ViewTrainPath;
                //Девыделение выделенной вторично нитки и ее хвоста.
                DeselectSecondaryTrainPath(thTrainPath);
            }
        }

        /// <summary>
        /// Девыделяет все выдеенные вторично нитки связнанные с данной.
        /// </summary>
        /// <param name="FreeTrainPath">Свободная нитка.</param>
        public static void DeselectSecondaryAllRelatedTrainPath(TrainPath FreeTrainPath)
        {
            //Пробегает по всем предыдущим ниткам относительно данной свободной и освобождает эти нитки.
            DeselectSecondaryBackTrainPath(FreeTrainPath);
            //Пробегает по всем последующим ниткам относительно данной свободной и освобождает эти нитки.
            DeselectSecondaryNextTrainPath(FreeTrainPath);
        }
    }
}