using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Actions.Processes;
using Exceptions.Actions;
using Converters;
using View;

namespace Forms.AutomationWindows
{
   
    /// <summary>
    /// Interaction logic for TransitionProcessWindow.xaml
    /// </summary>
    public partial class TransitionProcessWindow : Window
    {

        ListTrainPaths TransitionTrainPaths;
       // ListTrainPaths TrainPaths;
        /// <summary>
        /// Реализуемая парность для утреннего пика.
        /// </summary>
        public Int32 necessaryPairsForMorningPeak { get; protected set; }

        /// <summary>
        /// Реализуемый интервал для утреннего пика.
        /// </summary>
        public Int32 intervalTimeForMorningPeak;

        /// <summary>
        /// Используемый состав в утренний пик.
        /// </summary>
        public Int32 totalTrainsForMorningPeak;

        /// <summary>
        /// Необходимый состав на линии в период перед утренным пиком.
        /// </summary>
        public Int32 necessaryTrainsForNoPeak;

        /// <summary>
        /// Реализуемая парность после утреннего пика.
        /// </summary>
        public Int32 necessaryPairsForNoPeak;

        /// <summary>
        /// Реализуемый интервал после утреннего пика.
        /// </summary>
        public Int32 intervalTimeForNoPeak;

        /// <summary>
        /// Время начала 
        /// </summary>
        public Int32 StartTimeDg;

        /// <summary>
        /// Время завершения 
        /// </summary>
        public Int32 EndTimeDg;

        /// <summary>
        /// Введенный состав по первому пути. 
        /// </summary>
        public Int32 TrainsByFirstWayDg;

        /// <summary>
        /// Введенный состав по второму пути. 
        /// </summary>
        public Int32 TrainsBySecondWayDg { get; set; }

       // public int rowDataGrid;

        public Int32 StartTimeTransitionProcess { get; set; }

        public Int32 FullTurnaroundTimeNumber;

        public Int32 RowDataGrid;
        protected List<ResultForMyDataGrid> DataGridResults;

        protected List<ResultForTransitionProcess> TransitionResults;

        public Int32 CountFullTurnaroundNumber(Int32 endingTime, Int32 startingTime, Int32 fullTurnaroundTime)
        {
            return (endingTime - startingTime) / fullTurnaroundTime;
        }

