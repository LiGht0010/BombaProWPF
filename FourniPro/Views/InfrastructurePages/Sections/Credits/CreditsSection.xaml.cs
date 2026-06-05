using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Credits;

public partial class CreditsSection : UserControl
{
    public CreditsSectionViewModel ViewModel { get; } = new();

    public CreditsSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
