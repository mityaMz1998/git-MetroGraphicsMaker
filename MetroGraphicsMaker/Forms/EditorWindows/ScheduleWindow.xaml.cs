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

namespace WpfApplication1
{
    /// <summary>
    /// Логика взаимодействия для ScheduleWindow.xaml
    /// Описывает работу с окном расписания.
    /// </summary>
    public partial class ScheduleWindow : Window
    {
        GroupRadioGroups NewGRG;
        TaskTable OutputTaskTable;
        ListTrainPaths MasterTrainPaths;

        public ScheduleWindow(ListTrainPaths _MasterTrainPaths)
        {
            InitializeComponent();
            MasterTrainPaths = _MasterTrainPaths;

            NewGRG = new GroupRadioGroups(new List<int> { 2, 2 });
            foreach (RadioGroup thRadioG in NewGRG.RadioGroups) foreach (RadioButton thRadioB in thRadioG.RadioButtons) thRadioB.Checked += OutputСhoice;
            NewGRG.Margin = new Thickness(10, 30, 0, 0);
            NewGRG.RadioGroups[0].RadioButtons[0].Content = "Четное";
            NewGRG.RadioGroups[1].RadioButtons[0].Content = "Пиковое";
            NewGRG.RadioGroups[0].RadioButtons[1].Content = "Нечетное";
            NewGRG.RadioGroups[1].RadioButtons[1].Content = "Непиковое";

            MainGrid.Children.Add(NewGRG);

            Show();

            OutputTaskTable = new TaskTable(MasterTrainPaths.StandartTrainPathEVENPeak);
            ScheduleDataGrid.ItemsSource = OutputTaskTable.ListTaskRow;
            for (var index = 0; index < ScheduleDataGrid.Columns.Count; index++) 
            ScheduleDataGrid.Columns[index].Header = OutputTaskTable.NamesColomns[index]; 

            
        }

        public void OutputСhoice(object sender, RoutedEventArgs e)
        {
            switch (NewGRG.NumberOfVariant)
            {
                case 0:
                    OutputTaskTable = new TaskTable(MasterTrainPaths.StandartTrainPathEVENPeak);
                    ScheduleDataGrid.ItemsSource = OutputTaskTable.ListTaskRow;
                    for (int index = 0; index < ScheduleDataGrid.Columns.Count; index++) ScheduleDataGrid.Columns[index].Header = OutputTaskTable.NamesColomns[index];
                    break;
                case 1:
                    OutputTaskTable = new TaskTable(MasterTrainPaths.StandartTrainPathEVENNonpeak);
                    ScheduleDataGrid.ItemsSource = OutputTaskTable.ListTaskRow;
                    for (int index = 0; index < ScheduleDataGrid.Columns.Count; index++) ScheduleDataGrid.Columns[index].Header = OutputTaskTable.NamesColomns[index];
                    break;
                case 2:
                    OutputTaskTable = new TaskTable(MasterTrainPaths.StandartTrainPathODDPeak);
                    ScheduleDataGrid.ItemsSource = OutputTaskTable.ListTaskRow;
                    for (int index = 0; index < ScheduleDataGrid.Columns.Count; index++) ScheduleDataGrid.Columns[index].Header = OutputTaskTable.NamesColomns[index];
                    break;
                case 3:
                    OutputTaskTable = new TaskTable(MasterTrainPaths.StandartTrainPathODDNonpeak);
                    ScheduleDataGrid.ItemsSource = OutputTaskTable.ListTaskRow;
                    for (int index = 0; index < ScheduleDataGrid.Columns.Count; index++) ScheduleDataGrid.Columns[index].Header = OutputTaskTable.NamesColomns[index];
                    break;
            }
        }

