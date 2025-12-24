using BombaProMax.ViewModels;

namespace BombaProMax.Views.FournisseurViews;

public partial class FournisseurPage : ContentPage
{
    private readonly FournisseurViewModel _viewModel;

    public FournisseurPage(FournisseurViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadFournisseursCommand.Execute(null);
    }
}