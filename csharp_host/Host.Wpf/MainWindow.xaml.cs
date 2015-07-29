namespace Host.Wpf
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;

    using CommonServiceLocator.NinjectAdapter.Unofficial;

    using Microsoft.Owin.Hosting;
    using Microsoft.Practices.ServiceLocation;

    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private BackgroundWorker worker;

        public MainWindow()
        {
            this.InitializeComponent();
            this.SetupDependencyContainer();

            this.DataContext = this;

            this.worker = new BackgroundWorker { WorkerReportsProgress = true };
            this.worker.DoWork += this.WorkerDoWork;
            this.worker.ProgressChanged += this.WorkerProgressChanged;
            this.worker.RunWorkerAsync();

            this.PropertyChanged += this.ShowWindow;

            WebApp.Start<Startup>("http://localhost:8085/");
        }

        private void SetupDependencyContainer()
        {
            var kernel = new Ninject.StandardKernel();
            var serviceLocator = new NinjectServiceLocator(kernel);
            ServiceLocator.SetLocatorProvider(() => serviceLocator);
            kernel.Bind<Action<string>>().ToMethod(c => this.SetMessage);
        }

        private void SetMessage(string value)
        {
            this.Message = value;
            this.worker.ReportProgress(0);
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

                this.Message = "Native: " + str;
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
