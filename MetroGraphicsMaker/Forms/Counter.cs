using System;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;

namespace MetroGraphicsMaker.Forms
{
    public class Counter
    {
        /// <summary>
        /// Значение счётчика.
        /// </summary>
        public UInt64 Value { get; set; }

        /// <summary>
        /// Значение по достижении которого необходимо остановить работу алгоритма.
        /// </summary>
        public Nullable<UInt64> StopValue { get; set; }

        /// <summary>
        /// Метка счётчика.
        /// </summary>
        public String Label { get; set; }

        public String Key { get; protected set; }


        public Counter() { }

        public Counter(String key, String label)
        {
            Key = key;
            Label = label;
            Value = 0;
            StopValue = null;
        }

        public Control[] MakeControls()
        {
            DockPanel myDockPanel= new DockPanel();
            return new Control[]
            {
                //new CheckBox
                //{
                //    //AutoSize = true,
                //    //Dock = DockStyle.Fill,
                //    TextAlign = ContentAlignment.MiddleLeft,
                //    Name = Key + "CheckBox",
                //    Text = Label
                //},
                new TextBox
                {
                    //AutoSize = true,
                    //TextAlign = HorizontalAlignment.Right,
//                  Dock = DockStyle.Fill,
                    Name = Key + "CounterText", 
                    Text = Value.ToString("D"),
//myComment         Size = new Size(150, 25)
                    Width = 150,
                    Height = 25
                },
                new TextBox
                {
                    //AutoSize = true,
                    //TextAlign = HorizontalAlignment.Right,
//                  Dock = DockStyle.Fill,
                    IsEnabled = StopValue.HasValue,
                    Name = Key + "StopText",
                    Text = StopValue.HasValue ? StopValue.Value.ToString("D") : "Задать значение",
//myComment         Size = new Size(150, 25)
                    Width = 150,
                    Height = 25
                }
            };
        }

    }
}