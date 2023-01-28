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

namespace Forms
{
    /// <summary>
    /// Interaction logic for ChooseAlgorithmsForm.xaml
    /// </summary>
    public partial class ChooseAlgorithmsForm : Window
    {
        private RadioButton division_rb;
        private RadioButton hungarian_rb;
        private RadioButton modifiedHungarian_rb;
        private RadioButton graphical_rb;
        private RadioButton genetics_rb;

        private RadioButton withoutSorting_rb;
        private RadioButton desending_rb;
        private RadioButton ascending_rb;

        private RadioButton overall_rb;
        private RadioButton successed_rb;
        private RadioButton initiated_rb;

        CheckBox isOrderedCheckBox;
        CheckBox isBreakCheckBox;

        private RadioButton equality_rb;
        private RadioButton minAddTime_rb;
        public ChooseAlgorithmsForm()
        {
            InitializeComponent();

            var rowCount = 3;
            var colCount = 2;
            for (int row = 0; row < rowCount; row++)
                tableLayout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            for (int column = 0; column < colCount; column++)
                tableLayout.ColumnDefinitions.Add(new ColumnDefinition());

            #region Способ построения ГО
            GroupBox groupBox1 = new GroupBox
            {
                Header = "Способ построения ГО",
                Width = 350,
                MinHeight = 180,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                //Background = Brushes.LightGreen,
                FontSize = 12,
                Margin = new Thickness(10)
            };

            division_rb = new RadioButton
            {
                Content = "Деление на цепочки и звенья",
                Margin = new Thickness(5),
            };

            hungarian_rb = new RadioButton
            {
                Content = "Венгерский алгоритм",
                Margin = new Thickness(5)
            };

            modifiedHungarian_rb = new RadioButton
            {
                Content = "Модифицированный Венгерский алгоритм",
                Margin = new Thickness(5)
            };

            graphical_rb = new RadioButton
            {
                Content = "Графовый алгоритм",
                Margin = new Thickness(5),
                IsChecked = true
            };

            genetics_rb = new RadioButton
            {
                Content = "Генетический алгоритм",
                Margin = new Thickness(5)
            };

            StackPanel stpPanel = new StackPanel { Margin = new Thickness(10) };
            stpPanel.Children.Add(division_rb);
            stpPanel.Children.Add(hungarian_rb);
            stpPanel.Children.Add(modifiedHungarian_rb);
            stpPanel.Children.Add(graphical_rb);
            stpPanel.Children.Add(genetics_rb);

            var grid = new Grid();
            grid.Children.Add(stpPanel);
            groupBox1.Content = grid;
            tableLayout.Children.Add(groupBox1);
            #endregion

            #region Способ упорядочивания цепочек
            GroupBox groupBox2 = new GroupBox
            {
                Header = "Способ упорядочивания цепочек",
                Width = 350,
                //Height = 180,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 12,
                Margin = new Thickness(5)
            };

            StackPanel sortingPanel = new StackPanel { Margin = new Thickness(5) };
            withoutSorting_rb = new RadioButton
            {
                Content = "Без упорядочивания",
                Margin = new Thickness(5),
                IsChecked = true
            };

            desending_rb = new RadioButton
            {
                Content = "По уменьшению дополнительного времени",
                Margin = new Thickness(5)
            };

            ascending_rb = new RadioButton
            {
                Content = "По возрастанию величины окна возможностей",
                Margin = new Thickness(5)
            };
            sortingPanel.Children.Add(withoutSorting_rb);
            sortingPanel.Children.Add(desending_rb);
            sortingPanel.Children.Add(ascending_rb);
            var grid1 = new Grid();
            grid1.Children.Add(sortingPanel);
            groupBox2.Content = grid1;
            #endregion

            #region Способ остановки работы алгоритма
            GroupBox groupBox_Termination = new GroupBox
            {
                Header = "Способ остановки работы алгоритма",
                Width = 350,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                FontSize = 12,
                Margin = new Thickness(5)
            };

            successed_rb = new RadioButton
            {
                Content = "До заданного числа успешных",
                Margin = new Thickness(5)
            };

            initiated_rb = new RadioButton
            {
                Content = "До заданного числа начатых",
                Margin = new Thickness(5)
            };

            overall_rb = new RadioButton
            {
                Content = "До конца",
                Margin = new Thickness(5),
                IsChecked = true
            };

            StackPanel terminationPanel = new StackPanel { Margin = new Thickness(5) };
            terminationPanel.Children.Add(successed_rb);
            terminationPanel.Children.Add(initiated_rb);
            terminationPanel.Children.Add(overall_rb);

            StackPanel textBoxPanel = new StackPanel {Margin = new Thickness(5)};
            TextBox successedNumberBox = new TextBox
            {
                Text = "1",
                Width = 70,
                Height = 20,
                Margin = new Thickness(5)
            };

            TextBox initiatedNumberBox= new TextBox
            {
                Text = "10",
                Width = 70,
                Height = 20,
                Margin = new Thickness(5)
            };

            textBoxPanel.Children.Add(successedNumberBox);
            textBoxPanel.Children.Add(initiatedNumberBox);
            Grid.SetColumn(textBoxPanel,1);

            var grid3 = new Grid();
            grid3.ColumnDefinitions.Add(new ColumnDefinition {Width = GridLength.Auto});
            grid3.ColumnDefinitions.Add(new ColumnDefinition());
            grid3.Children.Add(terminationPanel);
            grid3.Children.Add(textBoxPanel);
            groupBox_Termination.Content = grid3;

            StackPanel groupPanel = new StackPanel { HorizontalAlignment = HorizontalAlignment.Center, VerticalAlignment = VerticalAlignment.Center };
            groupPanel.Children.Add(groupBox2);
            groupPanel.Children.Add(groupBox_Termination);
            Grid.SetColumn(groupPanel, 1);
            tableLayout.Children.Add(groupPanel);
            #endregion

            #region Тип критерия
            GroupBox groupBox_Creteria = new GroupBox
            {
                Header = "Тип критерия",
                Width = 350,
                Margin = new Thickness(15),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                FontSize = 12
            };

            equality_rb = new RadioButton
            {
                Content = "Равномерность размещения",
                Margin = new Thickness(5),
                IsChecked = true
            };

            minAddTime_rb = new RadioButton
            {
                Content = "Минимум превышения",
                Margin = new Thickness(5)
            };

            StackPanel criteriaPanel = new StackPanel { Margin = new Thickness(5)};
            criteriaPanel.Children.Add(equality_rb);
            criteriaPanel.Children.Add(minAddTime_rb);
            var criteriaGrid = new Grid();
            criteriaGrid.Children.Add(criteriaPanel);
            groupBox_Creteria.Content = criteriaGrid;
            Grid.SetRow(groupBox_Creteria, 1);
            Grid.SetColumnSpan(groupBox_Creteria,2);
            tableLayout.Children.Add(groupBox_Creteria);
            #endregion

           
            #region Buttons
            StackPanel buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            Button okButton = new Button
            {
                Content = "OK",
                Width = 100,
                Height = 35,
                Margin = new Thickness(20),

            };
            okButton.Click += okButton_Click;

            Button cancelButton = new Button
            {
                Content = "Cancel",
                Width = 100,
                Height = 35,
                Margin = new Thickness(20)
            };
            cancelButton.Click += cancelButton_Click;

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);
            Grid.SetColumnSpan(buttonPanel, 2);

