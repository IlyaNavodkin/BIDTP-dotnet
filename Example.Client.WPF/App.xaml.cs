using System.Windows;
using Piders.Dotnet.Client;
using TestClient.Views;

namespace TestClient
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static Client? Client;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            var options = new ClientOptions("testpipe", 
                1024, 9000, 
                1000, 5000);
            
            Client = new Client(options);
            
            var view = new MainWindow();

            view.ShowDialog();
        }
    }
}