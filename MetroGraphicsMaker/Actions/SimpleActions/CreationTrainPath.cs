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
    class CreationTrainPath : BaseEdit
    {
        public TrainPath StartTrainPath;
        //private int StartTimeTrainPath;

        private Direction DirectionTrainPath; //DirectionTrainPath - направление нитки: "Снизу вверх" или "Сверху вниз". True - Снизу вверх. 

        private Boolean isModify = false; //Использовалось ли изменение данного типа или нет.

        public CreationTrainPath(ListTrainPaths _MasterTrainPaths, Direction _DirectionTrainPath)
            : base(null, _MasterTrainPaths)
        {
            DirectionTrainPath = _DirectionTrainPath;
        }

        public override Boolean Check(int StartTimeTrainPath) //DirectionTrainPath - направление нитки: "Снизу вверх" или "Сверху вниз". True - Снизу вверх.
        {
            //StartTimeTrainPath = _StartTimeTrainPath;
            StartTrainPath = new TrainPath(StartTimeTrainPath, DirectionTrainPath, RegimeOfMotion.NonPeak, MasterTrainPaths); //Памятка: ПРичина Мастера в неправильной склейке логической и отображаемой нитки.
            View.TrainPath CreateViewTrainPath = new View.TrainPath(StartTrainPath, MasterTrainPaths);
            StartTrainPath.ViewTrainPath = CreateViewTrainPath;
            return true;
        }

        public override ActionState Do()
        {
            MasterTrainPaths.TrainPaths.Add(StartTrainPath.ViewTrainPath);
            MasterTrainPaths.Children.Add(StartTrainPath.ViewTrainPath);

            if (MasterTrainPaths != null)
            {
                StartTrainPath.ViewTrainPath.LogicalTrainPath.direction.RemakeСonsistencyTrainPathsByTime();

                if (!isModify) //Регистрация в журналах выполненых пользователем операций
                {
                    MasterTrainPaths.StackAllDoOperation.Push(this);
                    MasterTrainPaths.StackAllUndoOperation.Clear();
                }
            }

            isModify = true;
            return ActionState.DONE;
        }

        public override ActionState Undo()
        {
            MasterTrainPaths.TrainPaths.Remove(StartTrainPath.ViewTrainPath);
            MasterTrainPaths.Children.Remove(StartTrainPath.ViewTrainPath);

            StartTrainPath.direction.ColThreads.Remove(StartTrainPath);

            if (MasterTrainPaths != null)
            {
                CTrainPath.direction.RemakeСonsistencyTrainPathsByTime();
            }

            return ActionState.CANCELLED;
        }
    }
}
