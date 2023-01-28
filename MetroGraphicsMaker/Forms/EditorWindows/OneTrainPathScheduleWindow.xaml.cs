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

using Core;
using View;
using WpfApplication7.Controls.RadioGroupClasses;
using TrainPath = Core.TrainPath;
using Converters;

namespace WpfApplication1
{
    /// <summary>
    /// Логика взаимодействия для ScheduleWindow.xaml
    /// Описывает работу с окном расписания.
    /// </summary>
    public partial class OneTrainPathScheduleWindow : Window
    {
        TrainPath TrainPath;
        List<ElementOfSchedule> ScheduleTrainPath;

        public OneTrainPathScheduleWindow()
        {
            InitializeComponent();
            ScheduleDataGrid.IsReadOnly=true;
            Show();
        }

        public void SetTrainPath(TrainPath TrainPathToScheduleTable)
        {
            TrainPath = TrainPathToScheduleTable;
            if (TrainPath != null)
            {
                ScheduleTrainPath = new List<ElementOfSchedule>(TrainPath.MasElRasp.Count);

                for (int IndexElementSchedule = TrainPath.NumFirst; IndexElementSchedule <= TrainPath.NumLast; IndexElementSchedule++)
                {
                    ElementOfSchedule CreateElementOfSchedule = new ElementOfSchedule();
                    CreateElementOfSchedule.StationName = TrainPath.MasElRasp[IndexElementSchedule].task.departureStation.name;
                    CreateElementOfSchedule.ScheduleTime = TimeConverter.SecondsToTime(TrainPath.MasElRasp[IndexElementSchedule].departureTime);
                    CreateElementOfSchedule.Overtime = TimeConverter.SecondsMetroToPeopleSeconds(TrainPath.MasElRasp[IndexElementSchedule].TimeStoyanOtprav);
                    ScheduleTrainPath.Add(CreateElementOfSchedule);
                }

                ScheduleDataGrid.ItemsSource = ScheduleTrainPath;
            }
            else
            {
                ScheduleTrainPath = new List<ElementOfSchedule>(0);

                ScheduleDataGrid.ItemsSource = ScheduleTrainPath;
            }
        }
    }

    class ElementOfSchedule
    {
        public string StationName { get; set; }
        protected DateTime _ScheduleTime;
        public DateTime ScheduleTime
        {
            protected get
            {
                return _ScheduleTime;
            }
            set
            {
                _ScheduleTime = value;
                Hour = value.Hour;
                Minute = value.Minute;
                Second = value.Second;
            }
        }
        public int Hour { get; protected set; }
        public int Minute { get; protected set; }
        public int Second { get; protected set; }
        public int Overtime { get; set; }
    }
}
