using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sisusa.ServiceExceptions;

public class ServiceExceptionConverter : JsonConverter<ServiceException>
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private readonly JsonSerializerOptions _options;

    private const string TypeDiscriminator = "$type";

    public ServiceExceptionConverter(JsonSerializerOptions options)
    {
        _options = options;
    }

    public ServiceExceptionConverter()
    {
        _options = new JsonSerializerOptions();
    }

    public override bool CanConvert(Type typeToConvert)
    {
        return typeof(ServiceException).IsAssignableFrom(typeToConvert);
       // return base.CanConvert(typeToConvert);
    }

    public override ServiceException? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object.");
        }
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        if (!jsonDoc.RootElement.TryGetProperty(TypeDiscriminator, out var typeProp))
        {
            throw new JsonException($"Missing required property '{TypeDiscriminator}' in JSON object.");
        }

        var typeName = typeProp.GetString();
        string message = "Something went wrong - that is all we know.";
        Exception? innerException = null!;
        string errorCode = "SERVICE_ERROR";
        string configurationKey = "";
        string requiredPermission = "X";
        object? entity = null!;
        string entityName = "";
        string innerExceptionType = "";

        foreach (var property in jsonDoc.RootElement.EnumerateObject())
        {
            switch(property.Name)
            {
                case TypeDiscriminator:
                    // Skip type discriminator, already handled
                    continue;
                case "message":
                    message = property.Value.GetString() ?? "An error occurred.";
                    break;
                case "innerExceptionType":
                    innerExceptionType = property.Value.GetString() ?? "";
                    break;
                case "innerException":
                    if (property.Value.ValueKind != JsonValueKind.Null)
                    {
                        var exceptionType = Type.GetType(innerExceptionType);
                        
                        if (exceptionType != null && typeof(Exception).IsAssignableFrom(exceptionType))
                        {
                            innerException = (Exception)JsonSerializer.Deserialize(property.Value.GetRawText(), exceptionType, options) ?? null;
                        } else
                        {
                            innerException = JsonSerializer.Deserialize<Exception>(property.Value.GetRawText(), options);
                        }

                        if (innerException != null)
                        {
                            var exMsg = innerException.Message;
                            var serializedMsg = property.Value.GetProperty("Message").GetString() ?? "";
                            
                            if (!string.IsNullOrWhiteSpace(serializedMsg))
                            {
                                
                                if (!string.Equals
                                    (exMsg, serializedMsg, StringComparison.OrdinalIgnoreCase))
                                {
                                   
                                    var msgField = exceptionType!.GetField("_message", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                                    msgField?.SetValue(innerException, serializedMsg);
                                    
                                }
                            }
                        }
                    }
                    break;
                case "errorCode":
                    Console.WriteLine($"{property.Name}: {property.Value.GetString()}");
                    errorCode = property.Value.GetString();
                    break;
                case "entity":
                    entity = property.Value.ValueKind != JsonValueKind.Null ? JsonSerializer.Deserialize<object>(property.Value.GetRawText(), options) : null; 
                    break;
                case "configurationKey":
                    configurationKey = property.Value.GetString() ?? string.Empty;
                    break;
                case "entityName":
                    entityName = property.Value.GetString() ?? string.Empty;
                    break;
                case "requiredPermission":
                    requiredPermission = property.Value.GetString();
                    break;
                default:
                    // Ignore other properties
                    break;
            }
        }
        if (typeName == typeof(AuthenticationException).FullName)
        {
            return new AuthenticationException(message, innerException, errorCode ?? "AUTH_ERROR");
        }
        else if (typeName == typeof(ServiceException).FullName)
        {
            return new ServiceException(message, innerException, errorCode ?? "SERVICE_ERROR");
        }
        else if (typeName == typeof(ConcurrencyException).FullName)
        {
            return new ConcurrencyException(entity, message, innerException, errorCode ?? "CONCURRENCY_ERROR");
        }
        else if (typeName == typeof(ConfigurationException).FullName)
        {
            return new ConfigurationException(configurationKey, message, innerException, errorCode ?? "CONFIG_ERROR");
        }
        else if (typeName == typeof(DuplicateEntityException).FullName)
        {
            return new DuplicateEntityException(message, innerException, errorCode ?? "DUPLICATE_ENTITY");
        }
        else if (typeName == typeof(EntityNotFoundException).FullName)
        {
            return new EntityNotFoundException(entityName, message, innerException, errorCode ?? "NOT_FOUND");
        }
        else if (typeName == typeof(SecurityException).FullName)
        {
            return new SecurityException(message, innerException, errorCode ?? "SECURITY_ERROR");
        }
        else if (typeName == typeof(AccessDeniedException).FullName)
        {
            return new AccessDeniedException(requiredPermission, innerException, errorCode ?? "UNAUTHORIZED_ACCESS");
        }
            throw new JsonException($"Unexpected type {typeName}");
    }

    public override void Write(Utf8JsonWriter writer, ServiceException value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString(TypeDiscriminator, value.GetType().FullName);
        writer.WriteString("message", value.Message);
        writer.WriteString("errorCode", value.ErrorCode);
        if (value.InnerException != null)
        {
            writer.WriteString("innerExceptionType", value.InnerException.GetType().FullName);
            writer.WritePropertyName("innerException");
            JsonSerializer.Serialize(writer, value.InnerException, options);
   
        }
        if (value is ConcurrencyException cExc)
        {
            writer.WritePropertyName("entity");
            JsonSerializer.Serialize(writer, cExc.Entity, options);
        }
        if (value is ConfigurationException configExcept)
        {
            writer.WriteString("configurationKey", configExcept.ConfigurationKey);
        }
        if (value is EntityNotFoundException entityNotFound)
        {
            writer.WriteString("entityName", entityNotFound.EntityName);
        }
        if (value is AccessDeniedException unauthorizedAccessException)
        {
            Console.WriteLine($"Found and writing permission: {unauthorizedAccessException.RequiredPermission}");
            writer.WriteString("requiredPermission", unauthorizedAccessException.RequiredPermission);
        }

        writer.WriteEndObject();
    }
#pragma warning restore CS8618
}
