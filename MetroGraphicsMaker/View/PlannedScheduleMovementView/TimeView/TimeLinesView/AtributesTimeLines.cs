using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Windows;
using System.Windows.Controls;

namespace View
{
    //Обобщающий класс для отображения линий времени.
    public class AtributesTimeLines : Label
    {
        protected int TimePosition; //Время которое символизирует линия
        protected Boolean isAllLine;
        protected Boolean isTextOutput;
        protected double HeightWorkAreaDiagram;
        public int ShiftDownText; //Сдвиг вниз относительно PannelTime:Canvas 
        public Point ShiftLabel; //Сдвиг относительно Label

        protected PannelTime MasterTime;
        //double N = 1;
    }
}
