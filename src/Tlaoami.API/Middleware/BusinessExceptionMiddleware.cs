using System.Text.Json;
using Tlaoami.Application.Exceptions;

namespace Tlaoami.API.Middleware
{
    public class BusinessExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public BusinessExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (BusinessException ex)
            {
                context.Response.StatusCode = StatusCodes.Status409Conflict;
                context.Response.ContentType = "application/json";
                var payload = new { code = ex.Code ?? "BUSINESS_ERROR", message = ex.Message };
                var json = JsonSerializer.Serialize(payload);
                await context.Response.WriteAsync(json);
            }
            catch (ValidationException ex)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/json";
                var payload = new { code = ex.Code ?? "VALIDATION_ERROR", message = ex.Message };
                var json = JsonSerializer.Serialize(payload);
                await context.Response.WriteAsync(json);
            }
            catch (NotFoundException ex)
            {
                context.Response.StatusCode = StatusCodes.Status404NotFound;
                context.Response.ContentType = "application/json";
                var payload = new { code = ex.Code ?? "NOT_FOUND", message = ex.Message };
                var json = JsonSerializer.Serialize(payload);
                await context.Response.WriteAsync(json);
            }
        }
    }
}
