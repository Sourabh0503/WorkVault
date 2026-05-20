using System.Text.Json;
using FluentValidation;
using WorkVault.Application.Common.Exceptions;

namespace WorkVault.API.Middleware;

public class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (ValidationException ex)
        {
            await HandleValidationExceptionAsync(context, ex);
        }
        catch (NotFoundException ex)
        {
            await WriteErrorAsync(context, 404, "NotFound", ex.Message);
        }
        catch (ConflictException ex)
        {
            await WriteErrorAsync(context, 409, "Conflict", ex.Message);
        }
        catch (BusinessRuleException ex)
        {
            await WriteErrorAsync(context, 400, "BusinessRuleViolation", ex.Message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception");
            await HandleGenericExceptionAsync(context, ex);
        }
    }

    private static async Task HandleValidationExceptionAsync(
        HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        context.Response.ContentType = "application/json";

        // Group errors by property name — frontends love this shape
        var errors = ex.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray());

        var response = new
        {
            type = "ValidationFailure",
            title = "Validation Failed",
            status = 400,
            errors
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }

    private static async Task HandleGenericExceptionAsync(
        HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/json";

        var response = new
        {
            type = "ServerError",
            title = "An error occurred",
            status = 500
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
    
    private static async Task WriteErrorAsync(
        HttpContext context, int status, string type, string title)
    {
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";

        var response = new
        {
            type,
            title,
            status
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}