            tableLayout.Children.Add(buttonPanel);
            #endregion

            Show();
        }

        void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void okButton_Click(object sender, RoutedEventArgs e)
        {
            // для типа алгоритмов решения задачи о назначениях
            if (graphical_rb.IsChecked == true)
            {
                MovementSchedule.SelectedAlgorythm = MovementScheduleAlgorythm.RECURSIVE;
            }
            else if (division_rb.IsChecked == true)
            {
                MovementSchedule.SelectedAlgorythm = MovementScheduleAlgorythm.NONE;
            }
            else if (hungarian_rb.IsChecked == true)
            {
                MovementSchedule.SelectedAlgorythm = MovementScheduleAlgorythm.HUNGARIAN;
            }
            else if (modifiedHungarian_rb.IsChecked == true)
            {
                MovementSchedule.SelectedAlgorythm = MovementScheduleAlgorythm.DINAMIC_HUNGARIAN;
            }
            //else
            //MessageBox.Show("Выбран Генетический алгоритм!", "Your choice", MessageBoxButton.OKCancel, MessageBoxImage.Information);


            // для упорядочивания цепочек
            if (desending_rb.IsChecked == true)
            {
                MovementSchedule.SelectedSortingMode= ChainsSortingModes.DESCENDING_ADDITIONAL_TIME;
            }
            else if (ascending_rb.IsChecked == true)
            {
                MovementSchedule.SelectedSortingMode= ChainsSortingModes.ASCENDING_POSSIBILITY_WINDOW;
            }
            else
            {
                MovementSchedule.SelectedSortingMode= ChainsSortingModes.NONE;
            }

            // для остановки работы алгоритма
            if (successed_rb.IsChecked == true)
            {
                MovementSchedule.SelectedTerminationMode= AlgorithmTerminationModes.SPECIFIED_SUCCESS_NUMBER;
            }
            else if (initiated_rb.IsChecked==true)
            {
                MovementSchedule.SelectedTerminationMode= AlgorithmTerminationModes.SPECIFIED_INITIATING_NUMBER;
            }
            else
            {
                MovementSchedule.SelectedTerminationMode= AlgorithmTerminationModes.NONE;
            }

            // для критерии
            if (equality_rb.IsChecked == true)
            {
                MovementSchedule.SelectedCriteraType= CriteraTypes.EQUALITY;
            }
            else
            {
                MovementSchedule.SelectedCriteraType= CriteraTypes.MIN_ADDITIONAL_TIME;
            }

            //if (isBreakCheckBox.IsChecked == true)
            //{
            //    MovementSchedule.isBreak = true;
            //}
            MovementSchedule.CreateGraphicOborota();


            Close();
        }
    }
}
