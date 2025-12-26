using BombaProMax.ViewModels;

namespace BombaProMax.Views.RapportViews;

public partial class RapportPage : ContentPage
{
    private readonly RapportViewModel _viewModel;

    public RapportPage(RapportViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        // Set the picker to current month (index = month - 1)
        MoisPicker.SelectedIndex = _viewModel.SelectedMois - 1;
        
        await _viewModel.LoadRapportsAsync();
    }

    private void OnMoisPickerSelectedIndexChanged(object sender, EventArgs e)
    {
        if (MoisPicker.SelectedIndex >= 0)
        {
            _viewModel.SelectedMois = MoisPicker.SelectedIndex + 1;
        }
    }

    private void OnDateSpecifiqueSelected(object sender, DateChangedEventArgs e)
    {
        _viewModel.DateSpecifique = e.NewDate;
    }

    private void OnVentesTabClicked(object sender, EventArgs e)
    {
        SetActiveTab(0);
    }

    private void OnDepensesTabClicked(object sender, EventArgs e)
    {
        SetActiveTab(1);
    }

    private void OnStockTabClicked(object sender, EventArgs e)
    {
        SetActiveTab(2);
    }

    private void SetActiveTab(int tabIndex)
    {
        // Reset all tabs
        VentesTabButton.BackgroundColor = Color.FromArgb("#1565C0");
        VentesTabButton.TextColor = Color.FromArgb("#B3E5FC");
        VentesTabButton.FontAttributes = FontAttributes.None;

        DepensesTabButton.BackgroundColor = Color.FromArgb("#1565C0");
        DepensesTabButton.TextColor = Color.FromArgb("#B3E5FC");
        DepensesTabButton.FontAttributes = FontAttributes.None;

        StockTabButton.BackgroundColor = Color.FromArgb("#1565C0");
        StockTabButton.TextColor = Color.FromArgb("#B3E5FC");
        StockTabButton.FontAttributes = FontAttributes.None;

        VentesTab.IsVisible = false;
        DepensesTab.IsVisible = false;
        StockTab.IsVisible = false;

        // Activate selected tab
        switch (tabIndex)
        {
            case 0:
                VentesTabButton.BackgroundColor = Color.FromArgb("#1976D2");
                VentesTabButton.TextColor = Colors.White;
                VentesTabButton.FontAttributes = FontAttributes.Bold;
                VentesTab.IsVisible = true;
                break;
            case 1:
                DepensesTabButton.BackgroundColor = Color.FromArgb("#1976D2");
                DepensesTabButton.TextColor = Colors.White;
                DepensesTabButton.FontAttributes = FontAttributes.Bold;
                DepensesTab.IsVisible = true;
                break;
            case 2:
                StockTabButton.BackgroundColor = Color.FromArgb("#1976D2");
                StockTabButton.TextColor = Colors.White;
                StockTabButton.FontAttributes = FontAttributes.Bold;
                StockTab.IsVisible = true;
                break;
        }
    }
}
