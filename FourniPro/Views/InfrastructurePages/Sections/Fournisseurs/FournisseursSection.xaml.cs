using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Fournisseurs;

public partial class FournisseursSection : UserControl
{
    public FournisseursSectionViewModel ViewModel { get; } = new();

    public FournisseursSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
