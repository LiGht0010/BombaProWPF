namespace FourniPro.Models;

/// <summary>
/// Wraps a <see cref="ProduitDto"/> and exposes display-friendly computed
/// properties for the products table view.
/// </summary>
public class ProduitCardItem(ProduitDto dto)
{
    public ProduitDto Dto => dto;
    public int ProduitId => dto.ProduitId;
    public string Numero => dto.NumeroProduit;
    public string Description => string.IsNullOrWhiteSpace(dto.Description) ? "—" : dto.Description;
    public string PrixAchatDisplay => dto.PrixAchat.HasValue ? $"{dto.PrixAchat:N2} DH" : "—";
    public string PrixTtcDisplay => dto.PrixTTC.HasValue ? $"{dto.PrixTTC:N2} DH" : "—";
    public string MargeDisplay => dto.MargePourcentage.HasValue ? $"{dto.MargePourcentage:N1}%" : "—";
    public int Stock => dto.Stock ?? 0;
    public int StockMinimum => dto.StockMinimum ?? 0;

    /// <summary>"Ok" | "Bas" | "Rupture" — drives the badge colour in the view.</summary>
    public string StockState =>
        Stock <= 0 ? "Rupture" :
        StockMinimum > 0 && Stock <= StockMinimum ? "Bas" : "Ok";

    public string StockDisplay => Stock.ToString();
}
