using BombaProMaxWPF.Localization;
using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using BombaProMaxWPF.Theme;
using System.Windows;
using Wpf.Ui.Appearance;

namespace BombaProMaxWPF
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static UserDto? user;
        public static UserDto? CurrentUser { get; set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            ApiConfig.Initialize();

            // Load persisted theme + language and apply once before any window
            // is shown so the very first frame matches the user's last choice.
            var settings = AppSettingsService.Instance;
            settings.Load();

            ThemePalette.Apply(dark: settings.IsDarkTheme);
            ApplicationThemeManager.Apply(settings.IsDarkTheme
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light);
            LanguageManager.Instance.SetLanguage(settings.LanguageCode);
        }
    }
}
