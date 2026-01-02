using BombaProMax.ViewModels;

namespace BombaProMax.Views;

public partial class LoginPage : ContentPage
{
    private readonly LoginPageViewModel _viewModel;

    public LoginPage(LoginPageViewModel vm)
    {
        InitializeComponent();
        _viewModel = vm;
        BindingContext = vm;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
    }
}