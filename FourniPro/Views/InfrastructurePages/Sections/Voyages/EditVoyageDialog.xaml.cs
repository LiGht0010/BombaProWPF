using FourniPro.Models;
using FourniPro.ViewModels;
using System;
using System.Windows;
using System.Windows.Media;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Voyages;

public partial class EditVoyageDialog : FluentWindow
{
    public EditVoyageViewModel ViewModel { get; }

    public EditVoyageDialog(VoyageCardItem voyage)
    {
        InitializeComponent();
        ViewModel   = new EditVoyageViewModel(voyage);
        DataContext = ViewModel;
    }

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        _ = ViewModel.LoadLookupsAsync();

        // Auto-close when voyage header is saved
        ViewModel.SaveCommand.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.SaveCommand.IsRunning)
                && !ViewModel.SaveCommand.IsRunning
                && ViewModel.Saved)
            {
                Dispatcher.Invoke(Close);
            }
        };

        // Route "edit transaction" requests to the correct tab
        ViewModel.EditTransactionRequested += tab => Dispatcher.Invoke(() => SwitchToTab(tab));
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    // ── Tab switching ─────────────────────────────────────────────────────────

    private void OnTabClick(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button btn
            || btn.Tag is not string tagStr
            || !int.TryParse(tagStr, out int tab)) return;

        SwitchToTab(tab);
        ViewModel.ActiveTab = tab;
    }

    private void SwitchToTab(int tab)
    {
        PanelVente.Visibility  = tab == 0 ? Visibility.Visible : Visibility.Collapsed;
        PanelCredit.Visibility = tab == 1 ? Visibility.Visible : Visibility.Collapsed;
        PanelAchat.Visibility  = tab == 2 ? Visibility.Visible : Visibility.Collapsed;
        PanelFrais.Visibility  = tab == 3 ? Visibility.Visible : Visibility.Collapsed;

        var accent = (Brush)FindResource("NeuAccentBrush");
        var white  = Brushes.White;
        var secFg  = (Brush)FindResource("NeuTextSecondaryBrush");
        var transp = Brushes.Transparent;

        SetTabStyle(TabBtn0, tab == 0, accent, white, secFg, transp);
        SetTabStyle(TabBtn1, tab == 1, accent, white, secFg, transp);
        SetTabStyle(TabBtn2, tab == 2, accent, white, secFg, transp);
        SetTabStyle(TabBtn3, tab == 3, accent, white, secFg, transp);
    }

    private static void SetTabStyle(System.Windows.Controls.Button btn, bool active,
        Brush accent, Brush whiteBrush, Brush secondary, Brush transparent)
    {
        btn.Background = active ? accent     : transparent;
        btn.Foreground = active ? whiteBrush : secondary;
    }
}
