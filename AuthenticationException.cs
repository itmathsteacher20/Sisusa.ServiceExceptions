using System.Runtime.InteropServices.JavaScript;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Sisusa.ServiceExceptions;

/// <summary>
/// Represents an exception that occurs during authentication.
/// This exception is typically thrown when authentication fails due to invalid credentials
/// or other authentication-related issues.
/// </summary>
[JsonConverter(typeof(ServiceExceptionConverter))]
public class AuthenticationException : SecurityException
{

    private const string ERR_CODE = "AUTH_FAIL";

    private const string ERR_MSG = "Authentication failed. Please check your credentials and try again.";

    /// <summary>
    /// Represents an exception that occurs during authentication.
    /// This exception is typically thrown when authentication fails due to invalid credentials
    /// or other authentication-related issues.
    /// </summary>
    public AuthenticationException(string? message) :
        base(string.IsNullOrWhiteSpace(message) ? ERR_MSG : message)
    {
        ErrorCode = ERR_CODE;
    }

    public AuthenticationException() : this(ERR_MSG) { }

    /// <summary>
    /// Represents an exception that occurs during authentication.
    /// This ctoructor allows for specifying a message and an inner exception, preserving the call stack.
    /// </summary>
    /// <param name="message">Message explaining the exception.</param>
    /// <param name="innerException">Exception to wrap - contains the call stack.</param>
    public AuthenticationException(string message, Exception innerException) :
        base(
        string.IsNullOrWhiteSpace(message) ? ERR_MSG : message,
        innerException ?? throw new ArgumentNullException(nameof(innerException), "Expected an Exception to wrap, received none.")
        )
    {
        ErrorCode = ERR_CODE;
    }

    /// <summary>
    /// Represents an exception that occurs during authentication.
    /// </summary>
    /// <param name="message">Message explaining the cause for the exception.</param>
    /// <param name="errorCode">Custom error code for logging to identify the exception quicker.</param>
    public AuthenticationException(string message, string errorCode = ERR_CODE) :
        base(string.IsNullOrWhiteSpace(message) ? ERR_MSG : message)
    {
        ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? ERR_CODE : errorCode;
    }


    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationException"/> class with a specified error message,
    /// a reference to the inner exception that is the cause of this exception, and a specific error code.
    /// This constructor is typically used during deserialization or when detailed exception context is required.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    /// <param name="errorCode">A string representing the error code associated with the exception. Defaults to "AUTH_ERROR".</param>
    [JsonConstructor]
    public AuthenticationException(string message, Exception? innerException, string errorCode = ERR_CODE)
        : base(
            string.IsNullOrWhiteSpace(message) ? ERR_MSG : message,
            innerException,
            string.IsNullOrWhiteSpace(errorCode) ? ERR_CODE: errorCode)
    {
        
    }
}