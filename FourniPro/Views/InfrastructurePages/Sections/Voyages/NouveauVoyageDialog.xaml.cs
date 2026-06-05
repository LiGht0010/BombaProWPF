using FourniPro.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Voyages;

public partial class NouveauVoyageDialog : FluentWindow
{
    public NouveauVoyageViewModel ViewModel { get; }

    public NouveauVoyageDialog()
    {
        InitializeComponent();
        ViewModel   = new NouveauVoyageViewModel();
        DataContext = ViewModel;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        _ = ViewModel.LoadLookupsAsync();

        ViewModel.SaveCommand.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.SaveCommand.IsRunning)
                && !ViewModel.SaveCommand.IsRunning
                && ViewModel.Saved)
            {
                Dispatcher.Invoke(Close);
            }
        };
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => Close();
    private void OnCloseClick(object sender, RoutedEventArgs e)  => Close();

    /// <summary>Toggle tab panels and highlight the active tab button.</summary>
    private void OnTabClick(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button btn
            || btn.Tag is not string tagStr
            || !int.TryParse(tagStr, out int tab)) return;

        // Hide all panels, show selected
        PanelVente.Visibility   = tab == 0 ? Visibility.Visible : Visibility.Collapsed;
        PanelCredit.Visibility  = tab == 1 ? Visibility.Visible : Visibility.Collapsed;
        PanelAchat.Visibility   = tab == 2 ? Visibility.Visible : Visibility.Collapsed;
        PanelFrais.Visibility   = tab == 3 ? Visibility.Visible : Visibility.Collapsed;

        // Re-style tab buttons
        var accent = (System.Windows.Media.Brush)FindResource("NeuAccentBrush");
        var white  = System.Windows.Media.Brushes.White;
        var secFg  = (System.Windows.Media.Brush)FindResource("NeuTextSecondaryBrush");
        var transp = System.Windows.Media.Brushes.Transparent;

        SetTabStyle(TabBtn0, tab == 0, accent, white, secFg, transp);
        SetTabStyle(TabBtn1, tab == 1, accent, white, secFg, transp);
        SetTabStyle(TabBtn2, tab == 2, accent, white, secFg, transp);
        SetTabStyle(TabBtn3, tab == 3, accent, white, secFg, transp);

        ViewModel.ActiveTab = tab;
    }

    private static void SetTabStyle(System.Windows.Controls.Button btn, bool active,
        System.Windows.Media.Brush accent, System.Windows.Media.Brush whiteBrush,
        System.Windows.Media.Brush secondary, System.Windows.Media.Brush transparent)
    {
        btn.Background = active ? accent     : transparent;
        btn.Foreground = active ? whiteBrush : secondary;
    }
}