        private void OKbutton_Click(object sender, RoutedEventArgs e)
        {
            foreach (TaskRow thTaskRow in OutputTaskTable.ListTaskRow) thTaskRow.SaveChanges();
            switch (OutputTaskTable.RegimeTasks)
            {
                case RegimeOfMotion.NonPeak:
                    switch (OutputTaskTable.DirectionTasks)
                    {
                        case DirectionValue.EVEN:
                            MasterTrainPaths.StandartTrainPathEVENNonpeak = new View.TrainPath(MovementSchedule.CurrentLine.evenDirection, RegimeOfMotion.NonPeak, MasterTrainPaths);
                            break;
                        case DirectionValue.ODD:
                            MasterTrainPaths.StandartTrainPathODDNonpeak = new View.TrainPath(MovementSchedule.CurrentLine.oddDirection, RegimeOfMotion.NonPeak, MasterTrainPaths);
                            break;
                        default:
                            MessageBox.Show("Unknown DirectionTasks in ScheduleWindow class");
                            break;
                    }
                    break;
                case RegimeOfMotion.Peak:
                    switch (OutputTaskTable.DirectionTasks)
                    {
                        case DirectionValue.EVEN:
                            MasterTrainPaths.StandartTrainPathEVENPeak = new View.TrainPath(MovementSchedule.CurrentLine.evenDirection, RegimeOfMotion.Peak, MasterTrainPaths);
                            break;
                        case DirectionValue.ODD:
                            MasterTrainPaths.StandartTrainPathODDPeak = new View.TrainPath(MovementSchedule.CurrentLine.oddDirection, RegimeOfMotion.Peak, MasterTrainPaths);
                            break;
                        default:
                            MessageBox.Show("Unknown DirectionTasks in ScheduleWindow class");
                            break;
                    }
                    break;
                default:
                    MessageBox.Show("Unknown RegimeOfMotion in ScheduleWindow class");
                    break;
            }
            this.Close();
        }

