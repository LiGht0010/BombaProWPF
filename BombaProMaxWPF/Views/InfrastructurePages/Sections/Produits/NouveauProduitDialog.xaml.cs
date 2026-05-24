using BombaProMaxWPF.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Produits;

public partial class NouveauProduitDialog : FluentWindow
{
    public NouveauProduitViewModel ViewModel { get; }

    public NouveauProduitDialog()
    {
        InitializeComponent();
        ViewModel = new NouveauProduitViewModel();
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.LoadCategoriesAsync();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => Close();

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

        // Poll after SaveCommand completes; close dialog if saved successfully
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
