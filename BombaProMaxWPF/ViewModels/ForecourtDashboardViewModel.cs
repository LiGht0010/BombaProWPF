using System.Collections.Generic;
using BombaProMaxWPF.Models.Forecourt;

namespace BombaProMaxWPF.ViewModels;

/// <summary>
/// Static demo data backing the "Forecourt Overview" dashboard. Mirrors the
/// reference screenshot — no service calls. Real data wiring is out of scope
/// for issue #9 and tracked in <c>docs/plans/issue-09-neon-dashboard.md §8</c>.
/// </summary>
public sealed class ForecourtDashboardViewModel
{
    public IReadOnlyList<TankModel> Tanks { get; } = new[]
    {
        new TankModel("UNLEADED", 75, 12_450, 20_000, false),
        new TankModel("DIESEL",   50, 10_000, 20_000, false),
        new TankModel("PREMIUM",  15,  3_000, 20_000, true),
    };

    public IReadOnlyList<PumpModel> Pumps { get; } = new[]
    {
        new PumpModel("PUMP 01", PumpStatus.InUse),
        new PumpModel("PUMP 02", PumpStatus.Idle),
        new PumpModel("PUMP 03", PumpStatus.InUse),
        new PumpModel("PUMP 04", PumpStatus.Check),
        new PumpModel("PUMP 05", PumpStatus.Idle),
        new PumpModel("PUMP 06", PumpStatus.InUse),
        new PumpModel("PUMP 07", PumpStatus.Idle),
        new PumpModel("PUMP 08", PumpStatus.Idle),
    };

    public IReadOnlyList<FuelPriceModel> Prices { get; } = new[]
    {
        new FuelPriceModel("UNLEADED", 1.45m, "+0.02 Market", DeltaTone.Up),
        new FuelPriceModel("DIESEL",   1.62m, "Stable",        DeltaTone.Neutral),
        new FuelPriceModel("PREMIUM",  1.88m, "-0.05 Target",  DeltaTone.Down),
    };
}