        public TransitionProcessWindow(Int32 specifiedPikPair, Int32 endTime, Int32 totalTrains, Int32 calculatedInterval, String processString, Int32 fullTurnaroundTime)
        { 
            InitializeComponent();

            IntervalTimeForNoPeakBox.Text = "";

            StartTimeDg = endTime;
            NecessaryPairsForMorningPeakBox.Text = Convert.ToString(specifiedPikPair);
            IntervalTimeForMorningPeakBox.Text = Convert.ToString(calculatedInterval * TimeConverter.timeScale);
            TotalTrainsForMorningPeakBox.Text = Convert.ToString(totalTrains);
            ChangeStringBox.Text= processString;
            NecessaryTrainsForNoPeakBox.Text = "15";
            NecessaryPairsForNoPeakBox.Text = "20";
            MissionTimeBox.Text = "13:00:00";
            IntervalTimeForNoPeakBox.Text = Convert.ToString(3600 / Convert.ToInt32(NecessaryPairsForNoPeakBox.Text));
            FullTurnaroundTimeNumber = CountFullTurnaroundNumber(TimeConverter.TimeToSeconds(Convert.ToDateTime(MissionTimeBox.Text)), endTime, fullTurnaroundTime);
            
            DataGridResults = new List<ResultForMyDataGrid>();
            TransitionResults = new List<ResultForTransitionProcess>();
            var size = 16;
            String[] startTimeDataGrid = new String[size];
            String[] endTimeDataGrid = new String[size];
            String[] byFirstWayDataGrid = new String[size];
            String[] bySecondWayDataGrid = new String[size];

            RowDataGrid = 0;
            totalTrainsForMorningPeak = Convert.ToInt32(TotalTrainsForMorningPeakBox.Text);
            necessaryTrainsForNoPeak = Convert.ToInt32(NecessaryTrainsForNoPeakBox.Text);

            Int32 totalTrainsByFirstWay = 0;
            Int32 totalTrainsBySecondWay = 0;
            Int32 trainsSubtraction = totalTrainsForMorningPeak - necessaryTrainsForNoPeak;

            while (RowDataGrid <= FullTurnaroundTimeNumber)
            {
                startTimeDataGrid[RowDataGrid] = TimeConverter.SecondsToString(endTime);
                endTimeDataGrid[RowDataGrid] = TimeConverter.SecondsToString(endTime + fullTurnaroundTime);

                var pairs = Convert.ToString(totalTrainsForMorningPeak - necessaryTrainsForNoPeak / 2 * FullTurnaroundTimeNumber);
                byFirstWayDataGrid[RowDataGrid]  = pairs;
                bySecondWayDataGrid[RowDataGrid] = pairs;

                byFirstWayDataGrid[FullTurnaroundTimeNumber] = Convert.ToString(totalTrainsByFirstWay);
                bySecondWayDataGrid[FullTurnaroundTimeNumber] = Convert.ToString(totalTrainsBySecondWay);
                startTimeDataGrid[FullTurnaroundTimeNumber] = endTimeDataGrid[FullTurnaroundTimeNumber] = "";
        
                totalTrainsByFirstWay += Convert.ToInt32(byFirstWayDataGrid[RowDataGrid]);
                totalTrainsBySecondWay += Convert.ToInt32(bySecondWayDataGrid[RowDataGrid]);
                var totalTrainsForDg = totalTrainsByFirstWay + totalTrainsBySecondWay;

                if (totalTrainsForDg > trainsSubtraction)
                {
                    bySecondWayDataGrid[FullTurnaroundTimeNumber - 1] = Convert.ToString(Convert.ToInt32(bySecondWayDataGrid[FullTurnaroundTimeNumber - 1]) - 1);
                    bySecondWayDataGrid[FullTurnaroundTimeNumber] = Convert.ToString(Convert.ToInt32(bySecondWayDataGrid[FullTurnaroundTimeNumber]) - 1);
                }

                DataGridResults.Add(new ResultForMyDataGrid()
                {
                    StartingTimeDataGrid = startTimeDataGrid[RowDataGrid],
                    EndingTimeDataGrid = endTimeDataGrid[RowDataGrid],
                    PairsByFirstWay = Convert.ToInt32(byFirstWayDataGrid[RowDataGrid]),
                    PairsBySecondWay = Convert.ToInt32(bySecondWayDataGrid[RowDataGrid])
                });

                ++RowDataGrid;
                startTimeDataGrid[RowDataGrid] = endTimeDataGrid[RowDataGrid - 1];
                endTime = TimeConverter.StringToSeconds(startTimeDataGrid[RowDataGrid]);
            }

            EndTimeDg =TimeConverter.StringToSeconds(endTimeDataGrid[FullTurnaroundTimeNumber-1]);
            MyDataGrid.ItemsSource = DataGridResults;
            Show();
        }

        private void CalculateButton_OnClick(object sender, RoutedEventArgs e)
        {
        }

        public void ApplyButton_Click(object sender, RoutedEventArgs e)
        {
            necessaryPairsForMorningPeak = Convert.ToInt32(NecessaryPairsForMorningPeakBox.Text);
            intervalTimeForMorningPeak = Convert.ToInt32(IntervalTimeForMorningPeakBox.Text);
            totalTrainsForMorningPeak = Convert.ToInt32(TotalTrainsForMorningPeakBox.Text);
            necessaryTrainsForNoPeak = Convert.ToInt32(NecessaryTrainsForNoPeakBox.Text);
            necessaryPairsForNoPeak = Convert.ToInt32(NecessaryPairsForNoPeakBox.Text);
            intervalTimeForNoPeak = Convert.ToInt32(IntervalTimeForNoPeakBox.Text);


            TransitionResults.AddRange(MyDataGrid.Items.OfType<ResultForMyDataGrid>().Select(item => new ResultForTransitionProcess(item)));
            if (TransitionResults.Count > 0)
                TransitionResults.RemoveAt(TransitionResults.Count - 1);

            if (TransitionResults == null)
                return;

           // MessageBox.Show(TransitionResults.Count.ToString());
            
            try
            {
                TransitionProcess transitionProcessCreator = new TransitionProcess(necessaryPairsForMorningPeak, intervalTimeForMorningPeak, totalTrainsForMorningPeak, necessaryTrainsForNoPeak, necessaryPairsForNoPeak, intervalTimeForNoPeak, StartTimeDg, EndTimeDg, TransitionResults, TransitionTrainPaths);
                transitionProcessCreator.AnalyzeTransition(transitionProcessCreator.TransitionMyDo(0));
                Close();
            }
            catch (TheOperationIsNotFeasible tne)
            {
                MessageBox.Show("Нехватка :(", GetType().FullName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
            //StacProcessWindow backToStacProcessWindow = new StacProcessWindow(null);
        }
        private void DepotEditButton_Click(object sender, RoutedEventArgs e)
        {

        }
        private void ShuntingBrigadeEditButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
