using System.Net;
using System.Text.Json;
using ExpenseTracker.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ExpenseTracker.API.Middleware;

public class GlobalExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlingMiddleware> _logger;

    public GlobalExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlingMiddleware> logger)
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
            _logger.LogError(exception, "An unhandled exception occurred");
            await HandleExceptionAsync(context, exception);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, message, errors) = exception switch
        {
            FluentValidation.ValidationException fluentValidationEx => (
                HttpStatusCode.BadRequest,
                "One or more validation errors occurred",
                fluentValidationEx.Errors.Select(e => new
                {
                    field = e.PropertyName,
                    error = e.ErrorMessage
                }).ToList()
            ),
            
           ValidationException validationEx => (
                HttpStatusCode.BadRequest,
                validationEx.Message,
                null
            ),
            
           NotFoundException notFoundEx => (
                HttpStatusCode.NotFound,
                notFoundEx.Message,
                null
            ),
            
           UnauthorizedException unauthorizedEx => (
                HttpStatusCode.Unauthorized,
                unauthorizedEx.Message,
                null
            ),
            
            ForbiddenAccessException ex => (
                HttpStatusCode.Forbidden,
                ex.Message,
                null
                ),
            
            _ => (
                HttpStatusCode.InternalServerError,
                "An error occurred while processing your request",
                null
            )
        };

        context.Response.StatusCode = (int)statusCode;

        var problemDetails = new ProblemDetails
        {
            Status = (int)statusCode,
            Title = GetTitle(statusCode),
            Detail = message,
            Instance = context.Request.Path
        };
        
        if (errors != null)
        {
            problemDetails.Extensions["errors"] = errors;
        }

        var json = JsonSerializer.Serialize(problemDetails);
        await context.Response.WriteAsync(json);
    }

    private static string GetTitle(HttpStatusCode statusCode) => statusCode switch
    {
        HttpStatusCode.BadRequest => "Bad Request",
        HttpStatusCode.Unauthorized => "Unauthorized",
        HttpStatusCode.NotFound => "Not Found",
        HttpStatusCode.InternalServerError => "Internal Server Error",
        HttpStatusCode.Forbidden => "Forbidden access",
        _ => "Error"
    };
}