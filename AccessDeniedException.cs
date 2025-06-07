using System.Text.Json.Serialization;

namespace Sisusa.ServiceExceptions;

/// <summary>
/// An exception thrown when an operation is attempted without sufficient authorization.
/// </summary>
[JsonConverter(typeof(ServiceExceptionConverter))]
public class AccessDeniedException : SecurityException
{
    private const string DEFAULT_ERROR_CODE = "ACCESS_DENIED";

    /// <summary>
    /// Specifies the permission that the user lacks.
    /// </summary>
    [JsonPropertyName("requiredPermission")]
    public string RequiredPermission { get; private set; } = string.Empty;

    /// <summary>
    /// Validates the permission string before assignment.
    /// </summary>
    private static string ValidatePermission(string permission)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(permission, nameof(permission));
        return permission;
    }

    /// <summary>
    /// Represents an exception thrown when an operation is attempted without sufficient authorization.
    /// </summary>
    /// <param name="requiredPermission">The missing permission.</param>
    /// <param name="innerException">Underlying exception signifying the error that occurred.</param>
    public AccessDeniedException(string requiredPermission, Exception innerException)
        : base($"User lacks required permission: {ValidatePermission(requiredPermission)}.",
            innerException ?? throw new ArgumentNullException(nameof(innerException), "Expected valid Exception instance, none provided."))
    {
        ErrorCode = DEFAULT_ERROR_CODE;
        RequiredPermission = requiredPermission;
    }

    /// <summary>
    /// Represents an exception thrown when an operation is attempted without sufficient authorization.
    /// </summary>
    /// <param name="requiredPermission">The missing permission.</param>
    public AccessDeniedException(string requiredPermission)
        : base($"User lacks required permission: {ValidatePermission(requiredPermission)}.")
    {
        RequiredPermission = requiredPermission;
        ErrorCode = DEFAULT_ERROR_CODE;
    }

    /// <summary>
    /// JsonConstructor for deserialization.
    /// </summary>
    /// <param name="requiredPermission">The missing permission.</param>
    /// <param name="innerException">Underlying exception.</param>
    /// <param name="errorCode">Optional error code, defaults to `"ACCESS_DENIED"`.</param>
    [JsonConstructor]
    public AccessDeniedException(string requiredPermission, Exception? innerException, string errorCode = DEFAULT_ERROR_CODE)
        : base($"User lacks required permission: {ValidatePermission(requiredPermission)}.", innerException,
               string.IsNullOrWhiteSpace(errorCode) ? DEFAULT_ERROR_CODE : errorCode)
    {
        RequiredPermission = requiredPermission;
       // ErrorCode = errorCode;
    }

    /// <summary>
    /// Throws an AccessDeniedException if the predicate evaluates to true for the given value.
    /// </summary>
    /// <typeparam name="T">The type of the object being tested for permissions.</typeparam>
    /// <param name="predicate">Predicate that determines whether the user has required permission or not.</param>
    /// <param name="value">The user object being tested for permissions.</param>
    /// <param name="requiredPermission">The required permission.</param>
    /// <exception cref="AccessDeniedException">Thrown when the user lacks the required permission.</exception>
    public static void ThrowIf<T>(Func<T, bool> predicate, T value, string requiredPermission) where T : class
    {
        ArgumentNullException.ThrowIfNull(predicate, nameof(predicate));
        ArgumentNullException.ThrowIfNull(value, nameof(value));
        ArgumentException.ThrowIfNullOrWhiteSpace(requiredPermission, nameof(requiredPermission));
        if (predicate(value))
        {
            throw new AccessDeniedException(requiredPermission);
        }
    }
}
