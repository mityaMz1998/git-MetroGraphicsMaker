using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using View;

namespace Actions
{
    //Реализует логику выделения нитки, изменения ее цвета.
    static class SelectPrimaryTrainPathEditor
    {
        /// <summary>
        /// Выделяет первично одну нитку. При этом выделяется первично хвост этой нитки и выделяются вторично связанные нитки с данной.
        /// </summary>
        /// <param name="FreeTrainPath">Свободная нитка.</param>
        public static void SelectPrimaryTrainPath(TrainPath FreeTrainPath)
        {
            //Проверяет действительно ли свободна данная нитка.
            if (FreeTrainPath.Condition == ConditionTrainPath.Free)
            {
                //Если нитка свободна, то задает у нитки FreeTrainPath параметры как у выделенной первично.
                FreeTrainPath.Condition = ConditionTrainPath.PrimarySelected;
                FreeTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.Add(FreeTrainPath);
                FreeTrainPath.InvalidateVisual();

                //Задает параметры у хвоста нитки FreeTrainPath как у выделенной первично.
                if (FreeTrainPath.TailTrainPath != null)
                {
                    SelectPrimaryTailTrainPath(FreeTrainPath.TailTrainPath);
                }

                //Выделяет связанные нитки с данной (FreeTrainPath) как выделенные вторично.
                SelectSecondaryTrainPathEditor.SelectSecondaryAllRelatedTrainPath(FreeTrainPath);
            }
            else
            {
                //Если нитка не свободна, то происходит исключение.
                throw new System.ArgumentException("Для выделения нитки Primary ее параметр FreeTrainPath.Condition должен быть равен Free", "original");
            }
        }

        /// <summary>
        /// Выделяет хвост первично.
        /// </summary>
        /// <param name="FreeTailTrainPath">Свободный хвост.</param>
        private static void SelectPrimaryTailTrainPath(ViewTail FreeTailTrainPath)
        {
            //Проверяет свободен ли хвост.
            if (FreeTailTrainPath.Condition == ConditionTrainPath.Free)
            {
                //Если хвост свободен, то хадает его параметры как у выделенного первично.
                FreeTailTrainPath.Condition = ConditionTrainPath.PrimarySelected;

                FreeTailTrainPath.InvalidateVisual();
            }
            else
            {
                //Если хвост занаят, то сообщает об ошибке. (Исключением).
                throw new System.ArgumentException("Для выделения хвоста нитки Primary ее параметр FreeTrainPath.Condition должен быть равен Free", "original");
            }
        }
        
        /// <summary>
        /// Выделяет нитки из заданного диапазона ниток. При этом не учитываются граничные нитки. Интервал: (Time1,Time2);
        /// </summary>
        /// <param name="FirstFreeTrainPath">Первое ограничение. (Слева или права)</param>
        /// <param name="SecondFreeTrainPath">Второе ограничение. (Слева или права)</param>
        public static void SelectPrimaryTrainPathsWithoutBorder(TrainPath FirstFreeTrainPath, TrainPath SecondFreeTrainPath)
        {
            //Если нитки не принадлежат одному графику, то генерирует исключение.
            if (FirstFreeTrainPath.MasterTrainPaths != SecondFreeTrainPath.MasterTrainPaths)
            {
                throw new System.ArgumentException("FirstFreeTrainPath и SecondFreeTrainPath должны быть с одного ПГД.", "original");
            }
            //Если нитки имеют разные направления то генерируется исключение.
            if (FirstFreeTrainPath.LogicalTrainPath.direction != SecondFreeTrainPath.LogicalTrainPath.direction)
            {
                throw new System.ArgumentException("FirstFreeTrainPath и SecondFreeTrainPath должны быть одного направления.", "original");
            }

            //Определяет начальное и конечное время интервала.
            int FirstTimeStartFirstStation;
            int SecondTimeStartFirstStation;
            List<TrainPath> SelectTrainPaths;
            if (SecondFreeTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation > FirstFreeTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation)
            {
                FirstTimeStartFirstStation = FirstFreeTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation;
                SecondTimeStartFirstStation = SecondFreeTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation;
            }
            else
            {
                FirstTimeStartFirstStation = SecondFreeTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation;
                SecondTimeStartFirstStation = FirstFreeTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation;
            }
            //Ищет все нитки подпадающие под заданный интервал времени и направление.
            SelectTrainPaths = FirstFreeTrainPath.MasterTrainPaths.TrainPaths.FindAll(thTrainPath => FirstTimeStartFirstStation < thTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation && SecondTimeStartFirstStation > thTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation && FirstFreeTrainPath.LogicalTrainPath.direction == thTrainPath.LogicalTrainPath.direction && thTrainPath.Condition == ConditionTrainPath.Free);
            
            //Выделяет все нитки подпадающие под интервал как первичные.
            foreach (TrainPath thTrainPath in SelectTrainPaths)
            {
                SelectPrimaryTrainPath(thTrainPath);
            }
        }

        /// <summary>
        /// Выделяет нитки из заданного диапазона ниток. При этом учитываются граничные нитки, если они свободны. Интервал: [Time1,Time2];
        /// </summary>
        /// <param name="FirstFreeTrainPath">Первое ограничение. (Слева или права)</param>
        /// <param name="SecondFreeTrainPath">Второе ограничение. (Слева или права)</param>
        public static void SelectPrimaryTrainPathsWithBorder(TrainPath FirstFreeTrainPath, TrainPath SecondFreeTrainPath)
        {
            //Если нитки не принадлежат одному графику, то генерирует исключение.
            if (FirstFreeTrainPath.MasterTrainPaths != SecondFreeTrainPath.MasterTrainPaths)
            {
                throw new System.ArgumentException("FirstFreeTrainPath и SecondFreeTrainPath должны быть с одного ПГД.", "original");
            }
            //Если нитки имеют разные направления то генерируется исключение.
            if (FirstFreeTrainPath.LogicalTrainPath.direction != SecondFreeTrainPath.LogicalTrainPath.direction)
            {
                throw new System.ArgumentException("FirstFreeTrainPath и SecondFreeTrainPath должны быть одного направления.", "original");
            }

            //Если нитка FirstFreeTrainPath действительно свободна, то выделяет ее публично.
            if (FirstFreeTrainPath.Condition == ConditionTrainPath.Free)
            {
                SelectPrimaryTrainPath(FirstFreeTrainPath);
            }
            //Выделяет все нитки между данными как первичные.
            SelectPrimaryTrainPathsWithoutBorder(FirstFreeTrainPath, SecondFreeTrainPath);
            //Если нитка SecondFreeTrainPath действительно свободна, то выделяет ее публично.
            if (SecondFreeTrainPath.Condition == ConditionTrainPath.Free)
            {
                SelectPrimaryTrainPath(SecondFreeTrainPath);
            }
        }
    }
}
