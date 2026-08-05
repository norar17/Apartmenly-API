using System.Text.Json;
using ApartmentRental.Shared;
using ApartmentRental.Shared.Exceptions;

namespace ApartmentRental.API.Middleware;

// Catches every exception that escapes the pipeline and converts it into a
// consistent ApiResponse<T> JSON body. AppException subclasses carry their
// own status/error code; anything else becomes an unhandled 500.
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
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

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        int statusCode;
        string errorCode;
        IReadOnlyDictionary<string, string[]>? errors = null;

        if (exception is ValidationAppException validationException)
        {
            statusCode = validationException.StatusCode;
            errorCode = validationException.ErrorCode;
            errors = validationException.Errors;
        }
        else if (exception is AppException appException)
        {
            statusCode = appException.StatusCode;
            errorCode = appException.ErrorCode;
        }
        else
        {
            statusCode = StatusCodes.Status500InternalServerError;
            errorCode = "INTERNAL_ERROR";
        }

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Unhandled exception on {Path}", context.Request.Path);
        }
        else
        {
            _logger.LogWarning(exception, "Handled exception on {Path}: {Message}", context.Request.Path, exception.Message);
        }

        context.Response.StatusCode = statusCode;

        var message = statusCode == StatusCodes.Status500InternalServerError && !_environment.IsDevelopment()
            ? "An unexpected error occurred."
            : exception.Message;

        var response = ApiResponse<object>.Fail(message, errorCode, errors);

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
