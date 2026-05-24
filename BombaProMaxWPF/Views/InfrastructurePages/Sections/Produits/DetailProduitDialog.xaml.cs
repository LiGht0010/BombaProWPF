using BombaProMaxWPF.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace BombaProMaxWPF.Views.InfrastructurePages.Sections.Produits;

public partial class DetailProduitDialog : FluentWindow
{
    /// <summary>
    /// Set to true when the user clicks "Modifier" — the caller (ProduitsSectionViewModel)
    /// will open the EditProduitDialog immediately after this dialog closes.
    /// </summary>
    public bool ShouldEdit { get; private set; }

    public DetailProduitDialog(ProduitDto produit)
    {
        InitializeComponent();
        DataContext = produit;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        ShouldEdit = true;
        Close();
    }
}
