using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.PaiementsFournisseur;

public partial class PaiementsFournisseurSection : UserControl
{
    public PaiementsFournisseurSectionViewModel ViewModel { get; } = new();

    public PaiementsFournisseurSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
