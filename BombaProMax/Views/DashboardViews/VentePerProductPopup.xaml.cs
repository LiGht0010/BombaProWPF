using BombaProMax.Models.Dashboard;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.DashboardViews;

public partial class VentePerProductPopup : Popup
{
    public VentePerProductPopup(ProductCardModel product, List<VenteAnalyticsRowDto> ventes)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        BindingContext = product;
        VentesCollectionView.ItemsSource = ventes;

        // Calculate and display totals
        var totalQuantite = ventes.Sum(v => v.Quantite);
        var totalPrix = ventes.Sum(v => v.PrixTotal);

        TotalQuantiteLabel.Text = totalQuantite.ToString("N0");
        TotalPrixLabel.Text = $"{totalPrix:N2} DH";
        CountLabel.Text = $"{ventes.Count} vente(s) pour ce produit";
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        Close();
    }
}
