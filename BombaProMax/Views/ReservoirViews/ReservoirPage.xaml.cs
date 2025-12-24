using BombaProMax.ViewModels;

namespace BombaProMax.Views.ReservoirViews;

public partial class ReservoirPage : ContentPage
{
    private readonly ReservoirViewModel _viewModel;

    public ReservoirPage(ReservoirViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadReservoirsCommand.Execute(null);
    }
}