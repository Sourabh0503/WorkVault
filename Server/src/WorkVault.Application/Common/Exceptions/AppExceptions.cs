namespace WorkVault.Application.Common.Exceptions;

/// <summary>
/// Base class for all application-level exceptions. Each subclass maps
/// to a specific HTTP status code via ExceptionHandlingMiddleware.
/// 
/// Why custom exceptions instead of generic InvalidOperationException?
/// - Middleware can map to proper HTTP status codes
/// - Type-safe — caller knows what each handler can throw
/// - Stack trace is cleaner (only domain logic, not framework noise)
/// </summary>
public abstract class AppException(string message) : Exception(message);

public class NotFoundException(string message) : AppException(message);

public class ConflictException(string message) : AppException(message);

public class BusinessRuleException(string message) : AppException(message);