using BombaProMaxApi.Data;
using Microsoft.EntityFrameworkCore;

namespace BombaProMaxApi.MultiTenancy;

/// <summary>
/// Factory that creates AppDbContext instances with the correct connection string
/// based on the current tenant.
/// </summary>
public class MultiTenantDbContextFactory : IDbContextFactory<AppDbContext>
{
    private readonly ITenantService _tenantService;
    private readonly ILogger<MultiTenantDbContextFactory> _logger;

    public MultiTenantDbContextFactory(
        ITenantService tenantService,
        ILogger<MultiTenantDbContextFactory> logger)
    {
        _tenantService = tenantService;
        _logger = logger;
    }

    public AppDbContext CreateDbContext()
    {
        var connectionString = _tenantService.GetConnectionString();
        var tenantId = _tenantService.GetCurrentTenantId();

        _logger.LogDebug("Creating DbContext for tenant '{TenantId}'", tenantId);

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new AppDbContext(optionsBuilder.Options);
    }
}
