using FourniPro.Models;
using System.Windows;
using Wpf.Ui.Controls;

namespace FourniPro.Views.InfrastructurePages.Sections.Ventes;

public partial class DetailVenteDialog : FluentWindow
{
    /// <summary>
    /// Set to <see langword="true"/> when the user clicks "Modifier".
    /// The caller opens <see cref="EditVenteDialog"/> immediately after this dialog closes.
    /// </summary>
    public bool ShouldEdit { get; private set; }

    public DetailVenteDialog(VenteDto vente)
    {
        InitializeComponent();
        DataContext = vente;
    }

    private void OnCloseClick(object sender, RoutedEventArgs e) => Close();

    private void OnEditClick(object sender, RoutedEventArgs e)
    {
        ShouldEdit = true;
        Close();
    }
}
