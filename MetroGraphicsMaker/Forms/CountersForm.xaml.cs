using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Messages;
using MetroGraphicsMaker.Forms;

namespace Forms
{
    /// <summary>
    /// Interaction logic for CountersForm.xaml
    /// </summary>
    public partial class CountersForm : Window
    {

        protected Dictionary<String, Counter> counters;
        /// <summary>
        /// Инкрементирует значение, хранящееся в словаре
        /// </summary>
        /// <param name="key"></param>
        public void IncValue(String key)
        {
            if (counters.ContainsKey(key))
            {
                counters[key].Value++;
                UpdateValue(key);
            }
            else
                Logger.Output(
                    String.Format("counters not contains key \"{0}\"", key),
                    String.Format("{0}.{1}", GetType().FullName, System.Reflection.MethodBase.GetCurrentMethod().Name));
        }

        protected void UpdateValue(String key)
        {
            var tmpName = key.ToLowerInvariant() + "CounterText";
            var tmpText = mainTableLayout.Children.OfType<TextBox>().FirstOrDefault();
            if (tmpText != null)
            {
                tmpText.Text = counters[key].Value.ToString();
                //tmpText.Update();
            }
        }

        public CountersForm()
        {
            counters = new Dictionary<String, Counter>();
            counters["all"] = new Counter("all", "Всего построено вариантов пар");
            counters["bussy"] = new Counter("bussy", "Такое назначение в паросочении уже существует");
            counters["null"] = new Counter("null", "Маршрут не найден (имеет значение null)");
            counters["not_inside"] = new Counter("not_inside", "Кандидат не удовлетворяет временным ограничениям звена");
            counters["to_point"] = new Counter("to_point", "Окно во зможности звена стянуто в точку");
            counters["success"] = new Counter("success", "Успешно построено паросочетаний");

            InitializeComponent();



            var headerLabels = new[] { "Останавливаемся?", "Значение счётчика", "Значение остановки" };
            var columnCount = headerLabels.Length;
//myComment    mainTableLayout.ColumnCount = columnCount;
            for (var column = 0; column < columnCount; column++)
               mainTableLayout.ColumnDefinitions.Add(new ColumnDefinition());
//myComment    mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            // mainTableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

            var row = 0;
            //mainTableLayout.RowCount = counters.Count + 1;
            var rowCount = counters.Count + 1;
                mainTableLayout.RowDefinitions.Add(new RowDefinition());
//myComment     mainTableLayout.RowStyles.Add(new RowStyle());

                for (var column = 0; column < columnCount; column++)
                {
                    var newLabel= new Label
                    {
                        Content=headerLabels[column], 
                        HorizontalAlignment=HorizontalAlignment.Center,
                        VerticalAlignment=VerticalAlignment.Center,
                    };
                    Grid.SetColumn(newLabel, column);
                    Grid.SetRow(newLabel, row);
                    mainTableLayout.Children.Add(newLabel);
                }
                row++;
//                mainTableLayout.Children.Add(
//                    new Label
//                    {
//                        Content = headerLabels[column],
//                        HorizontalContentAlignment = HorizontalAlignment.Center,
//                        VerticalContentAlignment = VerticalAlignment.Center,
////myComment             TextAlign = ContentAlignment.MiddleCenter,
//                        //Dock = DockStyle.Fill, 
//                        //AutoSize = true, 
//                        //Font = new Font( new FontFamily(GenericFontFamilies.Monospace), 12, FontStyle.Bold )
//                    }, Сolumn: column, row: row);
//            row++;
            foreach (var tmpRow in counters.Select(counter => counter.Value.MakeControls()))
            {
                mainTableLayout.RowDefinitions.Add(new RowDefinition());
                ////mainTableLayout.RowStyles.Add(new RowStyle());
                for (var column = 0; column < tmpRow.Length; column++)
                    mainTableLayout.Children.Add(tmpRow[column]);
                row++;
            }

            
//myComment     mainTableLayout.CellBorderStyle = TableLayoutPanelCellBorderStyle.Inset;

            //    (mainTableLayout.Controls[0] as CheckBox).CheckedChanged += CheckBoxCheckedChanged;

            Show();
            mainTableLayout.UpdateLayout();
//myComment    Update();

        }

