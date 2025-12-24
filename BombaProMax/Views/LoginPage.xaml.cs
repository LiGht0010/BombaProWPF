using BombaProMax.ViewModels;
using BombaProMax.Views;

namespace BombaProMax.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginPageViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}