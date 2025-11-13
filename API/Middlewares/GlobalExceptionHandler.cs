using API.Constants;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace API.Middlewares
{
    public class GlobalExceptionHandler
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(RequestDelegate next, ILogger<GlobalExceptionHandler> logger)
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
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse
            {
                Message = ResponseMessages.UnhandledErrorOccurred,
                Details = new List<string>()
            };

            switch (exception)
            {
                case ValidationException validationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = ResponseMessages.ValidationFailed;
                    response.Details = validationException.Errors
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    break;

                case ArgumentNullException argumentNullException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = ResponseMessages.InvalidArgument;
                    response.Details.Add(argumentNullException.Message);
                    break;

                case KeyNotFoundException keyNotFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = ResponseMessages.ResourceNotFound;
                    response.Details.Add(keyNotFoundException.Message);
                    break;

                case UnauthorizedAccessException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = ResponseMessages.UnauthorizedAccess;
                    break;

                case InvalidOperationException invalidOperationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = ResponseMessages.InvalidOperation;
                    response.Details.Add(invalidOperationException.Message);
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = ResponseMessages.InternalServerError;
                    response.Details.Add(exception.Message);
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            return context.Response.WriteAsync(jsonResponse);
        }
    }

    public class ErrorResponse
    {
        public string Message { get; set; } = string.Empty;
        public List<string> Details { get; set; } = new();
    }
}
