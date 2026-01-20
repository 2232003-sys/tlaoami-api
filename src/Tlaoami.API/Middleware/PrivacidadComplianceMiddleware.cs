using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authorization;
using Tlaoami.Application.Interfaces;

namespace Tlaoami.API.Middleware
{
    /// <summary>
    /// Middleware que verifica que el usuario haya aceptado el aviso de privacidad vigente.
    /// Si no lo ha aceptado, bloquea el acceso (403) a endpoints protegidos.
    /// Excepciones: /activo, /estado, /aceptar (endpoints de privacidad), /auth/login, Swagger.
    /// </summary>
    public class PrivacidadComplianceMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string[] _endpointsExentos = new[]
        {
            "/api/v1/avisoprivacidad/activo",
            "/api/v1/avisoprivacidad/estado",
            "/api/v1/avisoprivacidad/aceptar",
            "/api/v1/auth/login",
            "/swagger",
            "/healthz"
        };

        public PrivacidadComplianceMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IAvisoPrivacidadService avisoService)
        {
            // Verificar si es un endpoint exento
            var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
            if (_endpointsExentos.Any(exento => path.StartsWith(exento, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            // Verificar si está autenticado
            var usuarioId = ObtenerUsuarioIdDelContext(context);
            if (usuarioId == Guid.Empty)
            {
                // No autenticado, dejar que siga (otros middlewares lo rechazarán)
                await _next(context);
                return;
            }

            // Verificar si aceptó el aviso vigente
            var haAceptado = await avisoService.UsuarioHaAceptadoVigenteAsync(usuarioId);
            if (!haAceptado)
            {
                // Bloquear: usuario no aceptó aviso vigente
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(
                    @"{""code"":""PRIVACIDAD_PENDIENTE"",""message"":""Debe aceptar el aviso de privacidad vigente para acceder a este recurso.""}");
                return;
            }

            await _next(context);
        }

        private Guid ObtenerUsuarioIdDelContext(HttpContext context)
        {
            var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? context.User.FindFirst("sub")
                ?? context.User.FindFirst("oid");

            if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var usuarioId))
                return usuarioId;

            return Guid.Empty;
        }
    }

    public static class PrivacidadComplianceMiddlewareExtensions
    {
        public static IApplicationBuilder UsePrivacidadCompliance(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PrivacidadComplianceMiddleware>();
        }
    }
}
