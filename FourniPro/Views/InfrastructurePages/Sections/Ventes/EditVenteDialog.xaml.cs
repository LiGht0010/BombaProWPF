using FourniPro.Models;
using FourniPro.ViewModels;
using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Ventes;

public partial class EditVenteDialog : FluentWindow
{
    public EditVenteViewModel ViewModel { get; }

    public EditVenteDialog(VenteDto vente)
    {
        InitializeComponent();
        ViewModel   = new EditVenteViewModel(vente);
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
            {
                Dispatcher.Invoke(Close);
            }
        };
    }
}
