using FourniPro.ViewModels;
using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.PaiementsCredit;

public partial class NouveauPaiementCreditDialog : FluentWindow
{
    public NouveauPaiementCreditViewModel ViewModel { get; }

    public NouveauPaiementCreditDialog()
    {
        InitializeComponent();
        ViewModel   = new NouveauPaiementCreditViewModel();
        DataContext = ViewModel;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => Close();

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        _ = ViewModel.LoadLookupsAsync();
        ViewModel.SaveCommand.PropertyChanged += (_, args) =>
        {
            if (args.PropertyName == nameof(ViewModel.SaveCommand.IsRunning)
                && !ViewModel.SaveCommand.IsRunning
                && ViewModel.Saved)
                Dispatcher.Invoke(Close);
        };
    }
}
