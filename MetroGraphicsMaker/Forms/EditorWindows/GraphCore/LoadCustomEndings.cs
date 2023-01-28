using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WpfApplication1.Forms.EditorWindows.CreateCustomEndingWindows
{
    class LoaderCustomEndings : BackgroundWorker
    {
        protected DelegateEjectionCustomEnding ProcedureEjectionCustomEnding;

        public LoaderCustomEndings(DelegateEjectionCustomEnding EjectionCustomEnding)
        {
            ProcedureEjectionCustomEnding = EjectionCustomEnding;

            var pathToMagicDirectory = //@"C:\Nodes\linii_graph\linii_graph\bin\Debug";
                //Environment.CurrentDirectory;
            AppDomain.CurrentDomain.BaseDirectory;
            // MessageBox mbs = 
            // new MessageBox(   (AppDomain.CurrentDomain.BaseDirector);
            var extentionalPattern = "*.jpg";

            WorkerReportsProgress = true;
            DoWork += bwLoader_DoWork;
            ProgressChanged += bwLoader_ProgressChanged;
            RunWorkerAsync(Directory.GetFiles(pathToMagicDirectory, extentionalPattern));
        }

        void bwLoader_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var tmpImage = e.UserState as BitmapImage;
            if (tmpImage == null)
                return;

            var buttonLength = 100;
            var toolTipLength = 200;

            var tmpButton = new Button
            {
                Tag = Path.ChangeExtension(tmpImage.UriSource.ToString(), ".xml"),
                Name = "btn1",
                Height = buttonLength,
                Width = buttonLength,
                Content = new Image { Source = tmpImage },
                ToolTip = new ToolTip
                {
                    Background = Brushes.LightGreen,
                    Width = toolTipLength,
                    Height = toolTipLength,
                    Content = new Image { Source = tmpImage }
                }
            };

            ProcedureEjectionCustomEnding(tmpButton);
        }

        private void bwLoader_DoWork(object sender, DoWorkEventArgs e)
        {
            var pathes = e.Argument as string[];
            if (pathes == null)
                return;

            foreach (var path in pathes)
            {
                //Thread.Sleep(2000);                

                var src = new BitmapImage { CacheOption = BitmapCacheOption.OnLoad };
                src.BeginInit();
                src.UriSource = new Uri(path, UriKind.Relative);
                src.EndInit();
                src.Freeze();

                ReportProgress(0, src);
            }
        }

        public delegate void DelegateEjectionCustomEnding(Button CustomEnding);
    }
}
