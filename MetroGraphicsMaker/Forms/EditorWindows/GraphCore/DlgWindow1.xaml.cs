using System.Windows;
using System.Windows.Controls;

namespace WpfApplication1.Forms.EditorWindows.GraphCore
{
    /// <summary>
    /// Interaction logic for DlgWindow.xaml
    /// </summary>
    public partial class DlgWindow1 : Window
    {
        public DlgWindow1()
        {
            InitializeComponent();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = sender as MenuItem;
            if (menuItem == null)
                return;

            if (menuItem.Header == null)
                return;
            
            MessageBox.Show(menuItem.Header.ToString());
            
            Close();
        } 
    }
}
