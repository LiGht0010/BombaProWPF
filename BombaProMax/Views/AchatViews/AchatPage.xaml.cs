using BombaProMax.ViewModels;

namespace BombaProMax.Views.AchatViews;

public partial class AchatPage : ContentPage
{
    private readonly AchatViewModel _viewModel;

    public AchatPage(AchatViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadAchatsCommand.Execute(null);
    }
}