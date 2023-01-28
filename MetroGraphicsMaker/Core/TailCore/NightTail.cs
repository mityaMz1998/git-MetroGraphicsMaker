using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows;
using Converters;

namespace Core
{

    //Класс описывающий хвосты, которые являются концами ниток и точками ночной расстановки.
    class NightTail : AbstractTail
    {
        public int LengthLine = 100;
        public Boolean isExistText = false;
        public NightStayPoint StayPoint;
    }
}
