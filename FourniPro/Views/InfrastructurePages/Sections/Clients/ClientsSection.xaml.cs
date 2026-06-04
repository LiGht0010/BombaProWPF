using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Clients;

public partial class ClientsSection : UserControl
{
    public ClientsSectionViewModel ViewModel { get; } = new();

    public ClientsSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
