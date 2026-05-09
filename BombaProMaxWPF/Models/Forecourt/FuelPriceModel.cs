namespace BombaProMaxWPF.Models.Forecourt;

public enum DeltaTone
{
    Up,
    Neutral,
    Down,
}

/// <summary>
/// Demo fuel price row for the Market Pricing card.
/// </summary>
public sealed record FuelPriceModel(string Label, decimal Price, string Delta, DeltaTone DeltaTone);
