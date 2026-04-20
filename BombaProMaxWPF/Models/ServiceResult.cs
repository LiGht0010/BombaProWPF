namespace BombaProMaxWPF.Models;

/// <summary>
/// Represents the result of a service operation with optional data and error details.
/// </summary>
/// <typeparam name="T">The type of data returned on success.</typeparam>
public class ServiceResult<T>
{
    /// <summary>
    /// Whether the operation was successful.
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// The data returned on success. Null on failure.
    /// </summary>
    public T? Data { get; init; }

    /// <summary>
    /// Error code for programmatic handling (e.g., "INSUFFICIENT_STOCK", "VALIDATION_ERROR").
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// User-friendly error message from the API.
    /// </summary>
    public string? ErrorMessage { get; init; }

    /// <summary>
    /// Creates a successful result with data.
    /// </summary>
    public static ServiceResult<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data
    };

    /// <summary>
    /// Creates a failure result with error details.
    /// </summary>
    public static ServiceResult<T> Failure(string? errorCode, string? errorMessage) => new()
    {
        IsSuccess = false,
        ErrorCode = errorCode,
        ErrorMessage = errorMessage
    };

    /// <summary>
    /// Creates a failure result with just an error message.
    /// </summary>
    public static ServiceResult<T> Failure(string errorMessage) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage
    };
}

/// <summary>
/// API error response structure matching the backend format.
/// </summary>
public class ApiErrorResponse
{
    public string? Error { get; set; }
    public string? Message { get; set; }
}
