using BombaProMax.ViewModels;

namespace BombaProMax.Views.CiterneViews;

public partial class CiternePage : ContentPage
{
    private readonly CiterneViewModel _viewModel;

    public CiternePage(CiterneViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCiternesCommand.Execute(null);
    }
}