using System.Net;
using System.Text.Json;
using TransportLogistics.Api.Exceptions; // Додайте цей using

namespace TransportLogistics.Api.Middleware
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception has occurred.");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var statusCode = HttpStatusCode.InternalServerError; // За замовчуванням 500
            var errorDetails = new ErrorDetails
            {
                StatusCode = (int)statusCode,
                Message = "An internal server error has occurred."
            };

            switch (exception)
            {
                case NotFoundException notFoundException:
                    statusCode = HttpStatusCode.NotFound;
                    errorDetails.StatusCode = (int)statusCode;
                    errorDetails.Message = notFoundException.Message;
                    break;
                case ValidationException validationException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorDetails.StatusCode = (int)statusCode;
                    errorDetails.Message = "One or more validation errors occurred.";
                    errorDetails.Errors = validationException.Errors; // Додаємо деталі валідації
                    break;
                case BadRequestException badRequestException:
                    statusCode = HttpStatusCode.BadRequest;
                    errorDetails.StatusCode = (int)statusCode;
                    errorDetails.Message = badRequestException.Message;
                    break;
                // Додайте інші типи винятків, якщо потрібно (наприклад, UnauthorizedAccessException)
                // case UnauthorizedAccessException unauthorizedAccessException:
                //    statusCode = HttpStatusCode.Unauthorized;
                //    errorDetails.StatusCode = (int)statusCode;
                //    errorDetails.Message = unauthorizedAccessException.Message;
                //    break;
                default:
                    // Для інших непередбачених помилок залишаємо 500
                    break;
            }

            context.Response.StatusCode = (int)statusCode;
            await context.Response.WriteAsync(JsonSerializer.Serialize(errorDetails));
        }
    }

    // Допоміжний клас для стандартизованої структури відповіді на помилку
    public class ErrorDetails
    {
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty; // <<< ВИПРАВЛЕНО CS8618!
        public IDictionary<string, string[]>? Errors { get; set; } // Для помилок валідації
    }
}