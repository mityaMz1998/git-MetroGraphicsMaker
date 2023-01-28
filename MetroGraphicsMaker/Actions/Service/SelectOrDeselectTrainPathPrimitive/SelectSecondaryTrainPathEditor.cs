using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using View;

namespace Actions
{
    //Реализует логику выделения нитки, изменения ее цвета.
    static class SelectSecondaryTrainPathEditor
    {
        /// <summary>
        /// Выделяет нитку как вторичную.
        /// </summary>
        /// <param name="FreeTrainPath">Свободная нитка.</param>
        private static void SelectSecondaryTrainPath(TrainPath FreeTrainPath)
        {
            //Проверяет действительно ли свободна нитка.
            if (FreeTrainPath.Condition == ConditionTrainPath.Free)
            {
                //Если нитка действительно свободна, то задает ей соответствующие параметры. (Как выделенную вторично)
                FreeTrainPath.Condition = ConditionTrainPath.SecondarySelected;
                FreeTrainPath.MasterTrainPaths.SecondarySelectedTrainPaths.Add(FreeTrainPath);
                FreeTrainPath.InvalidateVisual();

                //Задает хвосту данной нитки вторичное выделение.
                if (FreeTrainPath.TailTrainPath != null)
                {
                    SelectSecondaryTailTrainPath(FreeTrainPath.TailTrainPath);
                }
            }
            else
            {
                //Если нитка выделена, то генерирует исключение.
                throw new System.ArgumentException("Для выделения нитки Secondary ее параметр FreeTrainPath.Condition должен быть равен Free", "original");
            }
        }

        /// <summary>
        /// Выеделяет вторично хвост.
        /// </summary>
        /// <param name="FreeTailTrainPath">Свободный хвост.</param>
        private static void SelectSecondaryTailTrainPath(ViewTail FreeTailTrainPath)
        {
            //Проверяет действительно ли свободен хвост.
            if (FreeTailTrainPath.Condition == ConditionTrainPath.Free)
            {
                //Если хвост свободен, то задает ему параметры, как занятому вторично.
                FreeTailTrainPath.Condition = ConditionTrainPath.SecondarySelected;

                FreeTailTrainPath.InvalidateVisual();
            }
            else
            {
                //Если хвост занят, то генерирует исключение.
                throw new System.ArgumentException("Для выделения хвоста нитки Secondary ее параметр FreeTrainPath.Condition должен быть равен Free", "original");
            }
        }

        /// <summary>
        /// Выделяет предыдущеие нитки как вторичные, относительно данной первичной.
        /// </summary>
        /// <param name="PrimaryTrainPath">Первично выделенная нитка.</param>
        private static void SelectSecondaryBackTrainPath(TrainPath PrimaryTrainPath)
        {
            //Инициирует цикл для продвижения по всем Back ниткам, связанным с данной.
            TrainPath thTrainPath = PrimaryTrainPath;
            while (thTrainPath.LogicalTrainPath.BackThread != null)
            {
                //Переход к новой свободной нитке.
                thTrainPath = thTrainPath.LogicalTrainPath.BackThread.ViewTrainPath;
                //Выделение свободной нитки и ее хвоста как вторичной.
                SelectSecondaryTrainPath(thTrainPath);
            }
        }

        /// <summary>
        /// Выделяет последующие нитки как вторичные, относительно данной первичной.
        /// </summary>
        /// <param name="PrimaryTrainPath">Первично выделенная нитка.</param>
        private static void SelectSecondaryNextTrainPath(TrainPath PrimaryTrainPath)
        {
            //Инициирует цикл для продвижения по всем Next ниткам, связанным с данной.
            TrainPath thTrainPath = PrimaryTrainPath;
            while (thTrainPath.LogicalTrainPath.NextThread != null)
            {
                //Переход к новой свободной нитке.
                thTrainPath = thTrainPath.LogicalTrainPath.NextThread.ViewTrainPath;
                //Выделение свободной нитки и ее хвоста как вторичной.
                SelectSecondaryTrainPath(thTrainPath);
            }
        }

        /// <summary>
        /// Выделяет все связанные нитки с данной первично выделенной, как вторично выделенные.
        /// </summary>
        /// <param name="PrimaryTrainPath">Первично выделенная нитка.</param>
        public static void SelectSecondaryAllRelatedTrainPath(TrainPath PrimaryTrainPath)
        {
            //Пробегает по всем предыдущим ниткам относительно данной первично выделенной и выделяет эти нитки как вторично выделенные.
            SelectSecondaryBackTrainPath(PrimaryTrainPath);
            //Пробегает по всем последующи ниткам относительно данной первично выделенной и выделяет эти нитки как вторично выделенные.
            SelectSecondaryNextTrainPath(PrimaryTrainPath);
        }
    }
}