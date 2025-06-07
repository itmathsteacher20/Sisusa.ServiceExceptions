using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Sisusa.ServiceExceptions;

/// <summary>
/// An exception thrown when the application encounters a security problem.
/// </summary>
[JsonConverter(typeof(ServiceExceptionConverter))]
public class SecurityException : ServiceException
{
    private const string DEFAULT_MSG = "A security issue has been detected, preventing the requested operation. Please contact support if you need further assistance";

    private const string DEFAULT_CODE = "SECURITY_ERROR";

    /// <summary>
    /// Creates a new instance with the specified message explaining what caused the exceptional case
    /// and possible solutions to the issue.
    /// </summary>
    /// <param name="message">Message specifying what the security concern is.</param>
    public SecurityException(string message = "") :
        base(string.IsNullOrWhiteSpace(message) ? DEFAULT_MSG : message)
    {
        ErrorCode = DEFAULT_CODE; 
    }

    /// <summary>
    /// Creates a new instance with the specified message and exception.
    /// </summary>
    /// <param name="message">Message specifying what the security concern is.</param>
    /// <param name="innerException">The native <see cref="Exception"/> that the application encountered.</param>
    public SecurityException(string message, Exception innerException) :
        base(
        string.IsNullOrWhiteSpace(message) ? DEFAULT_MSG : message,
        innerException ?? 
           throw new ArgumentNullException(nameof(innerException),"Expected an Exception to wrap, received none.")
            )
    {
        ErrorCode = DEFAULT_CODE;
    }

    /// <summary>
    /// Initializes a new <see cref="SecurityException"/> accepting the given message, exception and errorCode.
    /// 
    /// </summary>
    /// <remarks>The JsonConstructor for populating this from a JSON string.</remarks>
    /// <param name="message">The message explaining the security issue that occured.</param>
    /// <param name="innerException">Inner exception to maintian the call stack.</param>
    /// <param name="errorCode">Error code specifying the type of security error, defaults to `SECURITY_ERROR`</param>
    [JsonConstructor]
    public SecurityException(string message, Exception? innerException, string errorCode=DEFAULT_CODE)
        :base(
            string.IsNullOrWhiteSpace(message) ? DEFAULT_MSG : message,
            innerException,
            string.IsNullOrWhiteSpace(errorCode) ? DEFAULT_CODE :
            errorCode) { }
}