        // protected Dictionary<String, ulong> countersDictionary;

        protected Stopwatch timer;

        public void StartTimer()
        {
            timer = Stopwatch.StartNew();
        }
        TextBox timerText= new TextBox(); 
        TextBox stopCounterText= new TextBox();
        CheckBox stopByCounterCheckBox= new CheckBox();
        public void StopTimer()
        {
            if (timer.IsRunning)
            {
                timer.Stop();
                timerText.Text = (Convert.ToDouble(timer.ElapsedMilliseconds) / 1000).ToString("N");
//myComment     timerText.Update();
            }
        }

        private void UpdateLabel(String name)
        {
            var tmpName = name.ToLowerInvariant() + "CounterLabel";
            //var tmpText = Controls.Find(tmpName, true).OfType<Label>().FirstOrDefault();
            var tmpText = mainTableLayout.Children.OfType<Label>().FirstOrDefault(n => n.Name == tmpName);
            if (tmpText != null && counters.ContainsKey(name))
            {
                tmpText.Content = counters[name].Label;
//myComment     tmpText.Update();
            }
        }

        private void UpdateTextBlock(String name, UInt64 value)
        {
            var tmpName = name.ToLowerInvariant() + "CounterText";
            //var tmpText = Controls.Find(tmpName, true).OfType<TextBox>().FirstOrDefault();
            var tmpText = mainTableLayout.Children.OfType<TextBox>().FirstOrDefault(n=>n.Name==tmpName);
            if (tmpText != null)
            {
                tmpText.Text = value.ToString();
//myComment     tmpText.Update();
            }

            // TODO: Накапливать информацию со счётчиков привязав её к удачно построенным --> распределение.
        }

        public void SetCounterValue(String key, ulong value)
        {
            if (counters.ContainsKey(key))
            {
                counters[key].Value = value;
                UpdateTextBlock(key, value);
            }
        }

        public UInt64 GetCounterValue(String key)
        {
            return counters[key].Value;
        }


        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            stopCounterText.IsEnabled = (bool)stopByCounterCheckBox.IsChecked;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (stopByCounterCheckBox.IsChecked==true)
                counters["stop"].Value = Convert.ToUInt64(stopCounterText.Text);
        }

        private void allCounterText_TextChanged(object sender, EventArgs e)
        {

        }

        private void CheckBoxCheckedChanged(object sender, EventArgs e)
        {
            var senderCheckBox = sender as CheckBox;
            if (senderCheckBox == null) return;

            //var fullTypeName = senderCheckBox.GetType().FullName;
            //var typeName = fullTypeName.Substring(fullTypeName.LastIndexOf(".", StringComparison.InvariantCulture));

            var typeName = senderCheckBox.GetType().FullName.Split('.').LastOrDefault();
            if (typeName == null)
                typeName = "Checkbox".ToLowerInvariant();

            var localName = senderCheckBox.Name.ToLowerInvariant().Replace(typeName, "");

            //var source = LogicalTreeHelper.FindLogicalNode(senderCheckBox.Parent, localName + "StopText") as TextBox;

            var source = (senderCheckBox.Parent) as Grid;
            // это заглушка
            if (source == null)
                source= new Grid();

            var stopText = source.Children.OfType<TextBox>().SingleOrDefault(t => t.Name == localName + "StopText");



            //= senderCheckBox.Parent.Children.Find(localName + "StopText", true).SingleOrDefault();
           // var stopText = source ;
            if (stopText != null && senderCheckBox.IsChecked == true)
                counters[localName].StopValue = Convert.ToUInt64(stopText.Text);
            else
                counters[localName].StopValue = null;

            //if (senderCheckBox.Checked)
            //    if (stopText == null)
            //        counters[localName].StopValue = null;
            //    else
            //        counters[localName].StopValue = Convert.ToUInt64(stopText.Text);
            //else
            //    counters[localName].StopValue = null;

        }
    }
}
