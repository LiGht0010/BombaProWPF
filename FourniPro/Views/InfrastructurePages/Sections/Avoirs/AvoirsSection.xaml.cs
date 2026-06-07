using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Avoirs;

public partial class AvoirsSection : UserControl
{
    public AvoirsSectionViewModel ViewModel { get; } = new();

    public AvoirsSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
