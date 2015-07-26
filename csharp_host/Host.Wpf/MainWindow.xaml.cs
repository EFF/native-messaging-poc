namespace Host.Wpf
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private BackgroundWorker worker;

        public MainWindow()
        {
            this.InitializeComponent();
            
            this.DataContext = this;

            this.worker = new BackgroundWorker { WorkerReportsProgress = true };
            this.worker.DoWork += this.WorkerDoWork;
            this.worker.ProgressChanged += this.WorkerProgressChanged;
            this.worker.RunWorkerAsync();

            this.PropertyChanged += this.ShowWindow;
        }

        private void ShowWindow(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            this.Topmost = true;
            this.Activate();
            this.Focus();
            this.Topmost = false;
        }

        public string Message { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void WorkerProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var handler = this.PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs("Message"));
            }
        }

        private void WorkerDoWork(object sender, DoWorkEventArgs e)
        {
            var stdin = Console.OpenStandardInput();

            while (true)
            {
                var buffer = new byte[4];

                // Read the 4 first bytes to get message length
                stdin.Read(buffer, 0, 4);

                if (ReceivedUnexpectedChromeHeaders(buffer))
                {
                    CleanlyExitNativeApplication(stdin);
                }

                var len = BitConverter.ToInt32(buffer, 0);

                // Read the message
                buffer = new byte[len];
                stdin.Read(buffer, 0, len);
                var str = Encoding.UTF8.GetString(buffer);

                this.Message = str;
                this.worker.ReportProgress(0);
            }
        }

        private static void CleanlyExitNativeApplication(Stream stdin)
        {
            stdin.Close();
            Environment.Exit(0);
        }

        private static bool ReceivedUnexpectedChromeHeaders(byte[] buffer)
        {
            return buffer == null || buffer.All(x => x == 0);
        }
    }
}
