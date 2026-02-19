namespace ExpenseTracker.API.ExceptionHandlers;

using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
public class ValidationExceptionHandler : IExceptionHandler
{
    private readonly ILogger<ValidationExceptionHandler> _logger;

    public ValidationExceptionHandler(ILogger<ValidationExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        if (exception is not FluentValidation.ValidationException validationEx)
        {
            return false; 
        }
        
        _logger.LogWarning("Validation failed: {Message}", validationEx.Message);
        
        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = "One or more validation errors occurred",
            Instance = httpContext.Request.Path
        };
        
        var errors = validationEx.Errors.Select(e => new { field = e.PropertyName, error = e.ErrorMessage });
        problemDetails.Extensions.Add("errors", errors);
        
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        
        return true;
    }
}