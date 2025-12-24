using BombaProMax.Models.Dashboard;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.DashboardViews;

public partial class AchatPerProductPopup : Popup
{
    public AchatPerProductPopup(ProductCardModel product, List<AchatAnalyticsRowDto> achats)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        BindingContext = product;
        AchatsCollectionView.ItemsSource = achats;
        
        // Calculate and display totals
        var totalQuantite = achats.Sum(a => a.Quantite);
        var totalPrix = achats.Sum(a => a.PrixTotal);
        
        TotalQuantiteLabel.Text = totalQuantite.ToString("N0");
        TotalPrixLabel.Text = $"{totalPrix:N2} DH";
        CountLabel.Text = $"{achats.Count} achat(s) pour ce produit";
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        Close();
    }
}