using System.Net.Http;

namespace BombaProMax.Services;

/// <summary>
/// Factory for creating HttpClient instances configured with tenant headers.
/// All API services should use this factory to ensure proper multi-tenant support.
/// </summary>
public static class HttpClientFactory
{
    /// <summary>
    /// Creates a new HttpClient configured with SSL bypass (for development) and tenant header.
    /// </summary>
    /// <returns>A configured HttpClient instance</returns>
    public static HttpClient Create()
    {
        // Create handler that ignores SSL certificate errors for development
        var handler = new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };

        var client = new HttpClient(handler);

        // Configure with tenant header
        ApiConfig.ConfigureHttpClient(client);

        return client;
    }

    /// <summary>
    /// Refreshes the tenant header on an existing HttpClient.
    /// Call this if the tenant ID changes during runtime.
    /// </summary>
    /// <param name="client">The HttpClient to refresh</param>
    public static void RefreshTenantHeader(HttpClient client)
    {
        ApiConfig.ConfigureHttpClient(client);
    }

    /// <summary>
    /// Clears the tenant header configuration.
    /// Call this when logging out to reset the tenant state.
    /// </summary>
    public static void ClearTenantHeader()
    {
        ApiConfig.ClearTenant();
    }
}
