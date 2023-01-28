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
    //Содержит описание абстрактного класса для всех операций.
    public class BaseEdit
    {
        public TrainPath CTrainPath;
        public ListTrainPaths MasterTrainPaths;

        protected ActionState state;

        public BaseEdit(TrainPath _СallingTrainPath, ListTrainPaths _MasterTrainPaths)
        {
            CTrainPath = _СallingTrainPath;

            MasterTrainPaths = _MasterTrainPaths;
        }

        public virtual Boolean Check(int NewPositionTime)
        {
            return false;
        }

        public virtual Boolean Check()
        {
            return false;
        }

        public virtual Boolean Check(Boolean _DirectionOfRemoval, byte NumberStation) //ButtonMouse - Выбор направления удаления точек: false удаляет левые относительно позиции NumberStation, true правые.
        {
            return false;
        }

        public virtual Boolean Check(byte NumberStation) //ButtonMouse - Выбор направления удаления точек: false удаляет левые относительно позиции NumberStation, true правые.
        {
            return false;
        }

        public virtual Boolean Check(int _StartTimeTrainPath, DirectionValue _DirectionTrainPath) //DirectionTrainPath - направление нитки: "Снизу вверх" или "Сверху вниз". True - Снизу вверх.
        {
            return false;
        }

        public virtual ActionState Do()
        {
            return ActionState.DONE;
        }

        public virtual ActionState Do(int _DeltaTime)
        {
            return ActionState.DONE;
        }

        public virtual ActionState Do(Boolean _DirectionOfRemoval, int _NumberStation)
        {
            return ActionState.DONE;
        }

        public virtual ActionState Do(int _StartTimeTrainPath, DirectionValue _DirectionTrainPath) //DirectionTrainPath - направление нитки: "Снизу вверх" или "Сверху вниз". True - Снизу вверх.
        {
            return ActionState.DONE;
        }

        public virtual ActionState Undo()
        {
            return ActionState.CANCELLED;
        }
    }
}

