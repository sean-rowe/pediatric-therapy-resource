using Microsoft.Extensions.Logging;

namespace TherapyDocs.Api.Utilities;

/// <summary>
/// Standardized error handling utilities for consistent error management across the application.
/// </summary>
public static class ErrorHandling
{
    /// <summary>
    /// Handles exceptions with standardized logging and response patterns.
    /// </summary>
    /// <typeparam name="T">The type of result to return on success.</typeparam>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">The name of the operation for logging.</param>
    /// <param name="fallbackResult">The result to return on failure.</param>
    /// <param name="logParameters">Optional parameters to include in the log.</param>
    /// <returns>The result of the operation or the fallback result on failure.</returns>
    public static async Task<T> HandleAsync<T>(
        ILogger logger,
        Func<Task<T>> operation,
        string operationName,
        T fallbackResult,
        object? logParameters = null)
    {
        try
        {
            return await operation();
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid argument in {OperationName}: {Parameters}", 
                operationName, logParameters);
            return fallbackResult;
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized access in {OperationName}: {Parameters}", 
                operationName, logParameters);
            return fallbackResult;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Invalid operation in {OperationName}: {Parameters}", 
                operationName, logParameters);
            return fallbackResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in {OperationName}: {Parameters}", 
                operationName, logParameters);
            return fallbackResult;
        }
    }

    /// <summary>
    /// Handles exceptions for operations that return boolean success indicators.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">The name of the operation for logging.</param>
    /// <param name="logParameters">Optional parameters to include in the log.</param>
    /// <returns>True if the operation succeeded, false otherwise.</returns>
    public static async Task<bool> HandleBooleanAsync(
        ILogger logger,
        Func<Task<bool>> operation,
        string operationName,
        object? logParameters = null)
    {
        return await HandleAsync(logger, operation, operationName, false, logParameters);
    }

    /// <summary>
    /// Handles exceptions for operations that don't return a value.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="operation">The operation to execute.</param>
    /// <param name="operationName">The name of the operation for logging.</param>
    /// <param name="logParameters">Optional parameters to include in the log.</param>
    /// <returns>True if the operation succeeded, false otherwise.</returns>
    public static async Task<bool> HandleVoidAsync(
        ILogger logger,
        Func<Task> operation,
        string operationName,
        object? logParameters = null)
    {
        try
        {
            await operation();
            return true;
        }
        catch (ArgumentException ex)
        {
            logger.LogWarning(ex, "Invalid argument in {OperationName}: {Parameters}", 
                operationName, logParameters);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            logger.LogWarning(ex, "Unauthorized access in {OperationName}: {Parameters}", 
                operationName, logParameters);
            return false;
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Invalid operation in {OperationName}: {Parameters}", 
                operationName, logParameters);
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error in {OperationName}: {Parameters}", 
                operationName, logParameters);
            return false;
        }
    }

    /// <summary>
    /// Validates that required string parameters are not null or empty.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when the value is null or empty.</exception>
    public static void ValidateRequiredString(string? value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException($"Parameter '{parameterName}' cannot be null or empty.", parameterName);
        }
    }

    /// <summary>
    /// Validates that an email address is in a valid format.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when the email is invalid.</exception>
    public static void ValidateEmail(string? email, string parameterName = "email")
    {
        ValidateRequiredString(email, parameterName);
        
        if (!System.Net.Mail.MailAddress.TryCreate(email, out _))
        {
            throw new ArgumentException($"Parameter '{parameterName}' must be a valid email address.", parameterName);
        }
    }

    /// <summary>
    /// Validates that a GUID is not empty.
    /// </summary>
    /// <param name="value">The GUID to validate.</param>
    /// <param name="parameterName">The name of the parameter.</param>
    /// <exception cref="ArgumentException">Thrown when the GUID is empty.</exception>
    public static void ValidateGuid(Guid value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"Parameter '{parameterName}' cannot be empty.", parameterName);
        }
    }
}