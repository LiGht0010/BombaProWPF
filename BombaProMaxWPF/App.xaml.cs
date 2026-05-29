using BombaProMaxWPF.Localization;
using BombaProMaxWPF.Models;
using BombaProMaxWPF.Services;
using BombaProMaxWPF.Theme;
using System.Windows;
using Wpf.Ui.Appearance;
using BombaProMaxWPF.Views;

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

            // Load persisted theme + language and apply all tokens before any
            // window is created so the very first frame renders correctly.
            var settings = AppSettingsService.Instance;
            settings.Load();

            ThemePalette.Apply(dark: settings.IsDarkTheme);
            ApplicationThemeManager.Apply(settings.IsDarkTheme
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light);

            ApiConfig.Initialize();
            LanguageManager.Instance.SetLanguage(settings.LanguageCode);

            // Create the first window explicitly — after all resources are set —
            // instead of relying on StartupUri which fires before OnStartup completes.
            new LoginWindow().Show();
        }
    }
}
