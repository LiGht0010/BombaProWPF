using BombaProMax.ViewModels;

namespace BombaProMax.Views.PompeViews;

public partial class PompePage : ContentPage
{
    private readonly PompeViewModel _viewModel;

    public PompePage(PompeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadPompesCommand.Execute(null);
    }
}