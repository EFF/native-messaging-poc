namespace Host.Wpf
{
    using System;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Text;

    using CommonServiceLocator.NinjectAdapter.Unofficial;

    using Microsoft.AspNet.SignalR;
    using Microsoft.Owin.Hosting;
    using Microsoft.Practices.ServiceLocation;

    using Newtonsoft.Json.Linq;

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
                var len = ReadMessageLength(stdin, buffer);
                var str = ReadNativeMessage(len, stdin);

                this.Message = "Native: " + str;
                this.worker.ReportProgress(0);
            }
        }

        private static int ReadMessageLength(Stream stdin, byte[] buffer)
        {
            stdin.Read(buffer, 0, 4);

            if (ReceivedUnexpectedChromeHeaders(buffer))
            {
                CleanlyExitNativeApplication(stdin);
            }

            var len = BitConverter.ToInt32(buffer, 0);
            return len;
        }

        private static string ReadNativeMessage(int len, Stream stdin)
        {
            var buffer = new byte[len];
            stdin.Read(buffer, 0, len);
            var str = Encoding.UTF8.GetString(buffer);
            return str;
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

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var connection = GlobalHost.ConnectionManager.GetConnectionContext<Connection>();
            connection.Connection.Broadcast(this.JsonMessage);
        }

        private void Button_Click_1(object sender, System.Windows.RoutedEventArgs e)
        {
            var json = this.JsonMessage;
            var bytes = Encoding.UTF8.GetBytes(json.ToString());

            var stdout = Console.OpenStandardOutput();
            
            stdout.WriteByte((byte)((bytes.Length >> 0) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 8) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 16) & 0xFF));
            stdout.WriteByte((byte)((bytes.Length >> 24) & 0xFF));

            stdout.Write(bytes, 0, bytes.Length);
            stdout.Flush();
        }

        private JObject JsonMessage
        {
            get
            {
                var json = new JObject();
                json["data"] = this.Message;
                return json;
            }
        }
    }
}
