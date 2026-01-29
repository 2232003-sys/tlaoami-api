using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using System;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto request)
        {
            var response = await _authService.LoginAsync(request);
            return Ok(response);
        }

        /// <summary>
        /// Logout del usuario actual.
        /// Con JWT stateless, el logout es manejado por el cliente (eliminar token).
        /// Este endpoint existe para logging y auditoría.
        /// </summary>
        [HttpPost("logout")]
        [Authorize(Roles = Roles.AllRoles)]
        public ActionResult Logout()
        {
            // Obtener información del usuario del JWT para logging
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var username = User.Identity?.Name;

            // Log de auditoría
            Console.WriteLine($"[LOGOUT] Usuario: {username} (ID: {userId}) - {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC");

            // En un sistema con JWT stateless, el token se invalida del lado del cliente.
            // Si necesitas invalidación server-side, implementa una blacklist de tokens.
            
            return Ok(new { message = "Logout exitoso" });
        }
    }
}
