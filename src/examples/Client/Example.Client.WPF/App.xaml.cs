using System.Threading;
using System.Windows;
using BIDTP.Dotnet;
using BIDTP.Dotnet.Core.Iteraction.Options;
using Example.Client.WPF.Views;


namespace Example.Client.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static BIDTP.Dotnet.Core.Iteraction.Client? Client;
        
        protected override async void OnStartup(StartupEventArgs e)
        {
            var options = new ClientOptions("testpipe", 
                1024, 9000, 
                1000, 5000);
            
            Client = new BIDTP.Dotnet.Core.Iteraction.Client(options);

            await Client.ConnectToServer(new CancellationTokenSource());
            var view = new MainWindow();

            view.ShowDialog();
        }
    }
}