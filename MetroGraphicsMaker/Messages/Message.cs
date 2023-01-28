using System;
using System.Windows;
using Core;
using Converters;

namespace Messages
{
    public static class Message
    {
        public static void showAboutRoute(Route route, TimeInterval time, RepairState state)
        {
            showAboutRoute(route, time.begin, time.end, state);
        }

        public static void showAboutRoute(Route route, Int32 beginTime, Int32 endTime, RepairState state)
        {
            var format = "";
            switch (state)
            { 
                case RepairState.ALREADY_EXIST:
                    format = "У маршрута №{0} уже предусмотрен ремонт с {1} по {2}";
                    break;
                case RepairState.CANNOT_INSERT:
                    format = "Нет возможности вставить ремонт для маршрута №{0} с {1} до {2}";
                    break;
                default:
                    format = "Не верное состояние. В функцию были переданы следующие параметры:"+ Environment.NewLine +"route.number = {0}, beginTime = {1}, endTime = {2}, state = " + state + ";";
                    break;
            }

            var result = String.Format(format, route.number, TimeConverter.SecondsToString(beginTime),
                TimeConverter.SecondsToString(endTime));
            Console.WriteLine(result);
            Logger.Output(result, "msg");
        }

        public static void showAboutHungarianAlgorithm(HungarianState state)
        {
            var format = "";
            switch (state)
            {
                case HungarianState.NOT_ENOUGH_TIME_AND_SPACE:
                    format = "Для проведения всех ремонтов не хватает времени и места!";
                    break;
                default:
                    format = "Неверное состояние.";
                    break;
            }

            MessageBox.Show(format, "Hungarian SelectedAlgorythm", MessageBoxButton.OK, MessageBoxImage.Error);

            Console.WriteLine(format);
            Logger.Output(format, "msg");
        }
    }
}
