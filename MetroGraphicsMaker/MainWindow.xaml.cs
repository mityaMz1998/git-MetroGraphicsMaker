using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BD_NightAlignment;

using View;
using Core;
using Forms.AutomationWindows;
using Messages;
using WpfApplication1.Forms.EditorWindows;
using WpfApplication1.LoadData;
using TrainPath = View.TrainPath;
using Forms;

public enum Instruments
{
    EditLengthOfTrainPath,
    CreateOrDeleteTrainPath,
    CreateOrDeleteСonnectionTrainPaths,
    EditSchedule
}

namespace WpfApplication1
{
    /// <summary>
    /// Логика взаимодействия для ConfigurationForm.xaml
    /// </summary>
    /// 
    partial class MainWindow : Window
    {
        protected enum InterfaceState
        {
            /// <summary>
            /// Операции с ПГД доступны.
            /// </summary>
            Enabled,
            /// <summary>
            /// Операции с ПГД заблокированы.
            /// </summary>
            NotEnabled
        }
        /// <summary>
        /// Стартовое значение масштаба для интерфейса. (Не задает масштаб при загрузке ПГД, только для интерфейса. Если хочется чтобы задовал необходимо дописать нечто в таком духе: Diagram.ZoomChange(StartZoomProcent) - в Медиаторе)
        /// </summary>
        const byte StartZoomProcent = 100;

        Diagram TimeSchedule = null;

        public MainWindow()
        {
            InitializeComponent();

            PrepareLogger();
        }

        private static void PrepareLogger()
        {
            string binDirectory = @"\bin";
            string saveDirectory = @"\Save Directory\Log";
            var logSaveDirectory = AppDomain.CurrentDomain.BaseDirectory;

            int positionBinDirectory = logSaveDirectory.IndexOf(binDirectory);
            if (positionBinDirectory > 0)
            {
                logSaveDirectory = logSaveDirectory.Substring(0, positionBinDirectory) + saveDirectory + "\\";
            }

            var logDirName = String.Format("{0}{1}logs", logSaveDirectory, System.IO.Path.DirectorySeparatorChar);
            if (!System.IO.Directory.Exists(logDirName))
                System.IO.Directory.CreateDirectory(logDirName);
            Logger.filename = String.Format("{0}{1}{2}.txt", logDirName, System.IO.Path.DirectorySeparatorChar,
                DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss.fff"));
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            MainGrid.Background = Brushes.GhostWhite;
            MasterGrid.Background = Brushes.GhostWhite;
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (MasterGrid.Children.Count != 0) TimeSchedule.HorizontalSrollMove(MasterScroll.HorizontalOffset);
        }

        private void button5_Click(object sender, RoutedEventArgs e)
        {
            double ZoomCoefficient = double.Parse(ZoomProcentEdit.Text) * 0.01;
            if (TimeSchedule != null)
            {
                TimeSchedule.ZoomChange(ZoomCoefficient);
                //MasterGrid.Height = Test.Height;// +500;
                //MasterGrid.Width = Test.Width;// +500;
            }

        }

        private void СonnectionTrainPaths_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSchedule != null)
            {
                TimeSchedule.SelectTool(Instruments.CreateOrDeleteСonnectionTrainPaths);
                CreateOrDeleteСonnectionTrainPaths.IsChecked = true;
            }
        }

