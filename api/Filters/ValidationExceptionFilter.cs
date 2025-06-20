using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace TherapyDocs.Api.Filters;

public class ValidationExceptionFilter : IExceptionFilter
{
    private readonly ILogger<ValidationExceptionFilter> _logger;

    public ValidationExceptionFilter(ILogger<ValidationExceptionFilter> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        if (context.Exception is ValidationException validationException)
        {
            _logger.LogWarning("Validation error occurred: {ErrorCount} errors", validationException.Errors.Count());
            
            var errors = validationException.Errors
                .Select(e => new ValidationError
                {
                    Property = e.PropertyName,
                    Message = e.ErrorMessage
                })
                .ToList();

            var response = new ValidationErrorResponse
            {
                Message = "Validation failed",
                Errors = errors
            };

            context.Result = new BadRequestObjectResult(response);
            context.ExceptionHandled = true;
        }
    }
}

public class ValidationErrorResponse
{
    public string Message { get; set; } = string.Empty;
    public List<ValidationError> Errors { get; set; } = new();
}

public class ValidationError
{
    public string Property { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}