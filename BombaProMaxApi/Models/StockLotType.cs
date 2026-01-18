namespace BombaProMaxApi.Models;

/// <summary>
/// Defines the type/source of a StockLot.
/// Determines how the stock entered the system.
/// </summary>
public enum StockLotType
{
    /// <summary>
    /// Initial inventory at system setup.
    /// Created during onboarding when reservoir has existing fuel.
    /// AchatID is null for this type.
    /// </summary>
    OpeningBalance = 0,

    /// <summary>
    /// Stock from a normal purchase (Achat).
    /// AchatID is required for this type.
    /// </summary>
    Purchase = 1,

    /// <summary>
    /// Future: Inventory corrections, transfers between reservoirs.
    /// Reserved for future functionality.
    /// </summary>
    Adjustment = 2
}
