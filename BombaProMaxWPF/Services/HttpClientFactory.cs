using System.Net.Http;

namespace BombaProMaxWPF.Services;

/// <summary>
/// Factory for creating/providing HttpClient instances.
/// Uses a singleton HttpClient for efficient connection management.
/// </summary>
public static class HttpClientFactory
{
    private static HttpClient? _sharedClient;
    private static readonly object _lock = new();

    /// <summary>
    /// Gets the shared HttpClient instance.
    /// </summary>
    /// <returns>A configured HttpClient instance</returns>
    public static HttpClient Create()
    {
        if (_sharedClient is null)
        {
            lock (_lock)
            {
                if (_sharedClient is null)
                {
                    _sharedClient = new HttpClient
                    {
                        Timeout = TimeSpan.FromSeconds(30)
                    };

                    System.Diagnostics.Debug.WriteLine("[HttpClientFactory] Created shared HttpClient");
                }
            }
        }

        return _sharedClient;
    }
}
