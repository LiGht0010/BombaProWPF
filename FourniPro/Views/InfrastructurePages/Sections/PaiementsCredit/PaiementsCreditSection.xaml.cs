using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.PaiementsCredit;

public partial class PaiementsCreditSection : UserControl
{
    public PaiementsCreditSectionViewModel ViewModel { get; } = new();

    public PaiementsCreditSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
