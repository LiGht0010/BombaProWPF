using FourniPro.ViewModels;
using System;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Produits;

public partial class NouveauProduitDialog : FluentWindow
{
    public NouveauProduitViewModel ViewModel { get; }

    public NouveauProduitDialog()
    {
        InitializeComponent();
        ViewModel   = new NouveauProduitViewModel();
        DataContext = ViewModel;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => Close();

    protected override void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        // Close the dialog automatically once SaveCommand finishes successfully.
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
