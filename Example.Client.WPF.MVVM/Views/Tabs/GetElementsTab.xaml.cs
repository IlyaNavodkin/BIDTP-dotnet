using System.Windows.Controls;
using Example.Client.WPF.MVVM.ViewModels.Tabs;

namespace Example.Client.WPF.MVVM.Views.Tabs;

/// <summary>
///  Interaction logic for GetElementsTab.xaml
/// </summary>
public partial class GetElementsTab : UserControl
{
    /// <summary>
    ///  Initialize a new instance of the <see cref="GetElementsTab"/> class.
    /// </summary>
    public GetElementsTab()
    {
        InitializeComponent();
        DataContext = new GetElementsTabViewModel();
    }
}