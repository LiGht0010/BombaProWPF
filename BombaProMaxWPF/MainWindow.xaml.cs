using System;
using System.Windows;
using System.Windows.Controls;
using BombaProMaxWPF.Localization;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        public MainWindow()
        {
            InitializeComponent();

            // Sync initial theme (App.xaml starts in Light)
            ApplicationThemeManager.Apply(ApplicationTheme.Light);
            ThemeToggle.IsChecked = false;

            // Sync initial language (resx default = French)
            LanguageManager.Instance.SetLanguage("fr");
        }

        private void ThemeToggle_Checked(object sender, RoutedEventArgs e)
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Dark);
            if (ThemeIcon is not null)
            {
                ThemeIcon.Symbol = SymbolRegular.WeatherMoon24;
            }
        }

        private void ThemeToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ApplicationThemeManager.Apply(ApplicationTheme.Light);
            if (ThemeIcon is not null)
            {
                ThemeIcon.Symbol = SymbolRegular.WeatherSunny24;
            }
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is not ComboBoxItem { Tag: string code })
            {
                return;
            }

            LanguageManager.Instance.SetLanguage(code);

            FlowDirection = code == "ar"
                ? System.Windows.FlowDirection.RightToLeft
                : System.Windows.FlowDirection.LeftToRight;
        }
    }
}

