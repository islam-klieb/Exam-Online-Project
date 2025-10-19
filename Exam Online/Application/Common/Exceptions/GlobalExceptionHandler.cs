using FluentValidation.Results;
using Microsoft.AspNetCore.Diagnostics;

namespace Exam_Online_API.Application.Common.Exceptions
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Error occurred: {Message}", exception.Message);
            httpContext.Response.ContentType = "application/json";
            var errorInfo = GetErrorInfo(exception);
            httpContext.Response.StatusCode = errorInfo.StatusCode;
            var response = new
            {
                message = errorInfo.Message,
                statusCode = errorInfo.StatusCode,
                path = httpContext.Request.Path.ToString()
            };
            await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);
            return true;
        }
        private ErrorInfo GetErrorInfo(Exception exception)
        {
            switch (exception)
            {
                case ValidationException:
                    return new ErrorInfo(400, "Please check your input and try again");

                case NotFoundException:
                    return new ErrorInfo(404, "The requested item was not found");

                case ConflictException:
                    return new ErrorInfo(409, exception.Message);

                case BusinessLogicException:
                    return new ErrorInfo(400, exception.Message);

                case UnauthorizedAccessException:
                    return new ErrorInfo(401, "You are not authorized to perform this action");

                default:
                    return new ErrorInfo(500, "Something went wrong. Please try again later");
            }
        }
        private class ErrorInfo
        {
            public int StatusCode { get; }
            public string Message { get; }

            public ErrorInfo(int statusCode, string message)
            {
                StatusCode = statusCode;
                Message = message;
            }
        }
    }
    public class ValidationException : Exception
    {
        public ValidationException(string message) : base(message) { }
    }
    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }
    public class BusinessLogicException : Exception
    {
        public BusinessLogicException(string message) : base(message) { }
    }
    public class CustomValidationException : Exception
    {
        public IDictionary<string, string[]> Errors { get; }

        public CustomValidationException(IEnumerable<ValidationFailure> failures)
            : base("One or more validation errors occurred.")
        {
            Errors = failures
                .GroupBy(f => f.PropertyName, f => f.ErrorMessage)
                .ToDictionary(g => g.Key, g => g.ToArray());
        }

        public override string ToString()
        {
            return $"{Message}: {string.Join("; ", Errors.Select(e => $"{e.Key}=[{string.Join(", ", e.Value)}]"))}";
        }
    }
}
