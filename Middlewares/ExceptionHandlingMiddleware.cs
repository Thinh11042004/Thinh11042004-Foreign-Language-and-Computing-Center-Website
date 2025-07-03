
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models;
using Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Models.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Hệ_thống_dạy_học_trung_tâm_ngoại_ngữ_và_tin_học.Middlewares
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

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse();

            switch (exception)
            {
                case UnauthorizedAccessException:
                    response.StatusCode = 401;
                    response.Message = "Unauthorized access";
                    break;
                case ValidationException validationEx:
                    response.StatusCode = 400;
                    response.Message = "Validation failed";
                    response.Details = validationEx.Message;
                    break;
                case NotFoundException:
                    response.StatusCode = 404;
                    response.Message = "Resource not found";
                    break;
                default:
                    response.StatusCode = 500;
                    response.Message = "An internal server error occurred";
                    response.Details = exception.Message;
                    break;
            }

            context.Response.StatusCode = response.StatusCode;

            var jsonResponse = JsonSerializer.Serialize(response);
            await context.Response.WriteAsync(jsonResponse);
        }
    }

}
