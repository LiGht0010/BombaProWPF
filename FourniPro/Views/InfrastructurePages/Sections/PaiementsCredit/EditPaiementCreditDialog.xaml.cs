using FourniPro.Models;
using FourniPro.ViewModels;
using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.PaiementsCredit;

public partial class EditPaiementCreditDialog : FluentWindow
{
    public EditPaiementCreditViewModel ViewModel { get; }

    public EditPaiementCreditDialog(PaiementCreditDto dto)
    {
        InitializeComponent();
        ViewModel   = new EditPaiementCreditViewModel(dto);
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
