using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly TlaoamiDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthService(TlaoamiDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == request.Username && u.Activo);

            if (user == null)
                throw new ValidationException("Usuario o contraseña incorrectos", code: "INVALID_CREDENTIALS");

            // Verify password (simple comparison for demo - in production use proper hashing)
            if (!VerifyPassword(request.Password, user.PasswordHash))
                throw new ValidationException("Usuario o contraseña incorrectos", code: "INVALID_CREDENTIALS");

            var token = GenerateJwtToken(user.Id, user.Username, user.Role);

            return new LoginResponseDto
            {
                Token = token,
                Username = user.Username,
                Role = user.Role
            };
        }

        private bool VerifyPassword(string password, string passwordHash)
        {
            // Simple comparison for demo - in production use BCrypt or similar
            return password == passwordHash;
        }

        private string GenerateJwtToken(Guid userId, string username, string role)
        {
            var jwtKey = _configuration["Jwt:Key"] ?? "TlaoamiSecretKeyForDevelopmentOnly12345678";
            var jwtIssuer = _configuration["Jwt:Issuer"] ?? "Tlaoami";
            var jwtAudience = _configuration["Jwt:Audience"] ?? "TlaoamiUsers";
            var jwtExpireMinutes = int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "60");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(System.Security.Claims.ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(System.Security.Claims.ClaimTypes.Name, username),
                new Claim(System.Security.Claims.ClaimTypes.Role, role),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: jwtAudience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(jwtExpireMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
