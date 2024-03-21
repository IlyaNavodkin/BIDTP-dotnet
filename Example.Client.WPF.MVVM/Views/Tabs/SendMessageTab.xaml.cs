using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BIDTP.Dotnet.Iteraction.Request;
using Example.Client.WPF.MVVM.ViewModels.Tabs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Example.Client.WPF.MVVM.Views.Tabs;

public partial class SendMessageTab : UserControl
{
    public SendMessageTab()
    {
        InitializeComponent();
        DataContext = new SendMessageTabViewModel();
    }
}