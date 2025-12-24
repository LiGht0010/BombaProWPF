using BombaProMax.ViewModels;

namespace BombaProMax.Views.CamionViews;

public partial class CamionPage : ContentPage
{
    private readonly CamionViewModel _viewModel;

    public CamionPage(CamionViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadCamionsCommand.Execute(null);
    }
}