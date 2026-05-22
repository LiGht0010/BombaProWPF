using BombaProMaxWPF.ViewModels;
using System.Windows.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Produits;

public partial class ProduitsSection : UserControl
{
    public ProduitsSectionViewModel ViewModel { get; } = new();

    public ProduitsSection()
    {
        InitializeComponent();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.EnsureLoadedAsync();
    }
}
