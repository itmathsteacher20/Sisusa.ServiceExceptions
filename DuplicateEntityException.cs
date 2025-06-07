using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Sisusa.ServiceExceptions;

/// <summary>
/// Represents an exception that indicates the presence of a duplicate entity.
/// This exception can be used to signal that an operation encountered an entity
/// with the same parameters that already exists, which violates uniqueness constraints.
/// </summary>
[JsonConverter(typeof(ServiceExceptionConverter))]
public class DuplicateEntityException : ServiceException
{
    const string DEFAULT_CODE = "DUPLICATE_ENTITY_ERROR";

    const string DEFAULT_MSG = "An entity with the same parameters already exists. Proceeding with the requested operation would violate constraints on UNIQUENESS.";

    //private string TypeName = "";

    private static string GetDefaultMessage(string typeName)
    {
        return
            string.IsNullOrWhiteSpace(typeName) ? DEFAULT_MSG :
            $"A `{typeName}` entity with the same parameters already exists. Proceeding with the requested operation would violate constraints on UNIQUENESS.";
    }

    //static readonly string DEFAULT_MSG = $"A `{typeof(T).Name}` entity with the same parameters already exists.";
    /// <summary>
    /// Represents an exception that indicates the presence of a duplicate entity.
    /// This exception can be used to signal that an operation encountered an entity
    /// with the same parameters that already exists, which violates uniqueness constraints.
    /// </summary>
    /// <param name="message">An optional custom error message to be included in the exception. If not provided, a default message is used.</param>
    public DuplicateEntityException(string message = "") :
        base(string.IsNullOrWhiteSpace(message) ? DEFAULT_MSG : message)
    {
        ErrorCode = DEFAULT_CODE;
    }

    [JsonConstructor]
    public DuplicateEntityException(string message, Exception? innerException, string errorCode)
        : base(
            string.IsNullOrWhiteSpace(message) ? DEFAULT_MSG : message,
            innerException,
            errorCode ?? DEFAULT_CODE) { }

    /// <summary>
    /// Throws a <see cref="DuplicateEntityException"/> if the specified target object is not null.
    /// This method is used to enforce uniqueness constraints by signaling the presence
    /// of a duplicate entity during an operation.
    /// </summary>
    /// <param name="target">The object being checked for duplication. If this is not null, the exception will be thrown.</param>
    /// <param name="message">An optional custom error message to be included in the exception. If not provided, a default message is used.</param>
    /// <exception cref="DuplicateEntityException">Thrown when the target object is not null, indicating a duplicate entity exists.</exception>
    public static void ThrowIfExists<T>(T? target, string? message = null)
    {
        if (target != null)
            throw new DuplicateEntityException(message ?? GetDefaultMessage(typeof(T).Name));
    }

    /// <summary>
    /// Checks if a target entity exists and optionally satisfies a specified condition.
    /// Throws a <see cref="DuplicateEntityException"/> if the entity exists and meets the given predicate condition.
    /// </summary>
    /// <param name="target">The entity to check for existence.</param>
    /// <param name="predicate">A function to evaluate additional constraints on the target entity.</param>
    /// <param name="message">An optional custom error message to include in the exception. If not provided, a default message is used.</param>
    /// <exception cref="DuplicateEntityException">
    /// Thrown when the entity exists and satisfies the provided predicate condition.
    /// </exception>
    public static void ThrowIfExists<T>(T? target, Func<T, bool> predicate, string? message = null)
    {
        if (target != null && predicate(target))
            throw new DuplicateEntityException(message ?? GetDefaultMessage(typeof(T).Name));   
    }

    /// <summary>
    /// Checks if the given target entity satisfies the given condition and throws the <see cref="DuplicateEntityException"/> if so.
    /// Assumes the entity exists if target is not null.
    /// </summary>
    /// <param name="predicate">Function to evaluate the target entity against. </param>
    /// <param name="target">The entity whose existence to check.</param>
    /// <param name="message">Optional custom message explaining the situation.</param>
    /// <exception cref="DuplicateEntityException">
    /// If the entity exists and satisfies the provided predicate.
    /// </exception>
    public static void ThrowIf<T>(Func<T, bool> predicate,  T? target, string? message = null)
    {
        
        if (target != null && predicate(target))
            throw new DuplicateEntityException(message ?? GetDefaultMessage(typeof(T).Name));
    }
}