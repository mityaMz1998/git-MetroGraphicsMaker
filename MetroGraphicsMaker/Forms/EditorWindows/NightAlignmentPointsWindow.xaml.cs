using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Data;
using System.Dynamic;
using Core;
using Converters;

using System.Reflection;
using Word = Microsoft.Office.Interop.Word;
using Excel = Microsoft.Office.Interop.Excel;
using System.Data.OleDb;

namespace BD_NightAlignment
{
	/// <summary>
	/// Логика взаимодействия для NightAlignmentPointsWindow.xaml
	/// </summary>
	public partial class NightAlignmentPointsWindow : Window
	{
		public Boolean flgHaveChanges = false;
		public Boolean flgCreatingNewPoinerProcess = false;
		public Boolean flgEditingCurrentPoiner = false;
		//
		public Boolean flgCopyingCurrentPoiner = false;

		public Dictionary<UInt32, String> stageDict = new Dictionary<UInt32, String>();

		public List<NightStayPoint> nightStayPointers = new List<NightStayPoint>();
		public List<UInt32> pointersCodesForSelectedStation = new List<UInt32>();

		public List<NightStayPoint> pointersForSelectedStation = new List<NightStayPoint>();

		public NightStayPoint currentNsp;

		public UInt32 selectedLineCode;
		public String selectedLineName;

		public String separator = " ---> ";

		public NightAlignmentPointsWindow()
		{
			InitializeComponent();
			//Show();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{

			comboBoxLine.ItemsSource = MovementSchedule.colLine;

			//ListOfStagesFill();

			ComboBoxАrrangemenFill();

			IsReadOnlyElements(true);

			textBoxRename.Visibility = Visibility.Hidden;
			labelRename.Visibility = Visibility.Hidden;

			buttonCancel.IsEnabled = false;
			buttonSave.IsEnabled = false;
			comboBoxStage.IsEnabled = false;
			comboBoxArrangement.IsEnabled = false;
			//
			//comboBoxDepot.IsEnabled = false;
			//comboBoxBelongingDepot.IsEnabled = false;
			//
			label.Content = "Режим чтения";

			NSPFill();
		}

		public void NSPFill()
		{
			nightStayPointers.Clear();
			foreach (var nsp in MovementSchedule.colNightStayPoint)
			{
				nightStayPointers.Add(nsp);
			}
		}

		private void comboBoxLine_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			selectedLineName = (comboBoxLine.SelectedItem != null) ? comboBoxLine.SelectedItem.ToString() : null;

			if (selectedLineName != null)
			{
				if (!flgCreatingNewPoinerProcess)
					ClearElements();

				comboBoxStation.Items.Clear();

				comboBoxStation.IsEnabled = true;

				SearchSelectedLineCode(selectedLineName);

				foreach (var station in MovementSchedule.colStation)
				{
					if (selectedLineCode == station.line.code)
					{
						comboBoxStation.Items.Add(station.name);
					}
				}
			}
		}

		public UInt32 selectedStationCode;
		public static String selectedStationName;
		public UInt32 selectedStageCode;

		public static string getStationName()
		{
			string _stationName = selectedStationName;
			return _stationName;
		}
		public Boolean flgComboBoxPointerShouldChange = false;

