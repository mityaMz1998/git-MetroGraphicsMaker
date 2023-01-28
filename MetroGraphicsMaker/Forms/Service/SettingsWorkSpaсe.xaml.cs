using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using View;
using Core;
using Converters;

namespace WpfApplication1
{
    /// <summary>
    /// Окно, предназначенное для настройки диаграммы ПГД.
    /// </summary>
    public partial class SettingsWorkSpaсe : Window
    {
        /// <summary>
        /// Диаграмма для которой вызвано окно "Настройка рабочей области".
        /// </summary>
        Diagram TimeSchedule;

        /// <summary>
        /// Минимально возможное значение максимального времени.
        /// </summary>
        const int MaxMinimum=30000;
        
        /// <summary>
        /// Количество часов в дне.
        /// </summary>
        const byte HourInDay = 24;

        /// <summary>
        /// Инициализирует окно "Настройка рабочей области".
        /// </summary>
        /// <param name="_TimeSchedule">Диаграмма для которой вызвано окно "Настройка рабочей облатси".</param>
        public SettingsWorkSpaсe(Diagram _TimeSchedule)
        {
            //Инициализирует компоненты окна.
            InitializeComponent();
            //Запоминает диаграмму.
            TimeSchedule = _TimeSchedule;
            int Time = MovementSchedule.MovementTime.begin;
            MinTimeHour.Text = Convert.ToString((int)(Time / TimeConverter.secondsInHour % HourInDay));
            MinTimeMinute.Text = Convert.ToString((int)((Time % TimeConverter.secondsInHour) / TimeConverter.secondsInMinute));
            MinTimeSecond.Text = Convert.ToString(Time % TimeConverter.secondsInMinute*5);

            Time = MovementSchedule.MovementTime.end;
            MaxTimeHour.Text = Convert.ToString((int)(Time / TimeConverter.secondsInHour % HourInDay));
            MaxTimeMinute.Text = Convert.ToString((int)((Time % TimeConverter.secondsInHour) / TimeConverter.secondsInMinute));
            MaxTimeSecond.Text = Convert.ToString(Time % TimeConverter.secondsInMinute*5);
            //Отображает окно.
            Show();
        }

        /// <summary>
        /// Вводит заданные настройки и закрывает окно "Настройка рабочей области".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void InputSettings_Click(object sender, RoutedEventArgs e)
        {
            //Поверяет корректность ввода максимального времени.
            int TimeMax = (Convert.ToInt16(MaxTimeHour.Text) + HourInDay) * TimeConverter.secondsInHour + Convert.ToInt16(MaxTimeMinute.Text) * TimeConverter.secondsInMinute + Convert.ToInt16(MaxTimeMinute.Text) / 5;
            int TimeMin = Convert.ToInt16(MinTimeHour.Text) * TimeConverter.secondsInHour + Convert.ToInt16(MinTimeMinute.Text) * TimeConverter.secondsInMinute + Convert.ToInt16(MinTimeMinute.Text)/5;
            MovementSchedule.MovementTime.begin = TimeMin;
            MovementSchedule.MovementTime.end = TimeMax;
            if (TimeMax * 5 > MaxMinimum)
            {
                //Если настройки допустимы, то вводит настройки начального и конечного времени.
                TimeSchedule.InputWidthHeightDiagram(TimeMin*5 , TimeMax*5);
                //Закрывает окно.
                Close();
            }
            else
            {
                //Если нет, то выводит сообщение об ошибке.
                MessageBox.Show("Число максимального времени должно быть не менее 9:0:0.");
            }
        }

        private void MaxTimeHourIncButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeHourMax = Convert.ToInt16(MaxTimeHour.Text) + 1;
            if (CheckHourMaxTime(TimeHourMax))
            {
                MaxTimeHour.Text = Convert.ToString(TimeHourMax);
            }
        }

        private void MaxTimeMinuteIncButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeMinuteMax = Convert.ToInt16(MaxTimeMinute.Text) + 1;
            if (TimeMinuteMax > 59 || TimeMinuteMax < 0)
            {
                MaxTimeMinute.Text = "0";
                MessageBox.Show("Минута должна быть в диапозоне от 0 до 60");
            }
            else MaxTimeMinute.Text = Convert.ToString(TimeMinuteMax);
        }

        private void MaxTimeSecondIncButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeSecondMax = Convert.ToInt16(MaxTimeSecond.Text) + 1;
            if (TimeSecondMax > 59 || TimeSecondMax < 0)
            {
                MaxTimeSecond.Text = "0";
                MessageBox.Show("Секуды должны быть в диапозоне от 0 до 60");
            }
            else MaxTimeSecond.Text = Convert.ToString(TimeSecondMax);
        }

        private void MaxTimeHourDecButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeHourMax = Convert.ToInt16(MaxTimeHour.Text) - 1;
            if (CheckHourMaxTime(TimeHourMax))
            {
                MaxTimeHour.Text = Convert.ToString(TimeHourMax);
            }
        }

        /// <summary>
        /// Проверка правильности ввода значения в графу максимальные часы.
        /// </summary>
        /// <param name="MaxHourTime">Максимальный час на графике.</param>
        /// <returns>True-значение верно; False-значение не верно.</returns>
        protected Boolean CheckHourMaxTime(int MaxHourTime)
        {
            //Проверка не превосходит ли часовое время верхний предел.
            if (MaxHourTime > HourInDay)
            {
                //Если превосходит, то выводится сообщение об ошибке и задатся максимальный час как 24.
                MaxTimeHour.Text = Convert.ToString(HourInDay);
                MessageBox.Show("Максимальный час должен быть до 24.");
                return false;
            }
            else
            {
                //Проверка не превосходит ли часове время нижний предел.
                if (MaxHourTime < 0)
                {
                    //Если превосходит, то выводит ошибку и задает максимальный час как 0.
                    MaxTimeHour.Text = Convert.ToString(0);
                    MessageBox.Show("Максимальный час должен быть положителен.");
                    return false;
                }
                else
                {
                    //Если оба предела выполняются, то возвращает true.
                    return true;
                }
            }
        }
        private void MaxTimeMinuteDecButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeMinuteMax = Convert.ToInt16(MaxTimeMinute.Text) - 1;
            if (TimeMinuteMax > 59 || TimeMinuteMax < 0)
            {
                MaxTimeMinute.Text = "0";
                MessageBox.Show("Минута должна быть в диапозоне от 0 до 60");
            }
            else MaxTimeMinute.Text = Convert.ToString(TimeMinuteMax);
        }

        private void MaxTimeSecondDecButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeSecondMax = Convert.ToInt16(MaxTimeSecond.Text) - 1;
            if (TimeSecondMax > 59 || TimeSecondMax < 0)
            {
                MaxTimeSecond.Text = "0";
                MessageBox.Show("Секуды должны быть в диапозоне от 0 до 60");
            }
            else MaxTimeSecond.Text = Convert.ToString(TimeSecondMax);
        }

        private void MinTimeHourIncButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeHourMin = Convert.ToInt16(MinTimeHour.Text) + 1;
            if (CheckHourMaxTime(TimeHourMin))
            {
                MinTimeHour.Text = Convert.ToString(TimeHourMin);
            }
        }

        private void MinTimeMinuteIncButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeMinuteMin = Convert.ToInt16(MinTimeMinute.Text) + 1;
            if (TimeMinuteMin > 59 || TimeMinuteMin < 0)
            {
                MinTimeMinute.Text = "0";
                MessageBox.Show("Минута должна быть в диапозоне от 0 до 60");
            }
            else MinTimeMinute.Text = Convert.ToString(TimeMinuteMin);
        }

        private void MinTimeSecondIncButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeSecondMin = Convert.ToInt16(MinTimeSecond.Text) + 1;
            if (TimeSecondMin > 59 || TimeSecondMin < 0)
            {
                MinTimeSecond.Text = "0";
                MessageBox.Show("Секуды должны быть в диапозоне от 0 до 60");
            }
            else MinTimeSecond.Text = Convert.ToString(TimeSecondMin);
        }

        private void MinTimeHourDecButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeHourMin = Convert.ToInt16(MinTimeHour.Text) - 1;
            if (CheckHourMaxTime(TimeHourMin))
            {
                MinTimeHour.Text = Convert.ToString(TimeHourMin);
            }
        }

        private void MinTimeMinuteDecButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeMinuteMin = Convert.ToInt16(MinTimeMinute.Text) - 1;
            if (TimeMinuteMin > 59 || TimeMinuteMin < 0)
            {
                MinTimeMinute.Text = "0";
                MessageBox.Show("Минута должна быть в диапозоне от 0 до 60");
            }
            else MinTimeMinute.Text = Convert.ToString(TimeMinuteMin);
        }

        private void MinTimeSecondDecButton_Click(object sender, RoutedEventArgs e)
        {
            int TimeSecondMin = Convert.ToInt16(MinTimeSecond.Text) - 1;
            if (TimeSecondMin > 59 || TimeSecondMin < 0)
            {
                MinTimeSecond.Text = "0";
                MessageBox.Show("Секуды должны быть в диапозоне от 0 до 60");
            }
            else MinTimeSecond.Text = Convert.ToString(TimeSecondMin);
        }
    }
}
