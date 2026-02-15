using BombaProMax.ViewModels;

namespace BombaProMax.Views;

public partial class StockWithdrawalPage : ContentPage
{
    private readonly StockWithdrawalViewModel _viewModel;

    public StockWithdrawalPage(StockWithdrawalViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Check if user is Super Admin
        var currentUser = App.CurrentUser;
        if (currentUser == null || !currentUser.IsSuperAdmin)
        {
            await DisplayAlert("Accès Refusé", 
                "Cette fonctionnalité est réservée aux Super Administrateurs.", 
                "OK");
            await Navigation.PopAsync();
            return;
        }

        await _viewModel.InitializeCommand.ExecuteAsync(null);
    }
}
