using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Camions;

public partial class CamionsSection : UserControl
{
    public CamionsSectionViewModel ViewModel { get; } = new();

    public CamionsSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
