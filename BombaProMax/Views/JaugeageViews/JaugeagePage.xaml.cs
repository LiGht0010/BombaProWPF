using BombaProMax.ViewModels;

namespace BombaProMax.Views.JaugeageViews;

public partial class JaugeagePage : ContentPage
{
    private readonly JaugeageViewModel _viewModel;

    public JaugeagePage(JaugeageViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}
