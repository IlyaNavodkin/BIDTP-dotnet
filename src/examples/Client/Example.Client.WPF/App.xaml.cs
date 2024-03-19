using System.Windows;
using BIDTP.Dotnet;
using BIDTP.Dotnet.Client;
using Example.Client.WPF.Views;


namespace Example.Client.WPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static BIDTP.Dotnet.Client.Client? Client;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            var options = new ClientOptions("testpipe", 
                1024, 9000, 
                1000, 5000);
            
            Client = new BIDTP.Dotnet.Client.Client(options);
            
            var view = new MainWindow();

            view.ShowDialog();
        }
    }
}