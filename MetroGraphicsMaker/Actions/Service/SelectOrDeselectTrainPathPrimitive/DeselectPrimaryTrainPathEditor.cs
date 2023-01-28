using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using View;

namespace Actions
{
    //Реализует логику выделения нитки, изменения ее цвета.
    static class DeselectPrimaryTrainPathEditor
    {
        /// <summary>
        /// Девыделяет одну нитку. При этом девыделяется хвост этой нитки и девыделяются связанные нитки с данной.
        /// </summary>
        /// <param name="PrimaryTrainPath">Выделенная первично нитка.</param>
        public static void DeselectPrimaryTrainPath(TrainPath PrimaryTrainPath)
        {
            //Проверяет действительно ли данная нитка выделена первично.
            if (PrimaryTrainPath.Condition == ConditionTrainPath.PrimarySelected)
            {
                //Если нитка выделена первично, то задает у нитки FreeTrainPath параметры как у свободной.
                PrimaryTrainPath.Condition = ConditionTrainPath.Free;
                PrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.Remove(PrimaryTrainPath);
                PrimaryTrainPath.InvalidateVisual();

                //Задает параметры у хвоста нитки FreeTrainPath как у свободной.
                if (PrimaryTrainPath.TailTrainPath != null)
                {
                    DeselectPrimaryTailTrainPath(PrimaryTrainPath.TailTrainPath);
                }

                //Девыделяет связанные нитки с данной (FreeTrainPath).
                DeselectSecondaryTrainPathEditor.DeselectSecondaryAllRelatedTrainPath(PrimaryTrainPath);
            }
            else
            {
                //Если нитка не выделена первино, то происходит исключение.
                throw new System.ArgumentException("Для девыделения нитки ее параметр FreeTrainPath.Condition должен быть равен PrimarySelected", "original");
            }
        }

        /// <summary>
        /// Выделяет хвост первично.
        /// </summary>
        /// <param name="PrimaryTailTrainPath">Выделенный вторично хвост.</param>
        private static void DeselectPrimaryTailTrainPath(ViewTail PrimaryTailTrainPath)
        {
            //Проверяет выделен ли первично ли хвост.
            if (PrimaryTailTrainPath.Condition == ConditionTrainPath.PrimarySelected)
            {
                //Если выделен то задает его параметры как у свободного.
                PrimaryTailTrainPath.Condition = ConditionTrainPath.Free;

                PrimaryTailTrainPath.InvalidateVisual();
            }
            else
            {
                //Если не выделен первично, то генерирует исключение.
                throw new System.ArgumentException("Для девыделения хвоста нитки Primary ее параметр FreeTrainPath.Condition должен быть равен PrimarySelected", "original");
            }
        }

        /// <summary>
        /// Девыделяет первично выделенные нитки исключая границы интревала. Интервал: (Time1,Time2).
        /// </summary>
        /// <param name="FirstPrimaryTrainPath">Первое ограничение. (Слева или права)</param>
        /// <param name="SecondPrimaryTrainPath">Второе ограничение. (Слева или права)</param>
        public static void DeselectPrimaryTrainPathsWithoutBorder(TrainPath FirstPrimaryTrainPath, TrainPath SecondPrimaryTrainPath)
        {
            //Если нитки не принадлежат одному графику, то генерирует исключение.
            if (FirstPrimaryTrainPath.MasterTrainPaths != SecondPrimaryTrainPath.MasterTrainPaths)
            {
                throw new System.ArgumentException("FirstFreeTrainPath и SecondFreeTrainPath должны быть с одного ПГД.", "original");
            }
            //Если нитки имеют разные направления то генерируется исключение.
            if (FirstPrimaryTrainPath.LogicalTrainPath.direction != SecondPrimaryTrainPath.LogicalTrainPath.direction)
            {
                throw new System.ArgumentException("FirstFreeTrainPath и SecondFreeTrainPath должны быть одного направления.", "original");
            }

            //Определяет начальное и конечное время интервала.
            int FirstTimeStartFirstStation;
            int SecondTimeStartFirstStation;
            List<TrainPath> SelectTrainPaths;
            if (SecondPrimaryTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation > FirstPrimaryTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation)
            {
                FirstTimeStartFirstStation = FirstPrimaryTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation;
                SecondTimeStartFirstStation = SecondPrimaryTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation;
            }
            else
            {
                FirstTimeStartFirstStation = SecondPrimaryTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation;
                SecondTimeStartFirstStation = FirstPrimaryTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation;
            }

            //Ищет все нитки подпадающие под заданный интервал времени и направление.
            SelectTrainPaths = FirstPrimaryTrainPath.MasterTrainPaths.PrimarySelectedTrainPaths.FindAll(thTrainPath => FirstTimeStartFirstStation < thTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation && SecondTimeStartFirstStation > thTrainPath.LogicalTrainPath.DepartureTimeFromFirstStation && FirstPrimaryTrainPath.LogicalTrainPath.direction == thTrainPath.LogicalTrainPath.direction && thTrainPath.Condition == ConditionTrainPath.PrimarySelected);
            
            //Девыделяет все нитки подпадающие под интервал.
            foreach (TrainPath thTrainPath in SelectTrainPaths)
            {
                DeselectPrimaryTrainPath(thTrainPath);
            }
        }
    }
}
