using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace Sisusa.ServiceExceptions;

/// <summary>
/// Represents an exception that occurs when there is an issue with accessing or utilizing application configuration settings.
/// </summary>
/// <remarks>
/// This exception is designed to be thrown when a specific configuration key is inaccessible, unrecognized, or causes an unexpected behavior in the application's configuration management.
/// </remarks>
/// <example>
/// The exception can contain additional diagnostic details, such as the name of the configuration key and an optional inner exception for advanced error analysis.
/// </example>
[JsonConverter(typeof(ServiceExceptionConverter))]
public class ConfigurationException : ServiceException
{
    const string DEFAULT_ERROR_CODE = "CONFIG_ERROR";

    /// <summary>
    /// The configuration key that could not be accessed.
    /// </summary>
    [JsonPropertyName("configurationKey")]
    public string ConfigurationKey { get; private set; } = string.Empty;


    /// <summary>
    /// Represents an exception that is thrown whenever a configuration key cannot be accessed.
    /// </summary>
    /// <param name="keyName">The name of the configuration key that could not be accessed.</param>
    public ConfigurationException(string keyName)
        : base($"An error occurred while accessing the configuration key: [{keyName}]")
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyName, nameof(keyName));
        ConfigurationKey = keyName;
        ErrorCode = DEFAULT_ERROR_CODE;
    }

    /// <summary>
    /// Creates a new instance of the exception using the name of the key that caused the
    /// exception and a message that explains the issue.
    /// </summary>
    /// <param name="keyName">The name of the configuration exception the application was trying
    /// to access.</param>
    /// <param name="message">An optional message that explains the exception.</param>
    public ConfigurationException(
        string keyName,
        string message = "Something went wrong while trying to access application configuration.") 
        : base(message)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(keyName, nameof(keyName));
        ConfigurationKey = keyName;
        ErrorCode = DEFAULT_ERROR_CODE;
    }

    /// <summary>
    /// Creates a new instance of the exception that includes both a message
    /// and an innerException. 
    /// </summary>
    /// <param name="configurationKey">The key that the app was trying to access when the error occurred.</param>
    /// <param name="message">Message describing the error.</param>
    /// <param name="innerException">Any exception that occurred while the key was being accessed.</param>
    public ConfigurationException(
        string configurationKey,
        Exception innerException,
        string message = "An error occurred while accessing a required configuration key."
    ) : base(message, innerException)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationKey, nameof(configurationKey));
        ArgumentNullException.ThrowIfNull(innerException);
        ConfigurationKey = configurationKey;
        ErrorCode = DEFAULT_ERROR_CODE;
    }


    [JsonConstructor]
    public ConfigurationException(string configurationKey, string message,  Exception? innerException, string errorCode = DEFAULT_ERROR_CODE)
        : base(message ?? $"An error occurred while accessing the configuration key [{configurationKey}].", innerException, errorCode ?? DEFAULT_ERROR_CODE)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(configurationKey, nameof(configurationKey));
        ConfigurationKey = configurationKey;
        ErrorCode = string.IsNullOrWhiteSpace(errorCode) ? DEFAULT_ERROR_CODE : errorCode;
    }


    /// <summary>
    /// A builder class used for creating instances of <see cref="ConfigurationException"/> with customized properties such as message, error code, and inner exception.
    /// </summary>
    public class ConfigurationExceptionBuilder(string configurationKey)
    {
        private readonly string _onKey = configurationKey ?? throw new ArgumentNullException(nameof(configurationKey));
        private Exception? _innerException;
        private string? _message;
        private string? _errorCode;

        /// <summary>
        /// Specifies the configuration key that the exception is being created for.
        /// </summary>
        /// <param name="configurationKey">The configuration key that the application could not read.</param>
        /// <returns>Builder instance for further operations.</returns>
        public static ConfigurationExceptionBuilder For(string configurationKey)
        {
            return new ConfigurationExceptionBuilder(configurationKey);
        }

        /// <summary>
        /// Sets a message explaining the security concern and optionally, possible remedial action.
        /// </summary>
        /// <param name="message">The message explaining the security concern and possible remedial action.</param>
        /// <returns>Current instance for further operations.</returns>
        public ConfigurationExceptionBuilder WithMessage(string message)
        {
            this._message = message;
            return this;
        }

        /// <summary>
        /// Sets a custom error code to make it easy to identify this exception.
        /// </summary>
        /// <param name="errorCode">The error code to use.</param>
        /// <returns>Current instance for further operations.</returns>
        public ConfigurationExceptionBuilder WithErrorCode(string errorCode)
        {
            this._errorCode = errorCode;
            return this;
        }

        /// <summary>
        /// Sets the underlying exception that caused the security concern or rather that
        /// signifies the security concern.
        /// </summary>
        /// <param name="innerException">The exception to include.</param>
        /// <returns>Current instance for further operations.</returns>
        public ConfigurationExceptionBuilder WithInnerException(Exception innerException)
        {
            this._innerException = innerException;
            return this;
        }
        
        /// <summary>
        /// Uses the given parameters to create a <see cref="ConfigurationException"/>
        /// </summary>
        /// <returns>The exception created based on the given parameters.</returns>
        public ConfigurationException Build()
        {
            var defaultMsg =
                $"An error occurred while trying to access `{_onKey}` in the application configuration.{Environment.NewLine}";
            
            if (_innerException != null)
            {
                return string.IsNullOrWhiteSpace(this._errorCode) ?
                    new ConfigurationException(
                        _onKey,
                        _innerException,
                        _message ?? defaultMsg)
                    :
                    new ConfigurationException(
                        _onKey,
                        _innerException,
                        _message ?? defaultMsg)
                    {
                        ErrorCode = this._errorCode
                    };
            }
            return string.IsNullOrWhiteSpace(_errorCode) ?
                new ConfigurationException(_onKey, _message ?? defaultMsg)
                :
                new ConfigurationException(_onKey, _message ?? defaultMsg)
                {
                    ErrorCode = this._errorCode
                };
        }
    }
    //public ConfigurationException(string message, string errorCode = "CONFIG_ERROR") : base(message){}
}