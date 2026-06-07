using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views;

/// <summary>
/// Paramètres page — application settings including automation toggles.
/// </summary>
public partial class ParametresView : UserControl
{
    public ParametresViewModel ViewModel { get; } = new();

    public ParametresView()
    {
        InitializeComponent();
        DataContext = ViewModel;
    }
}
