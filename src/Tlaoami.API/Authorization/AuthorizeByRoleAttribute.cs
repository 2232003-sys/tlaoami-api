using Microsoft.AspNetCore.Authorization;
using Tlaoami.Domain.Enums;

namespace Tlaoami.API.Authorization
{
    /// <summary>
    /// Atributo para autorizar endpoints por rol específico.
    /// Uso: [AuthorizeByRole(UserRole.Owner, UserRole.Secretaria)]
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeByRoleAttribute : AuthorizeAttribute
    {
        public AuthorizeByRoleAttribute(params UserRole[] roles)
        {
            if (roles.Length == 0)
                throw new ArgumentException("Debe especificar al menos un rol");

            // Convertir enum roles a strings para el atributo base
            var roleStrings = roles.Select(r => r switch
            {
                UserRole.Owner => "Owner",
                UserRole.Secretaria => "Secretaria",
                UserRole.Maestro => "Maestro",
                _ => throw new ArgumentException($"Rol desconocido: {r}")
            });

            Roles = string.Join(",", roleStrings);
        }
    }
}
