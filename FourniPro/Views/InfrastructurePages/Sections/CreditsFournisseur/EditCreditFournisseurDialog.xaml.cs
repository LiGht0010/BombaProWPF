using FourniPro.Models;
using FourniPro.ViewModels;
using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.CreditsFournisseur;

public partial class EditCreditFournisseurDialog : FluentWindow
{
    public EditCreditFournisseurViewModel ViewModel { get; }

    public EditCreditFournisseurDialog(CreditFournisseurDto dto)
    {
        InitializeComponent();
        ViewModel   = new EditCreditFournisseurViewModel(dto);
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
