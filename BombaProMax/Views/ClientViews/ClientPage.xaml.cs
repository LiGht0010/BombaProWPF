using BombaProMax.ViewModels;

namespace BombaProMax.Views.ClientViews;

public partial class ClientPage : ContentPage
{
    private readonly ClientViewModel _viewModel;

    public ClientPage(ClientViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadClientsCommand.Execute(null);
    }
}