        private void ScheduleDataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            ScheduleDataGrid.UpdateLayout();
        }

    }

        class TaskTable
        {
            public List<TaskRow> ListTaskRow;
            /// <summary>
            /// Строки расписания
            /// </summary>

            public List<String> NamesColomns = new List<String>(5) { "Номер", "Станции отправления и прибытия", "Tх", "Тст", "Нарастающее время" };
            /// <summary>
            /// Имена колонок.
            /// </summary>

            public DirectionValue DirectionTasks { get; private set; }
            /// <summary>
            /// Направление созданной таблицы времен хода
            /// </summary>

            public RegimeOfMotion RegimeTasks{ get; private set; }
            /// <summary>
            /// Режим созданной таблицы времен хода
            /// </summary>

            public TaskTable(View.TrainPath StandartTrain)
            {
                ListTaskRow = new List<TaskRow>();
                DirectionTasks = StandartTrain.LogicalTrainPath.direction.value;


                //RegimeTasks = StandartTrain.Regime;

                if (StandartTrain.LogicalTrainPath.flgPeak)
                {
                    RegimeTasks = RegimeOfMotion.Peak;
                }
                else
                {
                    RegimeTasks = RegimeOfMotion.NonPeak;
                }

                Int32 IncreasingTime = 0;
                for (int Number = 0; Number < StandartTrain.LogicalTrainPath.MasElRasp.Count(); Number++)
                {
                    ListTaskRow.Add(new TaskRow(StandartTrain.LogicalTrainPath.MasElRasp[Number].task, Number, RegimeTasks));
                    IncreasingTime = IncreasingTime + ListTaskRow.Last().RunningTime + ListTaskRow.Last().StoppingTime;
                    ListTaskRow.Last().IncreasingTime = IncreasingTime;
                }
                ListTaskRow.Last().IncreasingTime = IncreasingTime - ListTaskRow.Last().StoppingTime;
                ListTaskRow.Last().StoppingTime = 0;
            }

            public void ReCalculateIncreasingTime()
            {
                ListTaskRow[0].IncreasingTime = ListTaskRow[0].RunningTime + ListTaskRow[0].StoppingTime;
                for (int Number = 1; Number < ListTaskRow.Count(); Number++)
                {
                    ListTaskRow[Number].IncreasingTime = ListTaskRow[Number - 1].IncreasingTime + ListTaskRow[Number].RunningTime + ListTaskRow[Number].StoppingTime;
                }
                for (int Number = 0; Number < ListTaskRow.Count(); Number++)
                {
                    MessageBox.Show(Number + "=" + ListTaskRow[Number].IncreasingTime);
                }
            }
            //for (int Number = 0; Number < ListOutputTask.Count; Number++) ListTaskRow.Add(new TaskRow(ListOutputTask[Number], Number));
        }

    class TaskRow
    {
        private RegimeOfMotion Regime;
        /// <summary>
        /// Режим созданной таблицы времен хода
        /// </summary>
        private Task OutputTask;
        /// <summary>
        /// Задание, которое выводится.
        /// </summary>

        public int Number { get; private set; }
        /// <summary>
        /// Станция отправления.
        /// </summary>
        public String departureStation;

        /// <summary>
        /// Станция назначения.
        /// </summary>
        public String destinationStation;

        /// <summary>
        /// Станции отправления и назначения в формате "назначения-назначения".
        /// </summary>
        public String departureANDdestinationStations { get; private set; }

        /// <summary>
        /// Время хода по заданию в заданном режиме 
        /// </summary>
        public Int32 RunningTime { get; set; }

        /// <summary>
        /// Время стоянки поезда на станции прибытия в заданном режиме.
        /// </summary>
        public Int32 StoppingTime { get; set; }

        /// <summary>
        /// Нарастающее время (IncreasingTime = IncreasingTime[n-1] + RunningTime + StoppingTime)
        /// </summary>
        public Int32 IncreasingTime { get; set; }

        public TaskRow(Task _OutputTask, int _Number, RegimeOfMotion _Regime)
        {
            Regime=_Regime;
            OutputTask = _OutputTask;
            Number = _Number;
            departureStation = OutputTask.departureStation.name;
            destinationStation = OutputTask.destinationStation.name;
            departureANDdestinationStations = departureStation + "-" + destinationStation;

            switch (Regime)
            {
                case RegimeOfMotion.NonPeak:
                    RunningTime = OutputTask.ThNonpeak * 5;
                    StoppingTime = OutputTask.nextTask.StatNonpeak * 5;
                    break;
                case RegimeOfMotion.Peak:
                    RunningTime = OutputTask.ThPeak * 5;
                    StoppingTime = OutputTask.nextTask.StatPeak * 5;
                    break;
                default:
                    MessageBox.Show("Unknown RegimeOfMotion in TaskRow class");
                    break;
            }
        }

        public void SaveChanges()
        {
            switch (Regime)
            {
                case RegimeOfMotion.NonPeak:
                    OutputTask.ThNonpeak = RunningTime / 5;
                    if (StoppingTime != 0) OutputTask.nextTask.StatNonpeak = StoppingTime / 5;
                    break;
                case RegimeOfMotion.Peak:
                    OutputTask.ThPeak = RunningTime / 5;
                    if (StoppingTime != 0) OutputTask.nextTask.StatPeak = StoppingTime / 5;
                    break;
                default:
                    MessageBox.Show("Unknown RegimeOfMotion in TaskRow class");
                    break;
            }
            MessageBox.Show(OutputTask.ThNonpeak + ":" + OutputTask.nextTask.StatNonpeak + "|" + OutputTask.ThPeak + ":" + OutputTask.nextTask.StatPeak);
        }
    }

    class ScheduleTable
    {
        public List<ScheduleRow> ListScheduleRow;

        public ScheduleTable(TrainPath OutputThread)
        {
            ListScheduleRow = new List<ScheduleRow>();
            for (var number = 0; number < OutputThread.MasElRasp.Count; number++)
                ListScheduleRow.Add(new ScheduleRow(OutputThread, number));
        }

    }

    
    class ScheduleRow
    {
        public int Number { get; private set; }
        /// <summary>
        /// Станция отправления.
        /// </summary>
        public String departureStation { get; private set; }

        /// <summary>
        /// Станция назначения.
        /// </summary>
        public String destinationStation { get; private set; }

        /// <summary>
        /// Время отправления
        /// </summary>
        public int departureTime { get; private set; }

        /// <summary>
        /// Время прибытия. Только для оборота, когда направление NONE (едем в обе стороны).
        /// </summary>
        public int? arrivalTime { get; private set; }

        /// <summary>
        /// Сверхнормативная стоянка на станции, введенная по отправлению
        /// </summary>
        public int? TimeStoyanOtprav { get; private set; }

        /// <summary>
        /// Время движения
        /// </summary>
        public int? rideTime { get; private set; }

        public ScheduleRow(TrainPath OutputThread, int _Number)
        {
            Number = _Number;
            departureTime = OutputThread.MasElRasp[Number].departureTime;
            arrivalTime = OutputThread.MasElRasp[Number].arrivalTime;
            TimeStoyanOtprav = OutputThread.MasElRasp[Number].TimeStoyanOtprav;
            departureStation = OutputThread.MasElRasp[Number].task.departureStation.name;
            destinationStation = OutputThread.MasElRasp[Number].task.departureStation.name;

            if (arrivalTime != null)
            {
                rideTime = (int)arrivalTime - departureTime;
            }
            else
            {
                rideTime = null;
            }
        }
    }
}
