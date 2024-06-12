using System;
using System.Diagnostics;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Example.Client.WPF.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private CancellationTokenSource _cancelTokenSource;

        /// <summary>
        ///  Iteraction window
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}