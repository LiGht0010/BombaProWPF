using BombaProMax.ViewModels;

namespace BombaProMax.Views.SettingsViews;

public partial class StationInfoPage : ContentPage
{
    private readonly StationInfoViewModel _viewModel;

    public StationInfoPage(StationInfoViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadStationInfoAsync();
        UpdateLogoDisplay();
    }

    private void UpdateLogoDisplay()
    {
        if (_viewModel.StationInfo?.LogoBase64 != null)
        {
            try
            {
                var logoBytes = Convert.FromBase64String(_viewModel.StationInfo.LogoBase64);
                LogoImage.Source = ImageSource.FromStream(() => new MemoryStream(logoBytes));
                LogoImage.IsVisible = true;
                LogoPlaceholder.IsVisible = false;
            }
            catch
            {
                LogoImage.IsVisible = false;
                LogoPlaceholder.IsVisible = true;
            }
        }
        else
        {
            LogoImage.IsVisible = false;
            LogoPlaceholder.IsVisible = true;
        }
    }
}
