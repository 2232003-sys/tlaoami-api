using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AvisoPrivacidadController : ControllerBase
    {
        private readonly IAvisoPrivacidadService _service;

        public AvisoPrivacidadController(IAvisoPrivacidadService service)
        {
            _service = service;
        }

        /// <summary>
        /// Obtiene el aviso de privacidad vigente.
        /// Endpoint público: sin autenticación.
        /// </summary>
        [HttpGet("activo")]
        [AllowAnonymous]
        public async Task<ActionResult<AvisoPrivacidadDto>> GetAvisoActivo()
        {
            var aviso = await _service.GetAvisoVigenteAsync();
            return Ok(aviso);
        }

        /// <summary>
        /// Obtiene el estado de aceptación del usuario autenticado.
        /// Requiere JWT.
        /// </summary>
        [HttpGet("estado")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<EstadoAceptacionDto>> GetEstado()
        {
            var usuarioId = ObtenerUsuarioIdDelJwt();
            var estado = await _service.GetEstadoAceptacionAsync(usuarioId);
            return Ok(estado);
        }

        /// <summary>
        /// Acepta el aviso de privacidad vigente (idempotente).
        /// Requiere JWT.
        /// Guarda IP y User-Agent para auditoría.
        /// </summary>
        [HttpPost("aceptar")]
        [Authorize(Roles = Roles.AllRoles)]
        public async Task<ActionResult<EstadoAceptacionDto>> Aceptar([FromBody] AceptarAvisoDto? dto = null)
        {
            var usuarioId = ObtenerUsuarioIdDelJwt();
            var ip = ObtenerClienteIp();
            var userAgent = ObtenerUserAgent();

            var estado = await _service.AceptarAvisoAsync(usuarioId, ip, userAgent);
            return Ok(estado);
        }

        // === Métodos helper privados ===

        private Guid ObtenerUsuarioIdDelJwt()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
                ?? User.FindFirst("sub")
                ?? User.FindFirst("oid");

            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var usuarioId))
            {
                // Fallback: usar nombre de usuario (no ideal, pero funciona)
                var usernameClaim = User.FindFirst(System.Security.Claims.ClaimTypes.Name);
                if (usernameClaim != null && Guid.TryParse(usernameClaim.Value, out var guidFromUsername))
                    return guidFromUsername;

                throw new UnauthorizedAccessException("No se pudo extraer el ID del usuario del JWT.");
            }

            return usuarioId;
        }

        private string? ObtenerClienteIp()
        {
            // Intentar obtener IP real detrás de proxy
            if (Request.Headers.TryGetValue("X-Forwarded-For", out var forwardedFor))
                return forwardedFor.ToString().Split(',')[0].Trim();

            return HttpContext.Connection.RemoteIpAddress?.ToString();
        }

        private string? ObtenerUserAgent()
        {
            if (Request.Headers.TryGetValue("User-Agent", out var userAgent))
                return userAgent.ToString();

            return null;
        }
    }
}
