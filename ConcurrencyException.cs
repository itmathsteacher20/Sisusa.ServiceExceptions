using System.Text.Json.Serialization;

namespace Sisusa.ServiceExceptions;

/// <summary>
/// Represents an exception thrown when a concurrency conflict occurs during a service operation.
/// Typically used to indicate that an operation could not complete because the target entity has been modified
/// in the meantime by another process or operation.
/// </summary>
[JsonConverter(typeof(ServiceExceptionConverter))]
public class ConcurrencyException : ServiceException
{
    const string DEFAULT_ERROR_CODE = "CONCURRENCY_ERROR";

    /// <summary>
    /// Gets or sets the entity involved in the exception.
    /// Represents the object that encountered a concurrency conflict during a service operation.
    /// </summary>
    [JsonPropertyName("entity")]
    public object? Entity { get; init; }

    /// <summary>
    /// Represents an exception thrown when a concurrency conflict occurs during a service operation.
    /// Typically used to indicate that an operation could not complete because the target entity has been modified
    /// concurrently by another process or operation.
    /// </summary>
    /// <param name="entity">The entity involved in the exception.</param>
    /// <param name="message">Message giving more detail about the error.</param>
    public ConcurrencyException(object? entity, string message) : base(message)
    {
        Entity = entity;
        ErrorCode = DEFAULT_ERROR_CODE;
    }


    [JsonConstructor]
    public ConcurrencyException(object? entity, string message, Exception? innerException, string errorCode)
        : base(message, innerException, string.IsNullOrWhiteSpace(errorCode) ? DEFAULT_ERROR_CODE : errorCode)
    {
        Entity = entity;
    }

}

