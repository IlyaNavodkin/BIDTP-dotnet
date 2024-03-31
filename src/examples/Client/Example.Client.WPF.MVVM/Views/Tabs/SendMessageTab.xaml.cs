using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using Example.Client.WPF.MVVM.ViewModels.Tabs;

namespace Example.Client.WPF.MVVM.Views.Tabs;

public partial class SendMessageTab : UserControl
{
    public SendMessageTab()
    {
        InitializeComponent();
        DataContext = new SendMessageTabViewModel();
    }
}