using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows.Media;
using System.Windows;
using Converters;

namespace Core
{
    //Класс описывающий хвосты, которые являются концами ниток.
    class SingleTail: AbstractTail
    {
        /*
        public Double Angle;
        public int LeftHeight;
        public int RightHeight;
        public int NumberOfLines;*/
        public int TimeOffsetEndTail;
    }
}
