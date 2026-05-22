using System;
using System.Windows;
using System.Windows.Controls;
using BombaProMaxWPF.Localization;
using BombaProMaxWPF.Services;
using BombaProMaxWPF.Theme;
using BombaProMaxWPF.ViewModels;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views;

/// <summary>
/// Interaction logic for LoginWindow.xaml
/// </summary>
public partial class LoginWindow : FluentWindow
{
    private readonly LoginPageViewModel _viewModel;
    private bool _isInitializing;

    public LoginWindow()
    {
        _isInitializing = true;
        InitializeComponent();

        // TODO: replace with DI once a container is wired up.
        var stockLotService = new StockLotService();
        var reservoirService = new ReservoirService();
        var produitService = new ProduitService();
        var onboarding = new OpeningBalanceOnboardingService(stockLotService, reservoirService, produitService);
        _viewModel = new LoginPageViewModel(onboarding);
        _viewModel.LoginSucceeded += OnLoginSucceeded;
        DataContext = _viewModel;

        // Theme + language were applied centrally in App.OnStartup from the
        // persisted AppSettingsService — here we only sync the toolbar controls
        // so they reflect that state. SetResourceReference reasserts our brushes
        // after Wpf.Ui's ApplicationThemeManager replaced the local Background.
        var settings = AppSettingsService.Instance;
        this.SetResourceReference(BackgroundProperty, "NeuBackgroundBrush");
        this.SetResourceReference(ForegroundProperty, "NeuTextPrimaryBrush");
        ThemeToggle.IsChecked = settings.IsDarkTheme;
        if (ThemeIcon is not null)
        {
            ThemeIcon.Symbol = settings.IsDarkTheme
                ? SymbolRegular.WeatherMoon24
                : SymbolRegular.WeatherSunny24;
        }
        SyncLanguageComboFromSettings(settings.LanguageCode);

        _isInitializing = false;
    }

    private void SyncLanguageComboFromSettings(string code)
    {
        foreach (var item in LanguageComboBox.Items)
        {
            if (item is ComboBoxItem cbi && cbi.Tag is string tag &&
                string.Equals(tag, code, StringComparison.OrdinalIgnoreCase))
            {
                LanguageComboBox.SelectedItem = cbi;
                return;
            }
        }
    }

    private void OnLoginSucceeded()
    {
        var main = new MainWindow();
        Application.Current.MainWindow = main;
        main.Show();
        Close();
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        // PasswordBox.Password is non-bindable for security; push manually into the VM.
        if (sender is System.Windows.Controls.PasswordBox pb && DataContext is LoginPageViewModel vm)
        {
            vm.Password = pb.Password;
        }
    }

    private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (LanguageComboBox.SelectedItem is not ComboBoxItem { Tag: string code })
        {
            return;
        }

        LanguageManager.Instance.SetLanguage(code);

        if (_isInitializing) return;
        AppSettingsService.Instance.LanguageCode = code;
        AppSettingsService.Instance.Save();
    }

    private void LanguagePickerHost_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        // Clicks that originate inside the ComboBox itself are handled by its own template.
        if (e.OriginalSource is DependencyObject src && IsDescendantOf(src, LanguageComboBox))
        {
            return;
        }

        LanguageComboBox.IsDropDownOpen = !LanguageComboBox.IsDropDownOpen;
        e.Handled = true;
    }

    private static bool IsDescendantOf(DependencyObject node, DependencyObject ancestor)
    {
        for (var current = node; current is not null; current = System.Windows.Media.VisualTreeHelper.GetParent(current) ?? System.Windows.LogicalTreeHelper.GetParent(current))
        {
            if (ReferenceEquals(current, ancestor)) return true;
        }
        return false;
    }

    private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
    {
        ThemePalette.Apply(dark: true);
        ApplicationThemeManager.Apply(ApplicationTheme.Dark);
        this.SetResourceReference(BackgroundProperty, "NeuBackgroundBrush");
        this.SetResourceReference(ForegroundProperty, "NeuTextPrimaryBrush");
        if (ThemeIcon is not null)
        {
            ThemeIcon.Symbol = SymbolRegular.WeatherMoon24;
        }

        if (_isInitializing) return;
        AppSettingsService.Instance.IsDarkTheme = true;
        AppSettingsService.Instance.Save();
    }

    private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
    {
        ThemePalette.Apply(dark: false);
        ApplicationThemeManager.Apply(ApplicationTheme.Light);
        this.SetResourceReference(BackgroundProperty, "NeuBackgroundBrush");
        this.SetResourceReference(ForegroundProperty, "NeuTextPrimaryBrush");
        if (ThemeIcon is not null)
        {
            ThemeIcon.Symbol = SymbolRegular.WeatherSunny24;
        }

        if (_isInitializing) return;
        AppSettingsService.Instance.IsDarkTheme = false;
        AppSettingsService.Instance.Save();
    }
}
