using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Sisusa.ServiceExceptions;

/// <summary>
/// Represents an exception thrown when an entity cannot be found.
/// This exception is typically used to indicate that a requested entity does not exist
/// or cannot be located based on the given parameters.
/// </summary>
public class EntityNotFoundException : ServiceException
{
    const string ERROR_CODE = "NOT_FOUND";

    /// <summary>
    /// Gets the name of the entity type that could not be found.
    /// This property provides the type name of the entity associated with the exception,
    /// allowing consumers to identify which entity was missing when the exception was thrown.
    /// </summary>
    [JsonPropertyName("entityName")]
    public string EntityName { get; }

    

    /// <summary>
    /// Represents an exception thrown when an entity cannot be found.
    /// This exception is typically used to indicate that a requested entity does not exist
    /// or cannot be located based on the given parameters.
    /// </summary>
    public EntityNotFoundException(Type entityType, string message ="")
        : base(string.IsNullOrWhiteSpace(message) ? GetDefaultMessage(entityType): message)
    {
        ArgumentNullException.ThrowIfNull(entityType, nameof(entityType));
        ErrorCode = ERROR_CODE;
        EntityName = entityType.Name;
        Console.WriteLine(string.IsNullOrWhiteSpace(message) ? $"No entity of type `{entityType.Name}` matching given parameters was found." : message);
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class
    /// for cases where an entity cannot be found. This constructor allows specifying
    /// a custom error message, an inner exception, and an error code.
    /// </summary>
    /// <param name="entityName">The name of the entity that was not found.</param>
    /// <param name="message">A custom error message describing the exception.</param>
    /// <param name="innerException">The exception that is the cause of this exception, if any.</param>
    /// <param name="errorCode">A custom error code for the exception. If null, defaults to "NOT_FOUND".</param>
    /// <param name="message">A custom error message describing the exception.</param>
    /// <param name="innerException">The exception that is the cause of this exception, if any.</param>
    /// <param name="errorCode">A custom error code for the exception. If null, defaults to "NOT_FOUND".</param>
    /// </summary>
    [JsonConstructor]
    public EntityNotFoundException(
        string entityName,
        string message, 
        Exception innerException,
        string errorCode) :
        base(
            string.IsNullOrWhiteSpace(message) ? 
            GetDefaultMessage(entityName) : message,
            innerException,
            string.IsNullOrWhiteSpace(errorCode) ? ERROR_CODE : errorCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(entityName, nameof(entityName));
        //ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? ERROR_CODE : errorCode; 
        EntityName = entityName;
    }


    /// <summary>
    /// Throws an <see cref="EntityNotFoundException"/> if the specified target is null.
    /// Provides a way to validate and ensure that a given entity exists.
    /// </summary>
    /// <typeparam name="T">The type of the target to be checked.</typeparam>
    /// <param name="target">The object to validate for nullity.</param>
    /// <param name="message">An optional custom message to include in the exception.</param>
    /// <exception cref="EntityNotFoundException">Thrown when the target is null.</exception>
    public static void ThrowIfNull<T>(T? target, string? message = null)
    {
        if (target == null)
            throw new EntityNotFoundException(
                typeof(T),
                message ?? GetDefaultMessage(typeof(T).Name));
    }

    /// <summary>
    /// Throws an <see cref="EntityNotFoundException"/> if the provided target object is null.
    /// Optionally allows a custom error message to be specified.
    /// </summary>
    /// <typeparam name="T">The type of the target object to be checked.</typeparam>
    /// <param name="target">The object to check for null.</param>
    /// <param name="predicate">A function to evaluate additional constraints on the target object.</param>   
    /// <param name="message">An optional custom error message to include in the exception.</param>
    /// <exception cref="EntityNotFoundException">Thrown if the target object is null.</exception>
    /// <exception cref="ArgumentNullException">Thrown if the predicate is null.</exception>
    public static void ThrowIfNull<T>(T? target, Func<T, bool> predicate, string? message = null)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        if (target == null || predicate(target))
            throw new EntityNotFoundException(
                typeof(T),
                message ?? 
                GetDefaultMessage(typeof(T)));   
    }

    private static string GetDefaultMessage(string typeName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(typeName, nameof(typeName));
        return $"No `{typeName}` entity matching the given parameters could be found.";
    }

    private static string GetDefaultMessage(Type typeOfEntity)
    {
        ArgumentNullException.ThrowIfNull(typeOfEntity, nameof(typeOfEntity));
        return GetDefaultMessage(typeOfEntity.Name);
    }
}