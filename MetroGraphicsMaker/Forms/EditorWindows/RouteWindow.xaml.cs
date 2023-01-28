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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Core;
using System.Collections.ObjectModel;

namespace WpfApplication1.Forms.EditorWindows
{
    public class RouteDataGrid
    {
        public Int32 ReapairNumberColumn { get; set; }
        public ObservableCollection<RepairType> RepairTypeColumn { get; set; }
        public ObservableCollection<InspectionPoint> LocationColumn { get; set; }
        public bool ExitDepotColumn { get; set; }

    }
    /// <summary>
    /// Логика взаимодействия для ConfigurationForm.xaml
    /// </summary>
    public partial class TrainRouteEditorWindow : Window
    {
        public ObservableCollection<RouteDataGrid> RouteDataGridResults = new ObservableCollection<RouteDataGrid>();

        public Int32 CurrentIndex = 1;

        public TrainRouteEditorWindow()
        {

            InitializeComponent();
            RouteDataGridResults.Add(new RouteDataGrid()
            {
                ReapairNumberColumn = CurrentIndex,
                RepairTypeColumn = new ObservableCollection<RepairType>(MovementSchedule.colRepairType),
                LocationColumn = new ObservableCollection<InspectionPoint>(MovementSchedule.colInspectionPoint)
            });
            DataContext = RouteDataGridResults;
            //RepairDataGrid.ItemsSource = RouteDataGridResults;
            //RepairTypeColumn.ItemsSource = MovementSchedule.colRepairType;
            //LocationColumn.ItemsSource = MovementSchedule.colInspectionPoint;

            Route curRoute = MovementSchedule.colRoute[0];
            TrainNumberComboBox.ItemsSource = MovementSchedule.colRoute;
            TrainNumberComboBox.SelectedIndex = 0;
            DepotRegistryComboBox.ItemsSource = MovementSchedule.colDepot;
            DepotRegistryComboBox.SelectedIndex = 0;
            NightStayPointComboBox.ItemsSource = MovementSchedule.colNightStayPoint;
            NightStayPointComboBox.SelectedIndex = 0;


            Show();
        }


        private void CancelButton_OnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            //int i = 1;
            RouteDataGridResults.Add(new RouteDataGrid()
            {
                ReapairNumberColumn = ++CurrentIndex,
                RepairTypeColumn = new ObservableCollection<RepairType>(MovementSchedule.colRepairType),
                LocationColumn = new ObservableCollection<InspectionPoint>(MovementSchedule.colInspectionPoint)
            });
            DataContext = RouteDataGridResults;
            //i++;
        }

        private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (RepairDataGrid.SelectedIndex >= 0)
            {
                RouteDataGrid delRowGrid = RepairDataGrid.SelectedItem as RouteDataGrid;
                RouteDataGridResults.Remove(delRowGrid);
                MessageBox.Show(String.Format("Row {0} is deleted", delRowGrid.ReapairNumberColumn));
            }

        }
	}
}