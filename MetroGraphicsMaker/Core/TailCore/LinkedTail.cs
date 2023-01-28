using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows;

using Converters;

namespace Core
{
    //Класс, описывающий соединяющие хвосты.
    class LinkedTail : AbstractTail
    {
        public TrainPath RightLogicalTrainPath;
        public int DeltaTime;
    }
}

