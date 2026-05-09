namespace BombaProMaxWPF.Models.Forecourt;

public enum PumpStatus
{
    InUse,
    Idle,
    Check,
}

/// <summary>
/// Demo pump tile data for the Active Pump Monitor strip.
/// </summary>
public sealed record PumpModel(string Name, PumpStatus Status);
