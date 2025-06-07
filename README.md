# Service Exceptions Documentation

This document provides comprehensive usage examples and best practices for the `Sisusa.ServiceExceptions` namespace, which contains specialized exception classes for service-layer operations.

## Table of Contents
1. [ServiceException](#serviceexception)
2. [AccessDeniedException](#accessdeniedexception)
3. [AuthenticationException](#authenticationexception)
4. [ConcurrencyException](#concurrencyexception)
5. [ConfigurationException](#configurationexception)
6. [DuplicateEntityException](#duplicateentityexception)
7. [EntityNotFoundException](#entitynotfoundexception)
8. [SecurityException](#securityexception)
9. [Advanced Usage Patterns](#advanced-usage-patterns)
10. [Best Practices](#best-practices)

---

## ServiceException

**Base class** for all service exceptions. Provides common functionality like error codes.

### Basic Usage
```csharp
throw new ServiceException("Failed to process the request");
```

### With Error Code
```csharp
throw new ServiceException("Failed to process the request") 
{ 
    ErrorCode = "PROCESSING_ERROR" 
};
```

### Real-world Example
```csharp
try 
{
    // Service operation
}
catch (Exception ex)
{
    throw new ServiceException("Order processing failed", ex);
}
```

---

## AuthenticationException

Thrown when authentication fails.

### Basic Usage
```csharp
if (!user.IsAuthenticated)
{
    throw new AuthenticationException("Invalid credentials");
}
```

### Real-world Example
```csharp
public async Task<User> AuthenticateAsync(string username, string password)
{
    var user = await _userRepository.GetByUsernameAsync(username);
    
    if (user == null || !VerifyPassword(user.PasswordHash, password))
    {
        throw new AuthenticationException("Invalid username or password");
    }
    
    return user;
}
```

---

## ConcurrencyException

Thrown when a concurrency conflict occurs (e.g., optimistic concurrency).

### Basic Usage
```csharp
throw new ConcurrencyException(entity, "Record was modified by another user");
```

### Real-world Example
```csharp
public async Task UpdateOrderAsync(Order order)
{
    var existing = await _orderRepository.GetByIdAsync(order.Id);
    
    if (existing.Version != order.Version)
    {
        throw new ConcurrencyException(order, 
            "Order was modified by another process. Please refresh and try again.");
    }
    
    await _orderRepository.UpdateAsync(order);
}
```

---

## ConfigurationException

Thrown for configuration-related issues.

### Basic Usage
```csharp
var apiKey = Configuration["ApiKey"] 
    ?? throw new ConfigurationException("ApiKey", "API key is required");
```

### Using Builder Pattern
```csharp
throw ConfigurationExceptionBuilder.For("DatabaseConnection")
    .WithMessage("Failed to establish database connection")
    .WithInnerException(dbEx)
    .WithErrorCode("DB_CONN_FAILURE")
    .Build();
```

### Real-world Example
```csharp
public string GetRequiredConfig(string key)
{
    try 
    {
        return _configuration[key] 
            ?? throw new ConfigurationException(key, $"Configuration '{key}' is missing");
    }
    catch (Exception ex)
    {
        throw ConfigurationExceptionBuilder.For(key)
            .WithMessage($"Failed to read configuration '{key}'")
            .WithInnerException(ex)
            .Build();
    }
}
```

---

## DuplicateEntityException

Thrown when a duplicate entity is detected.

### Basic Usage
```csharp
if (await _userRepository.ExistsAsync(email))
{
    throw new DuplicateEntityException($"User with email {email} already exists");
}
```

### Using Helper Methods
```csharp
var existingUser = await _userRepository.GetByEmailAsync(email);
DuplicateEntityException.ThrowIfExists(existingUser, $"Email {email} is already registered");
```

### Real-world Example
```csharp
public async Task CreateUserAsync(User newUser)
{
    // Check if email exists
    var existing = await _userRepository.GetByEmailAsync(newUser.Email);
    DuplicateEntityException.ThrowIfExists(existing, 
        $"User with email {newUser.Email} already exists");
    
    // Check if username exists with additional condition
    var usernameExists = await _userRepository.GetByUsernameAsync(newUser.Username);
    DuplicateEntityException.ThrowIfExists(usernameExists, 
        u => !u.IsDeleted, 
        $"Username {newUser.Username} is taken");
    
    await _userRepository.AddAsync(newUser);
}
```

---

## EntityNotFoundException

Thrown when an expected entity is not found.

### Basic Usage
```csharp
var user = await _userRepository.GetByIdAsync(id) 
    ?? throw new EntityNotFoundException($"User {id} not found");
```

### Using Helper Methods
```csharp
var order = await _orderRepository.GetByIdAsync(orderId);
EntityNotFoundException.ThrowIfNull(order, $"Order {orderId} not found");
```

### Real-world Example
```csharp
public async Task<Order> GetOrderDetailsAsync(int orderId, int userId)
{
    var order = await _orderRepository.GetByIdAsync(orderId);
    
    EntityNotFoundException.ThrowIfNull(order, $"Order {orderId} not found");
    
    // Additional validation
    EntityNotFoundException.ThrowIfNull(order, 
        o => o.UserId == userId, 
        $"Order {orderId} not found for user {userId}");
    
    return order;
}
```

---

## SecurityException

Base class for security-related exceptions.

### Basic Usage
```csharp
if (!IsOperationAllowed(user))
{
    throw new SecurityException("Operation not permitted due to security constraints");
}
```

### Real-world Example
```csharp
public void ProcessPayment(Payment payment)
{
    if (payment.Amount > _userService.GetMaxPaymentAmount())
    {
        throw new SecurityException(
            $"Payment amount {payment.Amount} exceeds maximum allowed");
    }
    
    // Process payment
}
```

---

## AccessDeniedException

Thrown when authorization fails(user is logged in but not permitted to do the operation) and the operation has to be halted.

### Basic Usage
```csharp
var requiredPermission = "EDIT_ORDERS";
UnauthorizedAccessException.ThrowIf(u=> !u.HasPermission(requiredPermission), user, requiredPermission);

//or if you prefer to be more verbose:
if (!user.HasPermission("EDIT_ORDERS"))
{
    throw new UnauthorizedAccessException("EDIT_ORDERS");
}

```

### Real-world Example
```csharp
public void DeleteUser(int userId, User requestingUser)
{
    if (!requestingUser.IsAdmin)
    {
        throw new UnauthorizedAccessException("DELETE_USERS");
    }
    
    _userRepository.Delete(userId);
}
```

---

## Advanced Usage Patterns

### 1. Exception Translation Middleware

```csharp
app.UseExceptionHandler(appError =>
{
    appError.Run(async context =>
    {
        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
        
        switch (exception)
        {
            case EntityNotFoundException ex:
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                await context.Response.WriteAsJsonAsync(new { ex.Message });
                break;
                
            case UnauthorizedAccessException ex:
                context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                await context.Response.WriteAsJsonAsync(new { ex.Message, ex.RequiresPermission });
                break;
                
            // Handle other exception types...
        }
    });
});
```

### 2. Domain-Specific Exception Subclasses

```csharp
public class OrderProcessingException : ServiceException
{
    public int OrderId { get; }
    
    public OrderProcessingException(int orderId, string message) 
        : base(message)
    {
        OrderId = orderId;
        ErrorCode = "ORDER_ERROR";
    }
}

// Usage:
throw new OrderProcessingException(order.Id, "Payment processing failed");
```

### 3. Exception Enrichment

```csharp
try
{
    // Service operation
}
catch (Exception ex) when (ex is not ServiceException)
{
    var enrichedEx = new ServiceException("Operation failed", ex)
    {
        ErrorCode = "OPERATION_FAILED",
        Data = { 
            ["Timestamp"] = DateTime.UtcNow,
            ["UserId"] = currentUserId
        }
    };
    throw enrichedEx;
}
```

---

## Best Practices

1. **Use Specific Exceptions**: Always use the most specific exception type that fits the error condition.

2. **Provide Context**: Include relevant information in exception messages:
   ```csharp
   // Good
   throw new EntityNotFoundException($"Product with ID {productId} not found");
   
   // Bad
   throw new EntityNotFoundException("Not found");
   ```

3. **Use Helper Methods**: Leverage the static helper methods for common checks:
   ```csharp
   EntityNotFoundException.ThrowIfNull(product);
   DuplicateEntityException.ThrowIfExists(existingUser);
   ```

4. **Preserve Stack Traces**: Always include inner exceptions when wrapping exceptions:
   ```csharp
   catch (DbException ex)
   {
       throw new ServiceException("Database operation failed", ex);
   }
   ```

5. **Standardize Error Codes**: Define a standard set of error codes for your application.

6. **Log Before Throwing**: Consider logging exceptions before throwing them:
   ```csharp
   _logger.LogError(ex, "Failed to process order {OrderId}", orderId);
   throw new OrderProcessingException(orderId, "Processing failed", ex);
   ```

7. **Document Exception Contracts**: Document which exceptions your methods can throw.

8. **Use Builder Pattern for Complex Exceptions**: For exceptions with many properties (like `ConfigurationException`), use the builder pattern for cleaner code.

9. **Consider Localization**: For user-facing messages, consider adding support for localized messages.

10. **Test Exception Cases**: Write unit tests that verify your exceptions are thrown in the right circumstances.