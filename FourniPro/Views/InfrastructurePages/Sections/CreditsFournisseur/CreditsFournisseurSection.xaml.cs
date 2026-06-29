using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.CreditsFournisseur;

public partial class CreditsFournisseurSection : UserControl
{
    public CreditsFournisseurSectionViewModel ViewModel { get; } = new();

    public CreditsFournisseurSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
