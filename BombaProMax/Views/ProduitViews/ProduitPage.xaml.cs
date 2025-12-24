using BombaProMax.ViewModels;

namespace BombaProMax.Views.ProduitViews;

public partial class ProduitPage : ContentPage
{
    private readonly ProduitViewModel _viewModel;

    public ProduitPage(ProduitViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadProduitsCommand.Execute(null);
    }
}