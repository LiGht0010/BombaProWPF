using FourniPro.ViewModels;
using System.Windows.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Chauffeurs;

public partial class ChauffeursSection : UserControl
{
    public ChauffeursSectionViewModel ViewModel { get; } = new();

    public ChauffeursSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
