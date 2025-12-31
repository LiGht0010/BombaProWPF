namespace BombaProMaxApi.MultiTenancy;

/// <summary>
/// Interface for resolving the current tenant from the HTTP request.
/// </summary>
public interface ITenantService
{
    /// <summary>
    /// Gets the current tenant ID from the request context.
    /// </summary>
    string GetCurrentTenantId();

    /// <summary>
    /// Gets the connection string for the current tenant.
    /// </summary>
    string GetConnectionString();

    /// <summary>
    /// Gets all available tenant IDs (for admin/debugging purposes).
    /// </summary>
    IEnumerable<string> GetAllTenantIds();
}

/// <summary>
/// Service that resolves the current tenant from HTTP headers or defaults.
/// </summary>
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly MultiTenantSettings _settings;
    private readonly ILogger<TenantService> _logger;

    /// <summary>
    /// HTTP header name used to identify the tenant.
    /// </summary>
    public const string TenantHeaderName = "X-Tenant-ID";

    public TenantService(
        IHttpContextAccessor httpContextAccessor,
        MultiTenantSettings settings,
        ILogger<TenantService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _settings = settings;
        _logger = logger;
    }

    public string GetCurrentTenantId()
    {
        var httpContext = _httpContextAccessor.HttpContext;

        // Try to get tenant ID from header
        if (httpContext?.Request.Headers.TryGetValue(TenantHeaderName, out var tenantHeader) == true)
        {
            var tenantId = tenantHeader.FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(tenantId))
            {
                // Validate that tenant exists
                if (_settings.Tenants.Any(t => t.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase)))
                {
                    return tenantId;
                }
                _logger.LogWarning("Invalid tenant ID '{TenantId}' in header, using default", tenantId);
            }
        }

        // Fall back to default tenant
        return _settings.DefaultTenant;
    }

    public string GetConnectionString()
    {
        var tenantId = GetCurrentTenantId();
        var tenant = _settings.Tenants.FirstOrDefault(
            t => t.TenantId.Equals(tenantId, StringComparison.OrdinalIgnoreCase));

        if (tenant is null)
        {
            throw new InvalidOperationException($"Tenant '{tenantId}' not found in configuration.");
        }

        return tenant.ConnectionString;
    }

    public IEnumerable<string> GetAllTenantIds()
    {
        return _settings.Tenants.Select(t => t.TenantId);
    }
}
