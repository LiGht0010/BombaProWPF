using BombaProMaxWPF.ViewModels;
using System.Windows.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Pompes;

public partial class PompesSection : UserControl
{
    public PompeSectionViewModel ViewModel { get; } = new();

    public PompesSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}

