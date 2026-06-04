using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Ventes;

public partial class VentesSection : UserControl
{
    public VentesSectionViewModel ViewModel { get; } = new();

    public VentesSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
