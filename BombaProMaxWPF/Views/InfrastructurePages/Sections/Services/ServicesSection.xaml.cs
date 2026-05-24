using BombaProMaxWPF.ViewModels;
using System.Windows.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Services;

public partial class ServicesSection : UserControl
{
    public ServicesSectionViewModel ViewModel { get; } = new();

    public ServicesSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}

