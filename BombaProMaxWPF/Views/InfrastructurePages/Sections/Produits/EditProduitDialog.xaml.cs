using BombaProMaxWPF.Models;
using BombaProMaxWPF.ViewModels;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Produits;

public partial class EditProduitDialog : FluentWindow
{
    public EditProduitViewModel ViewModel { get; }

    public EditProduitDialog(ProduitDto produit)
    {
        InitializeComponent();
        ViewModel = new EditProduitViewModel(produit);
        DataContext = ViewModel;
        Loaded += async (_, _) => await ViewModel.LoadCategoriesAsync();
    }

    private void OnCancelClick(object sender, RoutedEventArgs e) => Close();

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);

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
