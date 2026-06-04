using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Achats;

public partial class AchatsSection : UserControl
{
    public AchatsSectionViewModel ViewModel { get; } = new();

    public AchatsSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
