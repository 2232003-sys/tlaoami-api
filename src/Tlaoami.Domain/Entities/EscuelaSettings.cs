using System;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Configuración de la escuela. Una sola fila por escuela.
    /// Contiene información institucional y reglas de negocio.
    /// </summary>
    public class EscuelaSettings
    {
        public Guid Id { get; set; }

        /// <summary>ID de la escuela a la que aplican estas configuraciones.</summary>
        public Guid EscuelaId { get; set; }

        // ===== INFORMACIÓN INSTITUCIONAL =====

        /// <summary>Nombre oficial de la escuela (ej: "Escuela Primaria Los Andes").</summary>
        public string Nombre { get; set; } = string.Empty;

        /// <summary>Razón social para documentos fiscales (ej: "EDUCACION LOS ANDES SA DE CV").</summary>
        public string RazonSocial { get; set; } = string.Empty;

        /// <summary>Dirección completa de la escuela.</summary>
        public string Direccion { get; set; } = string.Empty;

        /// <summary>Teléfono de contacto principal.</summary>
        public string Telefono { get; set; } = string.Empty;

        /// <summary>Email de contacto principal (ej: info@escuela.com).</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>URL del logo de la escuela.</summary>
        public string LogoUrl { get; set; } = string.Empty;

        /// <summary>Texto personalizado que aparece en recibos y comprobantes.</summary>
        public string TextoRecibos { get; set; } = string.Empty;

        // ===== CONFIGURACIÓN OPERATIVA =====

        /// <summary>Moneda para transacciones (ISO 4217, ej: "MXN").</summary>
        public string Moneda { get; set; } = "MXN";

        /// <summary>Zona horaria (IANA TZ id, ej: "America/Mexico_City").</summary>
        public string ZonaHoraria { get; set; } = "America/Mexico_City";

        /// <summary>Día de corte de colegiatura (1-31).</summary>
        public int DiaCorteColegiatura { get; set; } = 10;

        /// <summary>Si true, bloquea reinscripción cuando hay saldo pendiente &gt; 0.01.</summary>
        public bool BloquearReinscripcionConSaldo { get; set; } = false;

        // ===== AUDITORÍA =====

        /// <summary>Timestamp de creación.</summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Timestamp de última actualización.</summary>
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
