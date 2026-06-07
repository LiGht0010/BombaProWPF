using System.Windows;
using FourniPro.Automation;
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
            AutomationSettings.Instance.Load();

            ThemePalette.Apply(dark: settings.IsDarkTheme);
            ApplicationThemeManager.Apply(settings.IsDarkTheme
                ? ApplicationTheme.Dark
                : ApplicationTheme.Light);

            ApiConfig.Initialize();
            LanguageManager.Instance.SetLanguage(settings.LanguageCode);

                    DispatcherUnhandledException += (_, args) =>
                    {
                        args.Handled = true;
                        System.Windows.MessageBox.Show(
                            $"Unhandled exception:\n\n{args.Exception.GetType().Name}: {args.Exception.Message}\n\n{args.Exception.StackTrace}",
                            "Fatal Error",
                            System.Windows.MessageBoxButton.OK,
                            System.Windows.MessageBoxImage.Error);
                    };

                    var login = new LoginWindow();
                    MainWindow = login;
                    login.Show();
                }
            }
}

