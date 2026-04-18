using System.Net;
using System.Text.Json;
using OwnDeliveryApiP33.Application.Exceptions;

namespace OwnDeliveryApiP33.Infrastructure.Middleware;

public class ExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlerMiddleware> _logger;

    public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = new ErrorResponse();

        switch (exception)
        {
            case EntityNotFoundException ex:
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                response.StatusCode = 404;
                response.Message = ex.Message;
                response.Type = "EntityNotFound";
                break;

            case DuplicateEntityException ex:
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                response.StatusCode = 409;
                response.Message = ex.Message;
                response.Type = "DuplicateEntity";
                break;

            case UnauthorizedException ex:
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                response.StatusCode = 401;
                response.Message = ex.Message;
                response.Type = "Unauthorized";
                break;

            case ValidationException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = 400;
                response.Message = "One or more validation failures have occurred.";
                response.Type = "ValidationError";
                if (ex.Errors?.Any() == true)
                {
                    response.Errors = ex.Errors.ToDictionary(e => e.PropertyName, e => e.ErrorMessage);
                }
                break;

            case FluentValidation.ValidationException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = 400;
                response.Message = "One or more validation failures have occurred.";
                response.Type = "ValidationError";
                response.Errors = ex.Errors.GroupBy(e => e.PropertyName)
                    .ToDictionary(g => g.Key, g => string.Join("; ", g.Select(e => e.ErrorMessage)));
                break;

            case PaymentException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = 400;
                response.Message = ex.Message;
                response.Type = "PaymentError";
                break;

            case OwnDeliveryApiP33.Application.Exceptions.InvalidOperationException ex:
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                response.StatusCode = 400;
                response.Message = ex.Message;
                response.Type = "InvalidOperation";
                break;

            default:
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                response.StatusCode = 500;
                response.Message = "An internal server error occurred.";
                response.Type = "InternalServerError";
#if DEBUG
                response.Details = exception.Message;
                response.StackTrace = exception.StackTrace;
#endif
                break;
        }

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        return context.Response.WriteAsJsonAsync(response, options);
    }

    public class ErrorResponse
    {
        public int StatusCode { get; set; }
        public string Type { get; set; } = "Error";
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, string>? Errors { get; set; }
        public string? Details { get; set; }
        public string? StackTrace { get; set; }
    }
}

/// <summary>Extension method to add exception handler middleware</summary>
public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlerMiddleware>();
    }
}
