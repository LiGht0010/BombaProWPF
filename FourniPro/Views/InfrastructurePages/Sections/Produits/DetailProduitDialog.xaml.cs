using FourniPro.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Produits;

public partial class DetailProduitDialog : FluentWindow
{
    /// <summary>
    /// Set to <see langword="true"/> when the user clicks "Modifier".
    /// The caller opens <see cref="EditProduitDialog"/> immediately after this dialog closes.
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