        private void EditSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSchedule != null)
            {
                TimeSchedule.SelectTool(Instruments.EditSchedule);
                EditSchedule.IsChecked = true;
            }
        }

        private void OneTrainPathSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSchedule != null)
            {
                if (OneTrainPathSchedule.IsChecked.Value)
                {
                    TimeSchedule.MasterTrainPaths.ShowScheduleWindow();
                }
                else
                {
                    TimeSchedule.MasterTrainPaths.CloseScheduleWindow();
                }
            }
        }

        private void LengthOfTrainPath_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSchedule != null)
            {
                TimeSchedule.SelectTool(Instruments.EditLengthOfTrainPath);
                LengthOfTrainPath.IsChecked = true;
            }
        }

        private void CreateOrDelete_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSchedule != null)
            {
                TimeSchedule.SelectTool(Instruments.CreateOrDeleteTrainPath);
                CreateOrDeleteTrainPath.IsChecked = true;
            }
        }

        private void Undo(object sender, RoutedEventArgs e)
        {
            if (TimeSchedule != null)
            {
                TimeSchedule.UndoAction();
            }
        }
        private void TrainRouteEditor_OnClick(object sender, RoutedEventArgs e)
        {
            var trainRoute = new TrainRouteEditorWindow();
        }

        private void EditPointerMenuItem_OnClick(object sender, RoutedEventArgs e)
        {
            var editPointersWindow = new NightAlignmentPointsWindow();
            editPointersWindow.Show();
        }

        private void Redo(object sender, RoutedEventArgs e)
        {
            if (TimeSchedule != null)
            {
                TimeSchedule.DoAction();
            }
        }

        private void clcCreateStacProcess(object sender, RoutedEventArgs e)
        {
            StacProcessWindow tmpWnd = new StacProcessWindow(TimeSchedule.MasterTrainPaths);
        }

        private void clcCreateGraphicOborota(object sender, RoutedEventArgs e)
        {
            //           StacProcessWindow tmpWnd = new StacProcessWindow(Test.MasterTrainPaths);
            ChooseAlgorithmsForm cafWnd = new ChooseAlgorithmsForm();
        }

        private Loader loader;

        private Saver saver;

        private void Load(object sender, RoutedEventArgs e)
        {
            TimeSchedule = new Diagram();

//            OpenFileDialog openDialog = new OpenFileDialog();
//            openDialog.DefaultExt = ".mdb";
//            openDialog.Filter = "База данных (MS Access)|*.mdb";

            var openDialog = new OpenFileDialog
            {
                DefaultExt = ".mdb",
                Filter = "База данных (MS Access)|*.mdb"
            };

            var result = openDialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                var fInfo = new System.IO.FileInfo(openDialog.FileName);
                // string strFileName = fInfo.Name;
                // string strFilePath = fInfo.DirectoryName;
                Loader.PathToDB = fInfo;
                if (loader == null)
                    loader = new Loader(fInfo.ToString());
                //MovementSchedule NewMovementSchedule = new MovementSchedule();
                // MovementSchedule.flgReady = loader.Init(String.Format("{0}{1}{2}", strFilePath, System.IO.Path.DirectorySeparatorChar, strFileName));
                //MessageBox.Show("OK");
                Loader.PathToDB = fInfo;
                MovementSchedule.flgReady = loader.Init(fInfo.ToString());

                MovementSchedule.colStation.ForEach(thStation => TimeSchedule.OY.AddStation(thStation));
                TimeSchedule.OY.Stations.Reverse();

                TimeSchedule.MasterTrainPaths.StandartTrainPathODDNonpeak = new TrainPath(MovementSchedule.CurrentLine.oddDirection, RegimeOfMotion.NonPeak, TimeSchedule.MasterTrainPaths);
                TimeSchedule.MasterTrainPaths.StandartTrainPathEVENNonpeak = new TrainPath(MovementSchedule.CurrentLine.evenDirection, RegimeOfMotion.NonPeak, TimeSchedule.MasterTrainPaths);
                TimeSchedule.MasterTrainPaths.StandartTrainPathODDPeak = new TrainPath(MovementSchedule.CurrentLine.oddDirection, RegimeOfMotion.Peak, TimeSchedule.MasterTrainPaths);
                TimeSchedule.MasterTrainPaths.StandartTrainPathEVENPeak = new TrainPath(MovementSchedule.CurrentLine.evenDirection, RegimeOfMotion.Peak, TimeSchedule.MasterTrainPaths);

                TimeSchedule.InputWidthHeightDiagram(MovementSchedule.MovementTime.begin * 5, MovementSchedule.MovementTime.end * 5);
                TimeSchedule.OY.InvalidateStation();
                DefaultSettings();
                IsEnabledInterface(InterfaceState.Enabled);
            }
        }

        /// <summary>
        /// Задает параметры отображения и взаимодействия пользователя с интерфейсом главной формы.
        /// </summary>
        /// <param name="NewStateInterface">Требуемое состояние</param>
        protected void IsEnabledInterface(InterfaceState NewStateInterface)
        {
            //Создается переменная, в которой хранится значение флага активности.
            Boolean EnabledFlag;
            switch (NewStateInterface)
            {
                case InterfaceState.Enabled: EnabledFlag = true;
                    break;
                case InterfaceState.NotEnabled: EnabledFlag = false;
                    break;
                default:
                    throw new ArgumentException("Неизвестный целевой статус для интерфейса главной формы.", "original");
            }

            //Флаг активности задает возможность взаимодействия с панелью операций.
            LengthOfTrainPath.IsEnabled = EnabledFlag;
            CreateOrDeleteTrainPath.IsEnabled = EnabledFlag;
            CreateOrDeleteСonnectionTrainPaths.IsEnabled = EnabledFlag;
            EditSchedule.IsEnabled = EnabledFlag;
            OneTrainPathSchedule.IsEnabled = EnabledFlag;
            LeftTrainPath.IsEnabled = EnabledFlag;
            RightTrainPath.IsEnabled = EnabledFlag;

            OperationCancelButton.IsEnabled = EnabledFlag;
            OperationRestoreButton.IsEnabled = EnabledFlag;

            ZoomProcentEdit.IsEnabled = EnabledFlag;
            ZoomButton.IsEnabled = EnabledFlag;

            //Флаг активности задает возможность взаимодействия с строками меню "Сервис".
            SettingsWorkSpaceMenuItem.IsEnabled = EnabledFlag;

            //Флаг активности задает возможность взаимодействия с строками меню "Автоматизация".
            clcCreateStacProcessMenuItem.IsEnabled = EnabledFlag;

            //Флаг активности задает возможность взаимодействия с строками меню "Редактирование".
            OperationCancelMenuItem.IsEnabled = EnabledFlag;
            OperationRestoreMenuItem.IsEnabled = EnabledFlag;
            MenuEditSchedule.IsEnabled = EnabledFlag;
            MenuCreateOrDeleteСonnectionTrainPaths.IsEnabled = EnabledFlag;
            MenuCreateOrDeleteTrainPath.IsEnabled = EnabledFlag;
            MenuLengthOfTrainPath.IsEnabled = EnabledFlag;
            MenuScheduleMovement.IsEnabled = EnabledFlag;
        }

        private void Create(object sender, RoutedEventArgs e)
        {
            TimeSchedule = new Diagram();
            DefaultSettings();
        }

        /// <summary>
        /// Устанавливает интерфейс в "нулевое" состояние.
        /// </summary>
        private void DefaultSettings()
        {
            //Проверяет графическую область на наличие элементов и при их обнаружении очищает ее.
            if (MasterGrid.Children.Count > 0)
            {
                MasterGrid.Children.Clear();
            }
            //Если имеется диаграмма для отобрадения добавляет ее на графическую область.
            if (TimeSchedule != null)
            {
                MasterGrid.Children.Add(TimeSchedule);
            }

            //Устанавливает панель инструментов в стандартное состояние. (Редактирование расписания)
            EditSchedule.IsChecked = true;
            //Устанавливает масштаб на 100 процентов.
            ZoomProcentEdit.Text = StartZoomProcent.ToString();

            //Возвращает ползунки в начально состояние. (Горизонтальный до упора в лево. Вертикальный в самый верх)
            MasterScroll.ScrollToHome();
            MasterScroll.ScrollToTop();
        }

        private void LengthScheduleMovement_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSchedule != null)
            {
                ScheduleWindow TasksOutputOnTable = new ScheduleWindow(TimeSchedule.MasterTrainPaths);
            }
        }

        /// <summary>
        /// Создает окно Сервис->"Настройки рабочей области".
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingsWorkSpaсe_Click(object sender, RoutedEventArgs e)
        {
            //Проверяет есть ли диаграмма.
            if (TimeSchedule != null)
            {
                //Если диаграмма существует, то создает окно "Настройки рабочей области".
                SettingsWorkSpaсe SettingsWorkSpaсeWindow = new SettingsWorkSpaсe(TimeSchedule);
            }
        }

        private void LeftTrainPath_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSchedule.MasterTrainPaths.PrimarySelectedTrainPaths.Count == 1)
            {
                if (!TimeSchedule.MasterTrainPaths.SelectPrimaryLeftTrainPath(TimeSchedule.MasterTrainPaths.PrimarySelectedTrainPaths.First()))
                {
                    MessageBox.Show("Левее нет ниток.");
                }
            }
            else
            {
                MessageBox.Show("Должна быть выделена ровно одна нитка.");
            }
        }

        private void RightTrainPath_Click(object sender, RoutedEventArgs e)
        {
            if (TimeSchedule.MasterTrainPaths.PrimarySelectedTrainPaths.Count == 1)
            {
                if (!TimeSchedule.MasterTrainPaths.SelectPrimaryRightTrainPath(TimeSchedule.MasterTrainPaths.PrimarySelectedTrainPaths.First()))
                {
                    MessageBox.Show("Левее нет ниток.");
                }
            }
            else
            {
                MessageBox.Show("Должна быть выделена ровно одна нитка.");
            }
        }

		private void MenuItem_Click(object sender, RoutedEventArgs e)
		{

		}

		private void EditNightStatePointsItemNew_Click(object sender, RoutedEventArgs e)
		{
			frmEditNightStatePointsWpf frm = new frmEditNightStatePointsWpf();
			frm.Owner = this;
			frm.ShowDialog();
		}
	}
}