using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Controls;
using Core;
using View;
using TrainPath = Core.TrainPath;
using Exceptions.Actions;

namespace Actions
{
    class MoveGroupPrimaryTrainPathEditor : BaseEdit
    {
        /// <summary>
        /// Хранит в памяти все нитки над которыми производятся изменения.
        /// </summary>
        private List<TrainPath> EditTrainPaths;

        /// <summary>
        /// Хранит обработчики изменения позиции для каждой отдельной нитки.
        /// </summary>
        private List<MovingPrimaryTrainPath> MovePrimaryTrainPathEditors = new List<MovingPrimaryTrainPath>();

        /// <summary>
        /// Хранит велечену смещения ниток.
        /// </summary>
        private int DeltaTime;

        /// <summary>
        /// Флаг следящий за тем, добавлялась ли эта операция в стек выполненных пользователем операций или нет. True - добавлялась. False - не добавлялась.
        /// </summary>
        private Boolean isModify = false;

        /// <summary>
        /// Создает экземпляр перемещения множествы веделенных ниток.
        /// </summary>
        /// <param name="SelectedPrimaryTrainPaths">Выделенные нитки.</param>
        /// <param name="_MasterTrainPaths">Полотно ниток, которому принадлежат нитки.</param>
        public MoveGroupPrimaryTrainPathEditor(List<TrainPath> SelectedPrimaryTrainPaths, ListTrainPaths _MasterTrainPaths)
            : base(SelectedPrimaryTrainPaths[0], _MasterTrainPaths)
        {
            //Запоминает список ниток.
            EditTrainPaths = SelectedPrimaryTrainPaths;

            //Генерирует экземпляры обработчиков перемещения ниток.
            EditTrainPaths.ForEach(thTrainPath => MovePrimaryTrainPathEditors.Add(new MovingPrimaryTrainPath(thTrainPath)));
        }

        /// <summary>
        /// Проверяет можно ли выполнять данную операцию с выделенными нитками.
        /// </summary>
        /// <param name="_DeltaTime">Задает величну смещения ниток. Отрицательные число двигают нитку вправо. Положительные влево.</param>
        /// <returns>Возвращает True - если операцию можно выполнять. False - Если выполнение операции не рекомендуется</returns>
        public override Boolean Check(int _DeltaTime)
        {
            //Запоминает величину смещения.
            DeltaTime = _DeltaTime;
            //Проверяет каждую ниту на возможность такого смещения.
            foreach (MovingPrimaryTrainPath thMovingPrimaryTrainPath in MovePrimaryTrainPathEditors)
            {
                if (!thMovingPrimaryTrainPath.Check(DeltaTime))
                {
                    //Если хотябы одна нитка сообщила о невозможности совершения операции, то сообщает о невозможности выпонения операции вообще для всех.
                    return false;
                }
            }
            //Если все нитки ответили согласием на изменение, то разрешает выполнение для всех.
            return true;
        }

        /// <summary>
        /// Выполняет операцию перемещения выделенных ниток.
        /// </summary>
        /// <returns>Сообщает о результате операции</returns>
        public override ActionState Do()
        {
            //Выплолняет операцию перемещения для каждой нитки в отдельности.
            MovePrimaryTrainPathEditors.ForEach(thMovePrimaryTrainPathEditors => thMovePrimaryTrainPathEditors.Do());

            if (MasterTrainPaths != null)
            {
                CTrainPath.direction.RemakeСonsistencyTrainPathsByTime();

                if (!isModify) //Регистрация в журналах выполненых пользователем операций
                {
                    MasterTrainPaths.StackAllDoOperation.Push(this);
                    MasterTrainPaths.StackAllUndoOperation.Clear();
                }
            }
            isModify = true;

            return ActionState.DONE;
        }

        /// <summary>
        /// Отмена результата операции.
        /// </summary>
        /// <returns>Сообщает об успешности операции</returns>
        public override ActionState Undo()
        {
            //Выполняет операцию отмены для каждой отдельной нитки.
            MovePrimaryTrainPathEditors.ForEach(thMovePrimaryTrainPathEditors => thMovePrimaryTrainPathEditors.Undo());

            if (MasterTrainPaths != null)
            {
                CTrainPath.direction.RemakeСonsistencyTrainPathsByTime();
            }

            return ActionState.CANCELLED;
        }
    }
}
