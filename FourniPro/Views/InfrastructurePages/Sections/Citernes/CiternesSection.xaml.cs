using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Citernes;

public partial class CiternesSection : UserControl
{
    public CiternesSectionViewModel ViewModel { get; } = new();

    public CiternesSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
