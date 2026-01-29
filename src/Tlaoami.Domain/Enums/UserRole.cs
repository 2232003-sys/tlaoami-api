namespace Tlaoami.Domain.Enums
{
    /// <summary>
    /// Roles de usuario en el sistema TLAOAMI.
    /// Define niveles de acceso y permisos.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Propietario/Administrador - Acceso total.
        /// Puede: Ver finanzas completas, configurar escuela, gestionar roles, conciliar.
        /// </summary>
        Owner = 0,

        /// <summary>
        /// Secretaria - Acceso administrativo limitado.
        /// Puede: Ver movimientos, operar en colegiaturas, conciliar (sin totales globales).
        /// NO puede: Ver finanzas completas, cambiar configuración.
        /// </summary>
        Secretaria = 1,

        /// <summary>
        /// Maestro - Acceso solo lectura a información de estudiantes.
        /// Puede: Ver asistencia, calificaciones, grupo asignado.
        /// NO puede: Acceder a finanzas o configuración.
        /// </summary>
        Maestro = 2
    }
}
