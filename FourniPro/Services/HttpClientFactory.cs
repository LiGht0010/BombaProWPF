namespace FourniPro.Services;

/// <summary>
/// Factory providing a shared <see cref="HttpClient"/> instance.
/// </summary>
public static class HttpClientFactory
{
    private static HttpClient? _sharedClient;
    private static readonly object _lock = new();

    /// <summary>Gets the shared HttpClient instance.</summary>
    public static HttpClient Create()
    {
        if (_sharedClient is null)
        {
            lock (_lock)
            {
                _sharedClient ??= new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            }
        }

        return _sharedClient;
    }
}
