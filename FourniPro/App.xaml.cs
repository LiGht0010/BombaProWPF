using System.Windows;
using FourniPro.Localization;
using FourniPro.Models;
using FourniPro.Services;
using FourniPro.Theme;
using FourniPro.Views;
using Wpf.Ui.Appearance;

namespace FourniPro
{
    public partial class App : Application
    {
        public static UserDto? CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var settings = AppSettingsService.Instance;
            settings.Load();

            ThemePalette.Apply(dark: settings.IsDarkTheme);
            ApplicationThemeManager.Apply(settings.IsDarkTheme
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light);

            ApiConfig.Initialize();
            LanguageManager.Instance.SetLanguage(settings.LanguageCode);

            var login = new LoginWindow();
            MainWindow = login;
            login.Show();
        }
    }
}

