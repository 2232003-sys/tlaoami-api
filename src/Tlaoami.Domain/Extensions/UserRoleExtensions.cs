using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Extensions
{
    /// <summary>
    /// Extensiones para trabajar con roles de usuario.
    /// </summary>
    public static class UserRoleExtensions
    {
        /// <summary>
        /// Convierte string a UserRole enum.
        /// </summary>
        public static UserRole ToUserRole(this string roleString)
        {
            return roleString.ToLower() switch
            {
                "owner" or "admin" => UserRole.Owner,
                "secretaria" => UserRole.Secretaria,
                "maestro" or "teacher" => UserRole.Maestro,
                _ => throw new ArgumentException($"Rol desconocido: {roleString}")
            };
        }

        /// <summary>
        /// Convierte UserRole enum a string.
        /// </summary>
        public static string ToRoleString(this UserRole role)
        {
            return role switch
            {
                UserRole.Owner => "Owner",
                UserRole.Secretaria => "Secretaria",
                UserRole.Maestro => "Maestro",
                _ => throw new ArgumentException($"Rol desconocido: {role}")
            };
        }

        /// <summary>
        /// Verifica si el usuario tiene un rol.
        /// </summary>
        public static bool HasRole(this string roleString, UserRole requiredRole)
        {
            try
            {
                var userRole = roleString.ToUserRole();
                return userRole == requiredRole;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Verifica si el usuario tiene uno de varios roles.
        /// </summary>
        public static bool HasAnyRole(this string roleString, params UserRole[] requiredRoles)
        {
            try
            {
                var userRole = roleString.ToUserRole();
                return requiredRoles.Contains(userRole);
            }
            catch
            {
                return false;
            }
        }
    }
}
