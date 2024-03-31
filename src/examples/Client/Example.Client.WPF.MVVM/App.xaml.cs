using System.Windows;
using BIDTP.Dotnet.Core.Iteraction;
using BIDTP.Dotnet.Core.Iteraction.Options;
using Example.Client.WPF.MVVM.Views;

namespace Example.Client.WPF.MVVM
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        public static BIDTP.Dotnet.Core.Iteraction.Client? Client;
        
        protected override void OnStartup(StartupEventArgs e)
        {
            var options = new ClientOptions("*","testpipe", 
                1024, 9000, 
                1000, 5000);
            
            Client = new BIDTP.Dotnet.Core.Iteraction.Client(options);
            
            var view = new MainWindow();

            view.ShowDialog();
        }
    }
}