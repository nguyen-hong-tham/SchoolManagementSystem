using System.Net;
using System.Text.Json;

namespace UserService.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
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
                _logger.LogError(ex, "An unhandled exception occurred.");
                await HandleExceptionAsync(httpContext, ex);
            }
        }

        private static Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            
            var statusCode = HttpStatusCode.InternalServerError;
            var message = exception.Message;

            if (exception is ArgumentException || exception is InvalidOperationException)
            {
                statusCode = HttpStatusCode.BadRequest;
            }
            else if (exception is KeyNotFoundException)
            {
                statusCode = HttpStatusCode.NotFound;
            }
            else if (exception.Message.Contains("đã tồn tại") || exception.Message.Contains("không đúng") || exception.Message.Contains("không tồn tại") || exception.Message.Contains("not found"))
            {
                statusCode = HttpStatusCode.BadRequest;
            }

            context.Response.StatusCode = (int)statusCode;

            var response = new
            {
                message = message
            };

            return context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}