		private void comboBoxStation_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			selectedStationName = (comboBoxStation.SelectedItem != null) ? comboBoxStation.SelectedItem.ToString() : null;
			if (selectedStationName != null && !flgCreatingNewPoinerProcess && !flgEditingCurrentPoiner && !flgCopyingCurrentPoiner) ////
			{
				comboBoxPointer.IsEnabled = true;

				flgComboBoxPointerShouldChange = false;

				ClearElements();

				flgComboBoxPointerShouldChange = true;

				comboBoxStage.IsEnabled = true;
				comboBoxArrangement.IsEnabled = true;
				//
				//comboBoxDepot.IsEnabled = true;
				//comboBoxBelongingDepot.IsEnabled = true;
				//
				SearchSelectedStationCode(selectedStationName);

				Station lst = MovementSchedule.colStation[Convert.ToInt32(selectedStationCode) - 1];
				pointersForSelectedStation = MovementSchedule.giveStationPointers(lst);
				foreach (var nsp in pointersForSelectedStation)
				{
					comboBoxPointer.Items.Add(nsp.code + separator + nsp.name);
					pointersCodesForSelectedStation.Add(nsp.code);
				}

				//foreach (var nsp in nightStayPointers)
				//{
				//    if (selectedStationCode == nsp.station.code)
				//    {
				//        comboBoxPointer.Items.Add(nsp.code + separator + nsp.name);

				//        pointersCodesForSelectedStation.Add(nsp.code);
				//        pointersForSelectedStation.Add(nsp);
				//    }
				//}

				comboBoxStage.Items.Clear();
				if (selectedStationName != null)
				{
					foreach (var task in MovementSchedule.colTask)
					{
						if (task.departureStation.name == selectedStationName || task.destinationStation.name == selectedStationName)
						{
							var stageString = task.departureStation.name + separator + task.destinationStation.name;

							//stageDict.Add(task.code, stageString);

							comboBoxStage.Items.Add(stageString);
						}
					}
				}

				if (comboBoxPointer.Items.Count == 0)
				{
					comboBoxStage.SelectedIndex = -1;
					comboBoxArrangement.SelectedIndex = -1;
					comboBoxPointer.IsEnabled = false;
					IsEnableElements(false);
				}
				else
				{
					comboBoxPointer.IsEnabled = true;
					comboBoxPointer.SelectedIndex = 0;
				}
			}
		}

		public static string selectedPointer;
		public static string getPointerName()
		{
			string _selectedPointer = selectedPointer;
			return _selectedPointer;
		}

		private void comboBoxPointer_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (flgComboBoxPointerShouldChange)
			{
				var selectedIndex = comboBoxPointer.SelectedIndex;
				selectedPointer = (comboBoxPointer.SelectedItem != null)
					? comboBoxPointer.SelectedItem.ToString()
					: null;
				foreach (var nsp in nightStayPointers)
				{
					if (pointersCodesForSelectedStation[selectedIndex] == nsp.code)
					{
						currentNsp = nsp;
						break;
					}
				}

				if (currentNsp.task != null)
				{
					foreach (var task in MovementSchedule.colTask)
					{
						if (currentNsp.task.code == task.code)
						{
							var stageForCurrentPointer = (task != null) ? task.departureStation.name + separator + task.destinationStation.name : null;

							for (var j = 0; j < comboBoxStage.Items.Count; j++)
							{
								if (comboBoxStage.Items[j].ToString() == stageForCurrentPointer)
								{
									comboBoxStage.SelectedIndex = j;

									break;
								}
							}
							break;
						}
					}

				}
				else //Если у данного указателя не указан перегон
				{
					comboBoxStage.SelectedIndex = -1;
				}

				textBoxCapacity.Text = currentNsp.Capacity.ToString();
				textBoxPicket.Text = currentNsp.Picket.ToString();
				textBoxTrack.Text = currentNsp.Track.ToString();
				textBoxCoordinateMorning.Text = currentNsp.MorningCoordinate.ToString();
				textBoxCoordinateEvening.Text = currentNsp.EveningCoordinate.ToString();
				textBoxExitDirectionEven.Text = currentNsp.ExitDirectionEven.ToString();
				textBoxExitDirectionUneven.Text = currentNsp.ExitDirectionUneven.ToString();


				checkBoxReserve.IsChecked = currentNsp.flgReserve ? true : false;

				checkBoxGoBeforeFirstTrainEven.IsChecked = currentNsp.flgGoBeforeFirstTrainEven ? true : false;
				checkBoxGoBeforeFirstTrainUneven.IsChecked = currentNsp.flgGoBeforeFirstTrainUneven ? true : false;

				checkBoxGoAfterLastTrainEven.IsChecked = currentNsp.flgGoAfterLastTrainEven ? true : false;
				checkBoxGoAfterLastTrainUneven.IsChecked = currentNsp.flgGoAfterLastTrainUneven ? true : false;


				textBoxTimeToExitOnLine.Text = TimeConverter.SecondsToString(currentNsp.TimeToExitOnLine);

				textBoxStartEven.Text = TimeConverter.SecondsToString(currentNsp.StartEven);
				textBoxStartUneven.Text = TimeConverter.SecondsToString(currentNsp.StartUneven);

				textBoxEndEven.Text = TimeConverter.SecondsToString(currentNsp.EndEven);
				textBoxEndUneven.Text = TimeConverter.SecondsToString(currentNsp.EndUneven);


				comboBoxArrangement.SelectedIndex = currentNsp.Arrangement;
				//
				//comboBoxDepot.SelectedIndex = currentNsp.depot;
				//comboBoxBelongingDepot.SelectedIndex = currentNsp.depot;
				//
			}
		}

		/// <summary>
		/// Заполнение списка и словаря перегонов
		/// </summary>
		public void ListOfStagesFill()
		{
			//comboBoxStage.Items.Add("не выбрано");

			foreach (var task in MovementSchedule.colTask)
			{
				var stageString = task.departureStation.name + separator + task.destinationStation.name;

				stageDict.Add(task.code, stageString);

				comboBoxStage.Items.Add(stageString);
			}
		}

		/// <summary>
		/// Заполнение списка расстановок
		/// </summary>
		public void ComboBoxАrrangemenFill()
		{
			comboBoxArrangement.Items.Add("0");
			comboBoxArrangement.Items.Add("1");
			comboBoxArrangement.Items.Add("2");
		}

		/// <summary>
		/// Приведение элементов формы в состояние Enabled || Disabled
		/// </summary>
		/// <param name="isEnabled"></param>
		public void IsEnableElements(bool isEnabled)
		{
			comboBoxStage.IsEnabled = isEnabled;
			comboBoxArrangement.IsEnabled = isEnabled;

			checkBoxReserve.IsChecked = isEnabled;

			checkBoxGoBeforeFirstTrainEven.IsChecked = isEnabled;
			checkBoxGoBeforeFirstTrainUneven.IsChecked = isEnabled;

			checkBoxGoAfterLastTrainEven.IsChecked = isEnabled;
			checkBoxGoAfterLastTrainUneven.IsChecked = isEnabled;
		}

		/// <summary>
		/// Приведение элементов формы в состояние ReadOnly
		/// </summary>
		/// <param name="isReadOnly"></param>
		public void IsReadOnlyElements(bool isReadOnly)
		{
			textBoxCapacity.IsReadOnly = isReadOnly;
			textBoxCoordinateEvening.IsReadOnly = isReadOnly;
			textBoxCoordinateMorning.IsReadOnly = isReadOnly;
			textBoxPicket.IsReadOnly = isReadOnly;
			textBoxTrack.IsReadOnly = isReadOnly;
			textBoxTimeToExitOnLine.IsReadOnly = isReadOnly;

			checkBoxReserve.IsEnabled = !isReadOnly;

			textBoxStartEven.IsReadOnly = isReadOnly;
			textBoxStartUneven.IsReadOnly = isReadOnly;

			textBoxEndEven.IsReadOnly = isReadOnly;
			textBoxEndUneven.IsReadOnly = isReadOnly;

			textBoxExitDirectionEven.IsReadOnly = isReadOnly;
			textBoxExitDirectionUneven.IsReadOnly = isReadOnly;

			checkBoxGoBeforeFirstTrainEven.IsEnabled = !isReadOnly;
			checkBoxGoBeforeFirstTrainUneven.IsEnabled = !isReadOnly;

			checkBoxGoAfterLastTrainEven.IsEnabled = !isReadOnly;
			checkBoxGoAfterLastTrainUneven.IsEnabled = !isReadOnly;

			textBoxRename.IsReadOnly = isReadOnly;
		}

		/// <summary>
		/// Очистка элементов формы
		/// </summary>
		public void ClearElements()
		{
			flgComboBoxPointerShouldChange = false;

			pointersCodesForSelectedStation.Clear();

			comboBoxPointer.Items.Clear();

			textBoxCoordinateMorning.Clear();
			textBoxCoordinateEvening.Clear();
			textBoxCapacity.Clear();
			textBoxPicket.Clear();
			textBoxTrack.Clear();

			textBoxTimeToExitOnLine.Clear();

			textBoxStartEven.Clear();
			textBoxStartUneven.Clear();

			textBoxEndEven.Clear();
			textBoxEndUneven.Clear();

			textBoxExitDirectionEven.Clear();
			textBoxExitDirectionUneven.Clear();
		}

		private void buttonClose_Click(object sender, RoutedEventArgs e)
		{
			if (flgHaveChanges)
			{
				var res = MessageBox.Show("Сохранить изменения перед выходом?", "Редактирование указателей ночной расстановки", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);

				if (res == MessageBoxResult.Yes)
				{
					//SaveChanges();
					this.Close();
				}
				else if (res == MessageBoxResult.No)
					this.Close();
			}

			else
			{
				var res = MessageBox.Show("Вы уверены, что хотите выйти?", "Выход", MessageBoxButton.YesNo, MessageBoxImage.Question);

				if (res == MessageBoxResult.Yes)
					this.Close();
			}
		}

		public void FillNSPObject(NightStayPoint nsp)
		{
			nsp.line = new Core.Line();
			selectedLineName = comboBoxLine.SelectedItem.ToString();
			SearchSelectedLineCode(selectedLineName);
			nsp.line.code = selectedLineCode;

			if (textBoxRename.Text != "")
				nsp.name = textBoxRename.Text;
			else
				MessageBox.Show("Введите название указателя!");

			nsp.name = textBoxRename.Text;

			selectedStationName = comboBoxStation.SelectedItem.ToString();
			SearchSelectedStationCode(selectedStationName);
			nsp.station = new Station { code = selectedStationCode };

			nsp.task = new Core.Task();

			var selectedStage = (comboBoxStage.SelectedValue != null) ? comboBoxStage.SelectedValue.ToString() : null;

			foreach (var item in stageDict)
			{
				if (item.Value == selectedStage)
				{
					nsp.task.code = item.Key;
					selectedStageCode = item.Key;
					break;
				}
			}

			nsp.Arrangement = comboBoxArrangement.SelectedIndex;

			nsp.flgReserve = (checkBoxReserve.IsChecked == true) ? true : false;

			try
			{
				nsp.MorningCoordinate = Convert.ToInt32(textBoxCoordinateMorning.Text);
				nsp.EveningCoordinate = Convert.ToInt32(textBoxCoordinateEvening.Text);

				nsp.Picket = Convert.ToInt32(textBoxPicket.Text);
				nsp.Capacity = Convert.ToInt32(textBoxCapacity.Text);
				nsp.Track = Convert.ToInt32(textBoxTrack.Text);

				nsp.ExitDirectionEven = Convert.ToInt32(textBoxExitDirectionEven.Text);
				nsp.ExitDirectionUneven = Convert.ToInt32(textBoxExitDirectionUneven.Text);
			}
			catch
			{
				MessageBox.Show("Некоторые поля имеют неверные значения!", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
			}

			nsp.TimeToExitOnLine = TimeConverter.StringToSeconds(textBoxTimeToExitOnLine.Text);

			nsp.StartEven = TimeConverter.StringToSeconds(textBoxStartEven.Text);
			nsp.StartUneven = TimeConverter.StringToSeconds(textBoxStartUneven.Text);

			nsp.EndEven = TimeConverter.StringToSeconds(textBoxEndEven.Text);
			nsp.EndUneven = TimeConverter.StringToSeconds(textBoxEndUneven.Text);

			nsp.flgGoBeforeFirstTrainEven = (checkBoxGoBeforeFirstTrainEven.IsChecked == true) ? true : false;
			nsp.flgGoBeforeFirstTrainUneven = (checkBoxGoBeforeFirstTrainUneven.IsChecked == true) ? true : false;

			nsp.flgGoAfterLastTrainEven = (checkBoxGoAfterLastTrainEven.IsChecked == true) ? true : false;
			nsp.flgGoAfterLastTrainUneven = (checkBoxGoAfterLastTrainUneven.IsChecked == true) ? true : false;
		}

		private void button_Click(object sender, RoutedEventArgs e)
		{
			//SaveChanges();
		}

		public void SaveChanges(object[] Values)
		{
			if (flgCreatingNewPoinerProcess)
			{////
				//var nsp = new NightStayPoint { code = SearchUniqueCode() };

				//FillNSPObject(nsp);
				
				//MovementSchedule.colNightStayPoint.Add(nsp);

				DataTable myTable = new DataTable("Точки_ночного_отстоя");

				OleDbConnection mySqlCon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; Data Source = 2009 — new.mdb");
				OleDbCommand mySqlCmd = new OleDbCommand("SELECT * FROM" + "[" + myTable + "]");
				mySqlCmd.Connection = mySqlCon;

				mySqlCon.Open();
				OleDbDataAdapter adapter = new OleDbDataAdapter(mySqlCmd);
				adapter.Fill(myTable);

				DataRow newRow = myTable.NewRow();
				int mx = 1;
				foreach (DataRow dr in myTable.Rows)
					mx = Math.Max(mx, int.Parse(dr.ItemArray[0].ToString()));
				////
				string nl = "null";
				string sqlExpression = String.Format("INSERT INTO [Точки_ночного_отстоя] ([Код], [Название], [Станция], [Перегон], [Координата_утро], [Координата_вечер], [Пикетаж], [Путь], " +
														"[Предыдущая_точка_нечетная], [Предыдущая_точка_четная], [Резервная], [Депо], [Принадлежность_депо], [Емкость_точки], [Принадлежность_расстановке], " +
														"[Время_движения_до_выхода_на_линию], [Линия], [Начало_выпуска_из точки_нечетное], [Конец_выпуска_из точки_нечетное], [Выйти_до_первого_пассажирского_нечетное], " +
														"[Начало_выпуска_из точки_четное], [Конец_выпуска_из точки_четное], [Выйти_до_первого_пассажирского_четное], [Направление_выхода_нечетное], [Направление_выхода_четное], " +
														"[Зайти_после_последнего_пассажирского_нечетное], [Зайти_после_последнего_пассажирского_четное]) VALUES ({0},'{1}',{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12}" +
														",{13},{14},'{15}',{16},'{17}','{18}',{19},'{20}','{21}',{22},{23},{24},{25},{26});", mx + 1, textBoxRename.Text, selectedStationCode.ToString(),
														selectedStageCode.ToString(), textBoxCoordinateMorning.Text, textBoxCoordinateEvening.Text, textBoxPicket.Text, textBoxTrack.Text, nl, nl,
														checkBoxReserve.IsChecked.ToString(), nl, nl, textBoxCapacity.Text, comboBoxArrangement.Text, textBoxTimeToExitOnLine.Text.ToString(),
														selectedLineCode.ToString(), textBoxStartUneven.Text.ToString(), textBoxEndUneven.Text.ToString(), checkBoxGoBeforeFirstTrainUneven.IsChecked.ToString(),
														textBoxStartEven.Text.ToString(), textBoxEndEven.Text.ToString(), checkBoxGoBeforeFirstTrainEven.IsChecked.ToString(), textBoxExitDirectionUneven.Text,
														textBoxExitDirectionEven.Text, checkBoxGoAfterLastTrainUneven.IsChecked.ToString(), checkBoxGoAfterLastTrainEven.IsChecked.ToString());
				
				OleDbCommand command = new OleDbCommand(sqlExpression, mySqlCon);
				adapter.Update(myTable);
				int number = command.ExecuteNonQuery();

				//comboBoxPointer.Items.Add((mx + 1) + separator + textBoxRename.Text);
				//ResetFormElements();
				//NightStayPoint ns = new NightStayPoint();
				//ns.code = (uint)(mx + 1);
				//MovementSchedule.colNightStayPoint.Add(ns);
				MessageBox.Show("Новый указатель успешно добавлен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);

			}

			if (flgEditingCurrentPoiner)
			{
				//var index = 0;

				//for (var i = 0; i < MovementSchedule.colNightStayPoint.Count; i++)
				//{
				//	if (currentNsp.code == MovementSchedule.colNightStayPoint[i].code)
				//	{
				//		index = i;
				//		break;
				//	}

				//}
				////
				//var NSP = MovementSchedule.colNightStayPoint[(int)currentNsp.code];

				//FillNSPObject(NSP);

				DataTable myTable = new DataTable("Точки_ночного_отстоя");

				OleDbConnection mySqlCon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; Data Source = 2009 — new.mdb");

				OleDbCommand mySqlCmd = new OleDbCommand("SELECT * FROM" + "[" + myTable + "]");
				mySqlCmd.Connection = mySqlCon;

				mySqlCon.Open();
				OleDbDataAdapter adapter = new OleDbDataAdapter(mySqlCmd);
				adapter.Fill(myTable);

				//string cn = (listView1.SelectedItem as ListViewItem).Tag.ToString();
				string nl = "null";
				string sqlExpr = String.Format("UPDATE [Точки_ночного_отстоя] SET [Название] = '{0}', [Станция] = {1}, [Перегон] = {2}, [Координата_утро] = {3}, [Координата_вечер] = {4}, [Пикетаж] = {5}, [Путь] = {6}, " +
													"[Предыдущая_точка_нечетная] = {7}, [Предыдущая_точка_четная] = {8}, [Резервная] = {9}, [Депо] = {10}, [Принадлежность_депо] = {11}, [Емкость_точки] = {12}, [Принадлежность_расстановке] = {13}, " +
													"[Время_движения_до_выхода_на_линию] = '{14}', [Линия] = {15}, [Начало_выпуска_из точки_нечетное] = '{16}', [Конец_выпуска_из точки_нечетное] = '{17}', [Выйти_до_первого_пассажирского_нечетное] = {18}, " +
													"[Начало_выпуска_из точки_четное] = '{19}', [Конец_выпуска_из точки_четное] = '{20}', [Выйти_до_первого_пассажирского_четное] = {21}, [Направление_выхода_нечетное] = {22}, [Направление_выхода_четное] = {23}, " +
													"[Зайти_после_последнего_пассажирского_нечетное] = {24}, [Зайти_после_последнего_пассажирского_четное] = {25} WHERE [Код]= " + (int)currentNsp.code, textBoxRename.Text, selectedStationCode.ToString(),
													selectedStageCode.ToString(), textBoxCoordinateMorning.Text, textBoxCoordinateEvening.Text, textBoxPicket.Text, textBoxTrack.Text, nl, nl,
													checkBoxReserve.IsChecked.ToString(), nl, nl, textBoxCapacity.Text, comboBoxArrangement.Text, textBoxTimeToExitOnLine.Text.ToString(),
													selectedLineCode.ToString(), textBoxStartUneven.Text.ToString(), textBoxEndUneven.Text.ToString(), checkBoxGoBeforeFirstTrainUneven.IsChecked.ToString(),
													textBoxStartEven.Text.ToString(), textBoxEndEven.Text.ToString(), checkBoxGoBeforeFirstTrainEven.IsChecked.ToString(), textBoxExitDirectionUneven.Text,
													textBoxExitDirectionEven.Text, checkBoxGoAfterLastTrainUneven.IsChecked.ToString(), checkBoxGoAfterLastTrainEven.IsChecked.ToString());

				OleDbCommand command = new OleDbCommand(sqlExpr, mySqlCon);
				adapter.Update(myTable);
				command.ExecuteNonQuery();

				MessageBox.Show("Указатель успешно изменен!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
			}

			if (flgCopyingCurrentPoiner)
			{
				//var index = 0;

				//for (var i = 0; i < MovementSchedule.colNightStayPoint.Count; i++)
				//{
				//	if (currentNsp.code == MovementSchedule.colNightStayPoint[i].code)
				//	{
				//		index = i;
				//		break;
				//	}

				//}

				//var NSP = MovementSchedule.colNightStayPoint[(int) currentNsp.code];

				//FillNSPObject(NSP);

				DataTable myTable = new DataTable("Точки_ночного_отстоя");

				OleDbConnection mySqlCon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; Data Source = 2009 — new.mdb");

				OleDbCommand mySqlCmd = new OleDbCommand("SELECT * FROM" + "[" + myTable + "]");
				mySqlCmd.Connection = mySqlCon;

				mySqlCon.Open();
				OleDbDataAdapter adapter = new OleDbDataAdapter(mySqlCmd);
				adapter.Fill(myTable);

				DataRow newRow = myTable.NewRow();
				int mx = 1;
				foreach (DataRow dr in myTable.Rows)
					mx = Math.Max(mx, int.Parse(dr.ItemArray[0].ToString()));

				string sqlExpr = String.Format("INSERT INTO [Точки_ночного_отстоя] ([Код], [Название], [Станция], [Перегон], [Координата_утро], [Координата_вечер], [Пикетаж], [Путь], " +
													"[Предыдущая_точка_нечетная], [Предыдущая_точка_четная], [Резервная], [Депо], [Принадлежность_депо], [Емкость_точки], [Принадлежность_расстановке], " +
													"[Время_движения_до_выхода_на_линию], [Линия], [Начало_выпуска_из точки_нечетное], [Конец_выпуска_из точки_нечетное], [Выйти_до_первого_пассажирского_нечетное], " +
													"[Начало_выпуска_из точки_четное], [Конец_выпуска_из точки_четное], [Выйти_до_первого_пассажирского_четное], [Направление_выхода_нечетное], [Направление_выхода_четное], " +
													"[Зайти_после_последнего_пассажирского_нечетное], [Зайти_после_последнего_пассажирского_четное]) SELECT " + (mx + 1) + ", [Название], [Станция], [Перегон], [Координата_утро], [Координата_вечер], [Пикетаж], [Путь], " +
													"[Предыдущая_точка_нечетная], [Предыдущая_точка_четная], [Резервная], [Депо], [Принадлежность_депо], [Емкость_точки], [Принадлежность_расстановке], " +
													"[Время_движения_до_выхода_на_линию], [Линия], [Начало_выпуска_из точки_нечетное], [Конец_выпуска_из точки_нечетное], [Выйти_до_первого_пассажирского_нечетное], " +
													"[Начало_выпуска_из точки_четное], [Конец_выпуска_из точки_четное], [Выйти_до_первого_пассажирского_четное], [Направление_выхода_нечетное], [Направление_выхода_четное], " +
													"[Зайти_после_последнего_пассажирского_нечетное], [Зайти_после_последнего_пассажирского_четное] FROM [Точки_ночного_отстоя] WHERE [Код] = " + (int)currentNsp.code);

				OleDbCommand command = new OleDbCommand(sqlExpr, mySqlCon);
				adapter.Update(myTable);
				command.ExecuteNonQuery();

				MessageBox.Show("Выбранный Вами указатель успешно скопирован!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
			}

			ResetFormElements();

			buttonCreateNewPointer.IsEnabled = true;
			buttonEdit.IsEnabled = true;
			buttonDelete.IsEnabled = true;
			buttonCopy.IsEnabled = true;

			buttonExportExcel.IsEnabled = true;
			buttonExportWord.IsEnabled = true;

			flgCreatingNewPoinerProcess = false;
			flgEditingCurrentPoiner = false;
			flgCopyingCurrentPoiner = false; ////

			flgHaveChanges = false;

			NSPFill();
		}

		public enum Operations { Creating, Editing, Copying };

		public void ConfigElements(Operations key)
		{
			switch (key)
			{
				case Operations.Creating:
					label.Content = "Режим создания нового указателя";

					flgCreatingNewPoinerProcess = true;

					ClearElements();

					IsEnableElements(false);

					comboBoxLine.SelectedIndex = -1;

					comboBoxStage.SelectedIndex = -1;

					comboBoxPointer.SelectedIndex = -1;

					comboBoxArrangement.SelectedIndex = -1;

					comboBoxStation.Items.Clear();
					comboBoxStation.IsEnabled = false;

					textBoxRename.Clear();
					break;

				case Operations.Editing:
					label.Content = "Режим изменения данных о текущем указателе";

					flgEditingCurrentPoiner = true;

					textBoxRename.Text = currentNsp.name;
					break;

				case Operations.Copying:
					label.Content = "Создание нового указателя на основе скопированного";

					flgCopyingCurrentPoiner = true; ////

					textBoxRename.Text = currentNsp.name;
					break;
			}

			flgHaveChanges = true;

			IsReadOnlyElements(false);

			comboBoxPointer.IsEnabled = false;

			comboBoxStage.IsEnabled = true;

			comboBoxArrangement.IsEnabled = true;

			buttonCreateNewPointer.IsEnabled = false;
			buttonEdit.IsEnabled = false;
			buttonDelete.IsEnabled = false;
			buttonCopy.IsEnabled = false;
			buttonSave.IsEnabled = true;
			buttonCancel.IsEnabled = true;

			buttonExportExcel.IsEnabled = false;
			buttonExportWord.IsEnabled = false;

			labelRename.Visibility = Visibility.Visible;

			textBoxRename.Visibility = Visibility.Visible;
			textBoxRename.IsReadOnly = false;

			textBoxRename.Focus();
			textBoxRename.SelectionStart = 0;
			textBoxRename.SelectionLength = textBoxRename.Text.Length;
		}

		private void buttonCreateNewPointer_Click(object sender, RoutedEventArgs e)
		{
			ConfigElements(Operations.Creating);
		}

		private void buttonEdit_Click(object sender, RoutedEventArgs e)
		{
			if (comboBoxPointer.Items.Count != 0)
				ConfigElements(Operations.Editing);
			else
				MessageBox.Show("Указатель не выбран!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		private void buttonCopy_Click(object sender, RoutedEventArgs e)
		{
			if (comboBoxPointer.Items.Count != 0)
				ConfigElements(Operations.Copying);
			else
				MessageBox.Show("Указатель не выбран!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
		}

		private void buttonDelete_Click(object sender, RoutedEventArgs e)
		{
			if (comboBoxPointer.Items.Count != 0)
			{
				var res = MessageBox.Show("Вы действительно хотите безвозвратно удалить этот указатель?", "Удалить указатель", MessageBoxButton.YesNo, MessageBoxImage.Question);

				if (res == MessageBoxResult.Yes)
				{
					//var index = 0;

					//for (var i = 0; i < MovementSchedule.colNightStayPoint.Count; i++)
					//{
					//	if (currentNsp.code == MovementSchedule.colNightStayPoint[i].code)
					//	{
					//		index = (int) currentNsp.code;
					//		break;
					//	}
					//}

					//MovementSchedule.colNightStayPoint.Remove(MovementSchedule.colNightStayPoint[(int) currentNsp.code]);

					//NSPFill();

					DataTable myTable = new DataTable("Точки_ночного_отстоя");

					OleDbConnection mySqlCon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; Data Source = 2009 — new.mdb");

					OleDbCommand mySqlCmd = new OleDbCommand("SELECT * FROM" + "[" + myTable + "]");
					mySqlCmd.Connection = mySqlCon;
					mySqlCon.Open();

					OleDbDataAdapter adapter = new OleDbDataAdapter(mySqlCmd);
					adapter.Fill(myTable);

					//string idstr = (listView1.SelectedItem as ListViewItem).Tag.ToString();
					string sqlExpr = String.Format("DELETE FROM [Точки_ночного_отстоя] WHERE [Код] = " + (int)currentNsp.code);

					OleDbCommand command = new OleDbCommand(sqlExpr, mySqlCon);
					adapter.Update(myTable);
					command.ExecuteNonQuery();

					MessageBox.Show("Указатель удален!", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
					ResetFormElements();
				}
			}
			else
			{
				MessageBox.Show("Указатель не выбран!", "Предупреждение", MessageBoxButton.OK, MessageBoxImage.Warning);
			}

		}

		private void buttonCancel_Click(object sender, RoutedEventArgs e)
		{

			flgCreatingNewPoinerProcess = false;
			flgEditingCurrentPoiner = false;
			flgCopyingCurrentPoiner = false; ////
			flgHaveChanges = false;

			ResetFormElements();
		}

		private void buttonSaved_Click(object sender, RoutedEventArgs e)
		{
			SaveChanges(new string[] { textBoxRename.Text, selectedStationCode.ToString(), selectedStageCode.ToString(), textBoxCoordinateMorning.Text,
			textBoxCoordinateEvening.Text, textBoxPicket.Text, textBoxTrack.Text, null, null, checkBoxReserve.IsChecked.ToString(), null, null, textBoxCapacity.Text,
			comboBoxArrangement.Text, textBoxTimeToExitOnLine.Text.ToString(), selectedLineCode.ToString(), textBoxStartUneven.Text.ToString(),
			textBoxEndUneven.Text.ToString(), checkBoxGoBeforeFirstTrainUneven.IsChecked.ToString(), textBoxStartEven.Text.ToString(), textBoxEndEven.Text.ToString(),
			checkBoxGoBeforeFirstTrainEven.IsChecked.ToString(), textBoxExitDirectionUneven.Text, textBoxExitDirectionEven.Text, checkBoxGoAfterLastTrainUneven.IsChecked.ToString(),
			checkBoxGoAfterLastTrainEven.IsChecked.ToString() });
		}
		public void ResetFormElements()
		{
			ClearElements();
			IsReadOnlyElements(true);
			IsEnableElements(false);

			comboBoxLine.SelectedIndex = -1;

			comboBoxPointer.SelectedIndex = -1;
			comboBoxPointer.IsReadOnly = true;
			comboBoxPointer.IsEnabled = false;

			comboBoxStage.SelectedIndex = -1;
			comboBoxStage.IsReadOnly = true;
			comboBoxStage.IsEnabled = false;

			comboBoxStation.Items.Clear();
			comboBoxStation.IsEnabled = false;

			comboBoxArrangement.SelectedIndex = -1;
			comboBoxArrangement.IsEnabled = false;

			labelRename.Visibility = Visibility.Hidden;

			textBoxRename.Visibility = Visibility.Hidden;
			textBoxRename.IsReadOnly = true;
			textBoxRename.Clear();

			textBoxRename.Clear();

			buttonEdit.IsEnabled = true;
			buttonDelete.IsEnabled = true;
			buttonCopy.IsEnabled = true;

			buttonExportExcel.IsEnabled = true;
			buttonExportWord.IsEnabled = true;

			buttonSave.IsEnabled = false;

			label.Content = "Режим чтения";

			buttonCancel.IsEnabled = false;
			buttonCreateNewPointer.IsEnabled = true;
		}

		/// <summary>
		/// Возвращает уникальный Код
		/// </summary>
		/// <returns>Уникальный Код</returns>
		static UInt32 SearchUniqueCode()
		{
			UInt32 max = 0;

			foreach (var nsp in MovementSchedule.colNightStayPoint)
			{
				var current = nsp.code;

				if (current > max)
					max = current;
			}

			return max + 1;
		}

		/// <summary>
		/// Поиск Кода выбранной Линии
		/// </summary>
		/// <param name="lineName">Название Линии, Код которой требуется определить</param>
		public void SearchSelectedLineCode(String lineName)
		{
			foreach (var line in MovementSchedule.colLine.Where(line => lineName == line.name))
			{
				selectedLineCode = line.code;
				break;
			}
		}

		/// <summary>
		/// Поиск Кода выбранной Станции
		/// </summary>
		/// <param name="stationName">Название Станции, Код которой требуется определить</param>
		public void SearchSelectedStationCode(String stationName)
		{
			foreach (var station in MovementSchedule.colStation.Where(station => stationName == station.name))
			{
				selectedStationCode = station.code;
				break;
			}
		}

		object ObjMissing = Missing.Value;

		private void buttonExportWord_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Word._Application ObjWord = new Word.Application();

				wordCore(ObjMissing, ObjWord);

				ObjWord.Quit();
			}
			catch
			{
				MessageBox.Show("Ошибка");
			}
		}

		private void buttonExportExcel_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				Excel._Application ObjExcel = new Excel.Application();
				excelCore(ObjMissing, ObjExcel);
				//ObjExcel.Quit();
			}
			catch
			{

				MessageBox.Show("Ошибка экспорта в Excel");
			}
		}

		void excelCore(object ObjMissing, Excel._Application ObjExcel)
		{
			ObjExcel.Visible = true;

			Excel.Workbook ObjWorkbook = ObjExcel.Workbooks.Add(Missing.Value);
			Excel.Worksheet ObjWorkSheet = ObjWorkbook.Sheets[1];

			Excel.Range cells = ObjWorkSheet.get_Range("A1", "B" + (pointersForSelectedStation.Count + 1));

			cells.ColumnWidth = 30;
			cells.Borders.Weight = 2;

			cells.Cells[1, 1] = "Название указателя";
			cells.Cells[1, 2] = "Перегон";

			for (int i = 0; i < pointersForSelectedStation.Count; i++)
			{
				var nsp = pointersForSelectedStation[i];

				cells[i + 2, 1] = nsp.name;

				if (nsp.task != null)
					cells[i + 2, 2] = nsp.task.departureStation.name + separator + nsp.task.destinationStation.name;
				else
					cells[i + 2, 2] = "";
			}

			//ObjExcel.UserControl = true;
			//ObjWorkbook.SaveCopyAs("Pointers" + ".xlsx");
			//ObjWorkbook.Close(false, "", Missing.Value);
		}

		void wordCore(object ObjMissing, Word._Application ObjWord)
		{
			object EndOfDoc = "\\endofdoc";
			ObjWord.Visible = false;
			Word._Document ObjDoc = ObjWord.Application.Documents.Add();
			ObjDoc.Activate();

			object ObjRange = ObjDoc.Bookmarks.get_Item(ref EndOfDoc).Range;
			Word.Paragraph ObjParagraph = ObjDoc.Content.Paragraphs.Add(ref ObjRange);

			ObjParagraph.Range.Text = "Название указателя: " + currentNsp.name;
			ObjParagraph.Format.LeftIndent = 0;
			ObjParagraph.Range.Font.Bold = 0;
			ObjParagraph.Range.Font.AllCaps = 0;
			ObjParagraph.Range.Font.Size = 16;
			ObjParagraph.Alignment = Word.WdParagraphAlignment.wdAlignParagraphLeft;

			ObjParagraph.Range.InsertParagraphAfter();
			ObjParagraph.Range.InsertAfter("Код в базе данных: " + currentNsp.code);

			ObjParagraph.Range.InsertParagraphAfter();
			ObjParagraph.Range.InsertAfter("Линия: " + currentNsp.line.name);

			ObjParagraph.Range.InsertParagraphAfter();
			ObjParagraph.Range.InsertAfter("Станция: " + currentNsp.station.name);

			ObjParagraph.Range.InsertParagraphAfter();
			ObjParagraph.Range.InsertAfter("Перегон: " + currentNsp.task.departureStation.name + separator + currentNsp.task.destinationStation.name);

			ObjParagraph.Range.InsertParagraphAfter();
			ObjParagraph.Range.InsertAfter("Координата утро: " + currentNsp.MorningCoordinate);
			ObjParagraph.Range.InsertParagraphAfter();
			ObjParagraph.Range.InsertAfter("Координата вечер: " + currentNsp.EveningCoordinate);

			ObjParagraph.Range.InsertParagraphAfter();
			ObjParagraph.Range.InsertAfter("Пикетаж: " + currentNsp.Picket);
			ObjParagraph.Range.InsertParagraphAfter();
			ObjParagraph.Range.InsertAfter("Емкость: " + currentNsp.Capacity);
			ObjParagraph.Range.InsertParagraphAfter();
			ObjParagraph.Range.InsertAfter("Путь: " + currentNsp.Track);

			ObjParagraph.Range.InsertParagraphAfter();
			ObjParagraph.Range.InsertAfter("Расстановка: " + comboBoxArrangement.Text);

			ObjParagraph.Range.InsertParagraphAfter();
			ObjParagraph.Range.InsertAfter("Время движения до выхода на линию: " + textBoxTimeToExitOnLine.Text);

		}

		private void comboBoxStage_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{

		}
		/*
				private void ComboBoxDepot_SelectionChanged(object sender, SelectionChangedEventArgs e)
				{

				}

				private void ComboBoxBelongingDepot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
				{

				}
				DataTable dataTable = new DataTable("Точки ночного отстоя");
						OleDbConnection connection = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; Data Source = 2009 — new.mdb");
						OleDbCommand command = new OleDbCommand("SELECT * FROM" + "[" + dataTable + "]");
						command.Connection = connection;
						//OleDbDataAdapter adapter = new OleDbDataAdapter(command);
						//OleDbCommandBuilder builder = new OleDbCommandBuilder(adapter);
						//adapter.InsertCommand = builder.GetInsertCommand();
						//adapter.UpdateCommand = builder.GetUpdateCommand();
						connection.Open();
						OleDbDataAdapter adapter = new OleDbDataAdapter(command);
						OleDbCommandBuilder builder = new OleDbCommandBuilder(adapter);
						//adapter.InsertCommand = builder.GetInsertCommand();
						OleDbCommandBuilder oleDb = new OleDbCommandBuilder(adapter);
						adapter.InsertCommand = oleDb.GetInsertCommand();
						adapter.UpdateCommand = builder.GetUpdateCommand();
						adapter.Fill(dataTable);

						DataRow newRow = dataTable.NewRow();
						int mx = 1;
						foreach (DataRow dr in dataTable.Rows)
							mx = Math.Max(mx, int.Parse(dr.ItemArray[0].ToString()));
						newRow[0] = mx + 1;
						for (int i = 1; i < dataTable.Columns.Count; i++)
							if (Values[i - 1] == null)
								newRow[i] = DBNull.Value;
							else
								newRow[i] = Values[i - 1];
						dataTable.Rows.Add(newRow);
						//OleDbCommandBuilder oleDb = new OleDbCommandBuilder(adapter);
						//adapter.InsertCommand = oleDb.GetInsertCommand();
						adapter.Update(dataTable);

						//RefreshList(dataTable);
						connection.Close();

		OleDbConnection mySqlCon = new OleDbConnection("Provider=Microsoft.ACE.OLEDB.12.0; Data Source = 2009 — new.mdb");
				string sqlExpression = String.Format("INSERT INTO [Точки_ночного_отстоя] ([Код], [Название], [Станция], [Перегон], [Координата_утро], [Координата_вечер], [Пикетаж], [Путь], " +
													"[Предыдущая_точка_нечетная], [Предыдущая_точка_четная], [Резервная], [Депо], [Принадлежность_депо], [Емкость_точки], [Принадлежность_расстановке], " +
													"[Время_движения_до_выхода_на_линию], [Линия], [Начало_выпуска_из точки_нечетное], [Конец_выпуска_из точки_нечетное], [Выйти_до_первого_пассажирского_нечетное], " +
													"[Начало_выпуска_из точки_четное], [Конец_выпуска_из точки_четное], [Выйти_до_первого_пассажирского_четное], [Направление_выхода_нечетное], [Направление_выхода_четное], " +
													"[Зайти_после_последнего_пассажирского_нечетное], [Зайти_после_последнего_пассажирского_четное]) VALUES ({0},'{1}',{2},{3},{4},{5},{6},{7},'{8}','{9}',{10},'{11}','{12}'" +
													",{13},{14},'{15}',{16},'{17}','{18}',{19},'{20}','{21}',{22},{23},{24},{25},{26});", mx, textBoxRename.Text, selectedStationCode.ToString(), 
													selectedStageCode.ToString(), textBoxCoordinateMorning.Text, textBoxCoordinateEvening.Text, textBoxPicket.Text, textBoxTrack.Text, null, null, 
													checkBoxReserve.IsChecked.ToString(), null, null, textBoxCapacity.Text, comboBoxArrangement.Text, textBoxTimeToExitOnLine.Text.ToString(), 
													selectedLineCode.ToString(), textBoxStartUneven.Text.ToString(), textBoxEndUneven.Text.ToString(), checkBoxGoBeforeFirstTrainUneven.IsChecked.ToString(), 
													textBoxStartEven.Text.ToString(), textBoxEndEven.Text.ToString(), checkBoxGoBeforeFirstTrainEven.IsChecked.ToString(), textBoxExitDirectionUneven.Text, 
													textBoxExitDirectionEven.Text, checkBoxGoAfterLastTrainUneven.IsChecked.ToString(), checkBoxGoAfterLastTrainEven.IsChecked.ToString());
				*/
	}
}
