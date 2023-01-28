using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using System.Collections.ObjectModel;

using Core;
using View;
using Actions.Processes;
using Converters;
using Exceptions.Actions;

namespace Forms.AutomationWindows
{
    /// <summary>
    /// Interaction logic for ConfigurationForm.xaml
    /// </summary>
    public partial class StacProcessWindow : Window
    {
        ListTrainPaths MasterTrainPaths;

        public ObservableCollection<DirectionView> directions;

        protected Direction SelectedDirection;

        public StacProcessWindow(ListTrainPaths _MasterTrainPaths)
        {
            MasterTrainPaths = _MasterTrainPaths;



            InitializeComponent();

            // Последовательность вызовов конструктор, а потом инициализация компонентов ВАЖНА!!!
            directions = new ObservableCollection<DirectionView> {
                new DirectionView { Text = "По 1 пути", Value = MovementSchedule.CurrentLine.oddDirection},  
                new DirectionView { Text = "По 2 пути", Value = MovementSchedule.CurrentLine.evenDirection},  
            };

            ComboBox.ItemsSource = directions;
            ComboBox.DisplayMemberPath = "Text";
            ComboBox.SelectedValuePath = "Value";
            ComboBox.SelectedIndex = 0;
             
            ProcesStartTimeBox.Text = "7:00:00" ;
            ProcessEndTimeBox.Text = "10:00:00";
            SpecifiedPairBox.Text = "40";
            CalculatedIntervalBox.Text = Convert.ToString  (3600 / Convert.ToInt32(SpecifiedPairBox.Text));
            TotalTrainsNumberBox.Text = "30";
            FromPath1to2Box.Text= "0:02:30";
            FromPath2to1Box.Text = "0:02:30";


            Show();
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Отмена");
            Close();
        }

        public void CalculateButton_OnClick(object sender, RoutedEventArgs e)
        {
       
        }

        private void ApplyButton_OnClick(object sender, RoutedEventArgs e)
        {
            var startTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(ProcesStartTimeBox.Text));
            var endTime = TimeConverter.TimeToSeconds(Convert.ToDateTime(ProcessEndTimeBox.Text));
            var specifiedPair = Convert.ToInt32(SpecifiedPairBox.Text);
            var calculatedInterval = Convert.ToInt32(CalculatedIntervalBox.Text);
            var totalTrain = Convert.ToInt32(TotalTrainsNumberBox.Text);
            var path1To2 = TimeConverter.TimeToSeconds(Convert.ToDateTime(FromPath1to2Box.Text));
            var path2To1 = TimeConverter.TimeToSeconds(Convert.ToDateTime(FromPath2to1Box.Text));

                        
            try
            {

                StationaryProcess stacProcessCreator = new StationaryProcess(startTime, endTime, specifiedPair, calculatedInterval / TimeConverter.timeScale, totalTrain, path1To2, path2To1, SelectedDirection, MasterTrainPaths);
                stacProcessCreator.Analyze(stacProcessCreator.MyDo());
                Close();
            }
            catch (TheOperationIsNotFeasible ane)
            {
                MessageBox.Show("Нехватка ЭПС", GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void ComboBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var cmbSender = sender as ComboBox;
            if (cmbSender == null)
                return;

           SelectedDirection = (Direction) cmbSender.SelectedValue;
        }
    }
}
