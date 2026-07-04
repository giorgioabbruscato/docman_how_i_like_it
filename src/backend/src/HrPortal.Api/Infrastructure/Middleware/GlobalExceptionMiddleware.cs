using HrPortal.Api.Infrastructure.Persistence;
using HrPortal.SharedKernel.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace HrPortal.Api.Infrastructure.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
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
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail, errorCode) = MapException(exception);

        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = _environment.IsDevelopment() ? exception.Message : detail,
            Type = $"https://httpstatuses.com/{statusCode}",
            Instance = context.Request.Path
        };

        if (errorCode is not null)
            problem.Extensions["errorCode"] = errorCode;

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }

    private static (int StatusCode, string Title, string Detail, string? ErrorCode) MapException(Exception exception) =>
        exception switch
        {
            NotFoundException notFound => (
                StatusCodes.Status404NotFound,
                "Resource not found",
                notFound.Message,
                notFound.ErrorCode),
            DomainException domain => (
                StatusCodes.Status400BadRequest,
                "Domain error",
                domain.Message,
                domain.ErrorCode),
            UnauthorizedAccessException => (
                StatusCodes.Status403Forbidden,
                "Forbidden",
                "You do not have permission to perform this action.",
                "FORBIDDEN"),
            _ => (
                StatusCodes.Status500InternalServerError,
                "An unexpected error occurred",
                "An internal server error occurred. Please try again later.",
                "INTERNAL_ERROR")
        };
}
