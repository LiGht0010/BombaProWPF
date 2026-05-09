using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using BombaProMaxWPF.Localization;
using BombaProMaxWPF.Models;
using BombaProMaxWPF.Theme;
using BombaProMaxWPF.ViewModels;
using BombaProMaxWPF.Views;
using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml — neumorphic shell hosting a
    /// collapsible sidebar and a <see cref="System.Windows.Controls.Frame"/>
    /// for the active page.
    /// </summary>
    public partial class MainWindow : FluentWindow
    {
        private const double SidebarExpandedWidth = 260;
        private const double SidebarCollapsedWidth = 88;

        private readonly ShellViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new ShellViewModel();
            _viewModel.NavigationRequested += OnNavigationRequested;
            _viewModel.LogoutRequested += OnLogoutRequested;
            _viewModel.StartDayRequested += OnStartDayRequested;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;

            DataContext = _viewModel;

            ThemePalette.Apply(dark: false);
            ApplicationThemeManager.Apply(ApplicationTheme.Light);
            this.SetResourceReference(BackgroundProperty, "NeuBackgroundBrush");
            this.SetResourceReference(ForegroundProperty, "NeuTextPrimaryBrush");
            ThemeToggle.IsChecked = false;

            LanguageManager.Instance.SetLanguage("fr");

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SelectedItem is null && _viewModel.Items.Count > 0)
            {
                _viewModel.SelectedItem = _viewModel.Items[0];
            }
            SyncRadioCheckedState();
            UpdateCollapsedVisuals();
        }

        private void OnNavigationRequested(NavItem item)
        {
            ContentFrame.Content = item.Key switch
            {
                "dashboard" => new Views.DashboardPages.DashboardView(),
                _ => null,
            };
        }

        private void OnLogoutRequested()
        {
            App.CurrentUser = null;
            App.user = null;

            var login = new LoginWindow();
            Application.Current.MainWindow = login;
            login.Show();
            Close();
        }

        private void OnStartDayRequested()
        {
            // TODO: hook the actual "start day" workflow once the service is ported.
            System.Windows.MessageBox.Show(this, "Démarrer la journée — à implémenter.", "Info",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ShellViewModel.IsPaneOpen))
            {
                UpdateCollapsedVisuals();
            }
            else if (e.PropertyName == nameof(ShellViewModel.SelectedItem))
            {
                SyncRadioCheckedState();
            }
        }

        private void NavItem_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton { Tag: NavItem item })
            {
                _viewModel.SelectedItem = item;
            }
        }

        private void SyncRadioCheckedState()
        {
            foreach (var rb in FindVisualChildren<RadioButton>(this))
            {
                if (rb.GroupName == "ShellNav" && rb.Tag is NavItem item)
                {
                    bool shouldBeChecked = ReferenceEquals(item, _viewModel.SelectedItem);
                    if (rb.IsChecked != shouldBeChecked)
                    {
                        rb.IsChecked = shouldBeChecked;
                    }
                }
            }
        }

        private void UpdateCollapsedVisuals()
        {
            bool open = _viewModel.IsPaneOpen;
            SidebarColumn.Width = new GridLength(open ? SidebarExpandedWidth : SidebarCollapsedWidth);

            // Header brand stack (logo + title) — only the title block lives there
            // alongside the logo; we hide the whole stack when collapsed.
            SidebarBrand.Visibility = open ? Visibility.Visible : Visibility.Collapsed;
            UserText.Visibility = open ? Visibility.Visible : Visibility.Collapsed;
            StartDayLabel.Visibility = open ? Visibility.Visible : Visibility.Collapsed;
            LogoutButton.Visibility = open ? Visibility.Visible : Visibility.Collapsed;

            // Pane toggle button — in expanded mode it sits in col 1 (right);
            // when collapsed we span both header columns and center it.
            if (open)
            {
                Grid.SetColumn(PaneToggleButton, 1);
                Grid.SetColumnSpan(PaneToggleButton, 1);
                PaneToggleButton.HorizontalAlignment = HorizontalAlignment.Right;
            }
            else
            {
                Grid.SetColumn(PaneToggleButton, 0);
                Grid.SetColumnSpan(PaneToggleButton, 2);
                PaneToggleButton.HorizontalAlignment = HorizontalAlignment.Center;
            }

            // CTA play icon: drop the right-margin gap reserved for the label.
            StartDayIcon.Margin = open ? new Thickness(0, 0, 8, 0) : new Thickness(0);

            // CTA button: zero the horizontal style padding (16,10) when collapsed
            // so the icon centers in the narrow column instead of being trimmed.
            StartDayButton.Padding = open ? new Thickness(16, 10, 16, 10) : new Thickness(0, 10, 0, 10);

            // Footer pill: zero horizontal padding when collapsed so the avatar fits.
            UserPill.Padding = open ? new Thickness(10, 8, 10, 8) : new Thickness(0, 8, 0, 8);

            // Footer avatar: span the entire pill grid when collapsed so the
            // 24px avatar (shrunk from 32 to fit the narrow column) actually centers.
            if (open)
            {
                Grid.SetColumnSpan(UserAvatar, 1);
                UserAvatar.Width = 32;
                UserAvatar.Height = 32;
                UserAvatar.Margin = new Thickness(0, 0, 10, 0);
                UserAvatar.HorizontalAlignment = HorizontalAlignment.Left;
            }
            else
            {
                Grid.SetColumnSpan(UserAvatar, 3);
                UserAvatar.Width = 24;
                UserAvatar.Height = 24;
                UserAvatar.Margin = new Thickness(0);
                UserAvatar.HorizontalAlignment = HorizontalAlignment.Center;
            }

            // Nav rows: hide labels, drop the icon's right-margin, center the icon.
            foreach (var tb in FindVisualChildren<System.Windows.Controls.TextBlock>(this))
            {
                if (tb.Name == "NavLabel")
                {
                    tb.Visibility = open ? Visibility.Visible : Visibility.Collapsed;
                }
            }

            foreach (var icon in FindVisualChildren<Wpf.Ui.Controls.SymbolIcon>(this))
            {
                if (icon.Name == "NavIcon")
                {
                    icon.Margin = open ? new Thickness(0, 0, 12, 0) : new Thickness(0);
                    icon.HorizontalAlignment = open ? HorizontalAlignment.Left : HorizontalAlignment.Center;
                }
            }

            foreach (var rb in FindVisualChildren<RadioButton>(this))
            {
                if (rb.GroupName == "ShellNav")
                {
                    rb.HorizontalContentAlignment = open ? HorizontalAlignment.Stretch : HorizontalAlignment.Center;
                    // Strip the 14px left/right padding from NeuNavItemStyle when
                    // collapsed so the icon isn't clipped by the indicator+padding.
                    rb.Padding = open ? new Thickness(14, 10, 14, 10) : new Thickness(0, 10, 0, 10);
                }
            }
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
        }

        private void LanguageComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageComboBox.SelectedItem is not ComboBoxItem { Tag: string code })
            {
                return;
            }

            LanguageManager.Instance.SetLanguage(code);
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

        private static System.Collections.Generic.IEnumerable<T> FindVisualChildren<T>(DependencyObject root)
            where T : DependencyObject
        {
            if (root is null) yield break;

            int count = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(root, i);
                if (child is T t)
                {
                    yield return t;
                }
                foreach (var sub in FindVisualChildren<T>(child))
                {
                    yield return sub;
                }
            }
        }
    }
}
