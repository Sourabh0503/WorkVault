namespace WorkVault.Application.Common.Exceptions;

/// <summary>
/// Base class for all application-level exceptions.
/// </summary>
/// <remarks>
/// Each subclass maps to a specific HTTP status code via ExceptionHandlingMiddleware:
/// - <see cref="NotFoundException"/> → 404 Not Found
/// - <see cref="ConflictException"/> → 409 Conflict
/// - <see cref="BusinessRuleException"/> → 400 Bad Request
///
/// Why custom exceptions instead of generic InvalidOperationException?
/// - Middleware can map to proper HTTP status codes
/// - Type-safe — caller knows what each handler can throw
/// - Stack trace is cleaner (only domain logic, not framework noise)
///
/// Usage:
/// <code>
/// throw new NotFoundException("User not found");
/// throw new ConflictException("Email already exists");
/// throw new BusinessRuleException("Cannot delete active employee");
/// </code>
/// </remarks>
public abstract class AppException(string message) : Exception(message);

/// <summary>
/// Thrown when a requested resource does not exist.
/// Maps to HTTP 404 Not Found.
/// </summary>
public class NotFoundException(string message) : AppException(message);

/// <summary>
/// Thrown when an operation conflicts with existing state (e.g., duplicate email).
/// Maps to HTTP 409 Conflict.
/// </summary>
public class ConflictException(string message) : AppException(message);

/// <summary>
/// Thrown when a business rule is violated (e.g., invalid state transition).
/// Maps to HTTP 400 Bad Request.
/// </summary>
public class BusinessRuleException(string message) : AppException(message);