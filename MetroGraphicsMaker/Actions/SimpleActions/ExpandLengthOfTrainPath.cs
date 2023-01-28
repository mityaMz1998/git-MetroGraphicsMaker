using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using View;

using Core;
using TrainPath = Core.TrainPath;
using Exceptions.Actions;

namespace Actions
{
    /// <summary>
    /// Добавляет нитке реальные станции до конечной вначале или в конце. (В зависимости от флага)
    /// </summary>
    class ExpandLengthOfTrainPath : BaseEdit
    {
        /// <summary>
        /// Хранит номер первой реальной станции нитки.
        /// </summary>
        private byte StartNumberFirstStation;
        /// <summary>
        /// Хранит номер последней реальной станции нитки.
        /// </summary>
        private byte StartNumberLastStation;
        /// <summary>
        /// Хранит с момента проверки (Check) станцию, с которой происходит операция.
        /// </summary>
        private byte NumberStation;

        /// <summary>
        /// Использовалось ли изменение данного типа или нет. Необходимо для корректного добавления в журнал выполненных операций пользователем.
        /// </summary>
        private Boolean isModify = false;

        public ExpandLengthOfTrainPath(TrainPath TrainPath, ListTrainPaths _MasterTrainPaths)
            : base(TrainPath, _MasterTrainPaths)
        {
            //Запоминает начальную и конечную реальную станцию нитки.
            StartNumberFirstStation = TrainPath.NumFirst;
            StartNumberLastStation = TrainPath.NumLast;
        }

        /// <summary>
        /// Делает проверку выполнимости операции, а так же подгатавливает данные к ее выполнению.
        /// </summary>
        /// <param name="_NumberStation">Номер станции до которой следует пролить нитку.</param>
        /// <returns>Возвращает true если операцию делать можно и false если выполнять операцию не рекомендуется.</returns>
        public override Boolean Check(byte _NumberStation)
        {
            //Запоминает станцию, до которой следует пролить нитку.
            NumberStation = _NumberStation;
            //Если нитка идет "снизу вверх", то "инвертирует" номер станции, до которой следует продлит нитку. Делается для того, чтобы скрыть, что такая нитка имеет последнюю станцию первой в своем списке.
            if (CTrainPath.direction.value == DirectionValue.EVEN) NumberStation = (byte)(MovementSchedule.colStation.Count - NumberStation - 1);

            //Если номер станции, до которой следует продлить нитку принадлежит интервалу возможных станций и при этом ее номер не пренадлежит интервалу реальных станций нитки, то возвращает true. 
            if (NumberStation >= 0 && NumberStation < MovementSchedule.colStation.Count && (NumberStation < StartNumberFirstStation || NumberStation > StartNumberLastStation))
            {
                return true;
            }
            //Если условие выше не выполняется, то возвращает false.
            return false;
        }

        /// <summary>
        /// Выполняет операцию продления нитки до заданной станции.
        /// </summary>
        /// <returns>Возвращает результат операции.</returns>
        public override ActionState Do()
        {
            //Смотрит новая реальная станция больше последней или нет.
            if (NumberStation > StartNumberLastStation)
            {
                //Если станция больше текущей максимальной реальной станции, то задает новую последниюю реальную станцию для нитки.
                CTrainPath.NumLast = NumberStation;
            }
            else
            {
                //Если станция меньше текущей минимальной реальной станции, то задает новую первую реальную станцию для нитки.
                CTrainPath.NumFirst = NumberStation;
            }

            if (!isModify) //Регистрация в журналах выполненых польхователем операций
            {
                MasterTrainPaths.StackAllDoOperation.Push(this);
                MasterTrainPaths.StackAllUndoOperation.Clear();
                isModify = true;
            }
            //Перерисовывает нитку.
            CTrainPath.ViewTrainPath.InvalidateVisual();

            //Возвращает результать операции. (Все удачно выполнено).
            return ActionState.DONE;
        }

        /// <summary>
        /// Отменяет операцию удлинения нитки.
        /// </summary>
        /// <returns>Возвращает результат операции отмены удлинения нитки.</returns>
        public override ActionState Undo()
        {
            //Сбрасывает все настройки нитки в состояние предшествующее выполнению операции удлинения нитки.
            CTrainPath.NumFirst = StartNumberFirstStation;
            CTrainPath.NumLast = StartNumberLastStation;

            //Перерисовывает нитку.
            CTrainPath.ViewTrainPath.InvalidateVisual();

            //Возвращает вызвавшему сообщение об успехе выполнения отмены.
            return ActionState.CANCELLED;
        }
    }
}
