namespace BombaProMaxWPF.Models.Forecourt;

/// <summary>
/// Demo tank inventory record used by the Forecourt Overview dashboard.
/// </summary>
public sealed record TankModel(
    string Name,
    double FillPercent,
    int Liters,
    int CapacityLiters,
    bool IsLow);
