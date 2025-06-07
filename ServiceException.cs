using System.Text.Json.Serialization;

namespace Sisusa.ServiceExceptions;

/// <summary>
/// Represents an exception that is specific to a service-related operation.
/// This exception can be used to indicate errors or issues encountered
/// during the execution of service requests or operations.
/// </summary>
[JsonConverter(typeof(ServiceExceptionConverter))]
public class ServiceException : Exception
{
    const string DEFAULT_ERROR_CODE = "SERVICE_ERROR";

    /// <summary>
    /// Represents an exception specific to service-related operations or errors.
    /// Can be used to handle and provide additional context about service-level issues.
    /// </summary>
    public ServiceException(string message) : base(message)
    {
        ErrorCode = DEFAULT_ERROR_CODE;
    }

    /// <summary>
    /// Represents an exception specific to service-related operations or errors.
    /// Can be used to handle and provide additional context about service-level issues.
    /// </summary>
    public ServiceException(string message, Exception innerException) : base(message, innerException)
    {
        ErrorCode = DEFAULT_ERROR_CODE;
    }

    public ServiceException() : base("An exception occurred during service operation.")
    {
        ErrorCode = DEFAULT_ERROR_CODE;
    }


    /// Initializes a new instance of the <see cref="ServiceException"/> class with a specified error message, 
    /// a reference to the inner exception that is the cause of this exception, and a specific error code.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
    /// <param name="errorCode">A string representing the error code associated with the exception. If null, defaults to "SERVICE_ERROR".</param>
    /// <param name="innerException"></param>
    /// <param name="errorCode"></param>
    [JsonConstructor]
    public ServiceException(string message, Exception? innerException, string errorCode = DEFAULT_ERROR_CODE)
        :base(message, innerException)
    {
        if (string.IsNullOrEmpty(errorCode))
        {
            ErrorCode = DEFAULT_ERROR_CODE;
        }
        else
        {
            ErrorCode = errorCode;
            
        }
        //ErrorCode = errorCode ?? "SERVICE_ERROR";
    }

    /// <summary>
    /// Gets or sets the error code associated with the exception.
    /// The error code provides a predefined identifier for the specific error or issue
    /// encountered during a service operation, allowing for categorization or
    /// easier tracking of service-level issues.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public string ErrorCode { get; protected set; }

    
}
