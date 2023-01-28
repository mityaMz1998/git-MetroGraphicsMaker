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

namespace WpfApplication1.Forms
{
   public class ResultForExtraTime
    {
        public Int32 NumberOfRepair { get; set; }
        public Int32 Additionaltime { get; set; }
        public bool CheckAdditionalTime { get; set; }
        public string Chains { get; set; }
        public Int32 AmountOfRepair { get; set; }
    }
    /// <summary>
    /// Interaction logic for ExtraTimewindow.xaml
    /// </summary>
    /// 
    public partial class ExtraTimewindow : Window
    {
        protected List<ResultForExtraTime> AdditionalResults;
        public int index=0;
        public int total = 0;
        public ExtraTimewindow()
        {
            InitializeComponent();
            AdditionalResults = new List<ResultForExtraTime>();
            while (index< MovementSchedule.preparedChains.Count)
            {
                AdditionalResults.Add(new ResultForExtraTime()
                {
                    NumberOfRepair = index+1,
                    Additionaltime = MovementSchedule.preparedChains[index].AdditionalTime,
                    Chains = MovementSchedule.preparedChains[index].ToString(),
                    AmountOfRepair = MovementSchedule.preparedChains[index].NumberOfRepairs
                });
                total = total + MovementSchedule.preparedChains[index].NumberOfRepairs;
                ++index;
            }
            AdditionalDataGrid.ItemsSource = AdditionalResults;
            totalBox.Text = total.ToString();
            Show();
        }

        private void OKBtn_OnClick(Object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
