using BombaProMax.ViewModels;

namespace BombaProMax.Views.ChauffeurViews;

public partial class ChauffeurPage : ContentPage
{
    private readonly ChauffeurViewModel _viewModel;

    public ChauffeurPage(ChauffeurViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadChauffeursCommand.Execute(null);
    }
}