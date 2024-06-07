using System.Threading;
using System.Windows;
using BIDTP.Dotnet;
using BIDTP.Dotnet.Core.Iteraction;
using Example.Client.WPF.Views;


namespace Example.Client.WPF
{
    public partial class App
    {
        public static BidtpClient Client;

        protected override void OnStartup(StartupEventArgs e)
        {

            Client = new BidtpClient();

            Client.SetPipeName("testpipe");

            var view = new MainWindow();

            view.ShowDialog();
        }
    }
}