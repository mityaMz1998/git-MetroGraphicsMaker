using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using View;

namespace Core
{
    //Обстрактный класс для хвостов.
    public abstract class AbstractTail
    {
        public AbstractTailGiver.NamesTails NameTail;
        public string SecondName;
        //String Station; //Вероятно лишнее поле. Имеет некоторый смысл в случае.... (___/^)

        public int TimeBeginTail;
        public TrainPath LeftLogicalTrainPath;

        public ViewTail ViewAbstractTail;
    }
}
