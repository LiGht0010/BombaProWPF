using BombaProMax.Models.Dashboard;
using CommunityToolkit.Maui.Views;

namespace BombaProMax.Views.DashboardViews;

public partial class VenteCarburantPerProductPopup : Popup
{
    public VenteCarburantPerProductPopup(ProductCardModel product, List<VenteCarburantAnalyticsRowDto> ventes)
    {
        InitializeComponent();
        CanBeDismissedByTappingOutsideOfPopup = false;

        BindingContext = product;

        // Group data by Reservoir
        var groupedByReservoir = ventes
            .GroupBy(v => v.ReservoirId)
            .Select(g => new VenteCarburantParReservoirDto
            {
                ReservoirId = g.Key,
                ReservoirNumero = g.First().ReservoirNumero ?? $"Reservoir {g.Key}",
                TotalQuantiteElectronique = g.Sum(x => x.QuantiteElectronique),
                TotalQuantiteMecanique = g.Sum(x => x.QuantiteMecanique),
                TotalDifference = g.Sum(x => x.DifferenceQuantite),
                TotalPrix = g.Sum(x => x.PrixTotalElectronique),
                NombrePeriodes = g.Count()
            })
            .OrderBy(r => r.ReservoirNumero)
            .ToList();

        VentesCollectionView.ItemsSource = groupedByReservoir;

        // Calculate and display grand totals
        var totalQuantiteElec = groupedByReservoir.Sum(r => r.TotalQuantiteElectronique);
        var totalQuantiteMeca = groupedByReservoir.Sum(r => r.TotalQuantiteMecanique);
        var totalDifference = groupedByReservoir.Sum(r => r.TotalDifference);
        var totalPrix = groupedByReservoir.Sum(r => r.TotalPrix);

        TotalQuantiteElecLabel.Text = totalQuantiteElec.ToString("N2");
        TotalQuantiteMecaLabel.Text = totalQuantiteMeca.ToString("N2");
        TotalDifferenceLabel.Text = totalDifference.ToString("N2");
        TotalPrixLabel.Text = $"{totalPrix:N2} DH";
        CountLabel.Text = $"{groupedByReservoir.Count} reservoir(s) - {ventes.Count} periode(s) au total";
    }

    private void OnCloseClicked(object? sender, EventArgs e)
    {
        Close();
    }
}
