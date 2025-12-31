using BombaProMax.ViewModels;

namespace BombaProMax.Views;

public partial class TenantSelectionPage : ContentPage
{
    public TenantSelectionPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        // Refresh the selection state when navigating back to this page
        if (BindingContext is TenantSelectionViewModel viewModel)
        {
            viewModel.RefreshSelection();
        }
    }
}
