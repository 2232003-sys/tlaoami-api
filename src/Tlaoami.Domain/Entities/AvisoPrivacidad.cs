using System;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Aviso de Privacidad: Define versiones del aviso que deben ser aceptadas.
    /// Solo una versión puede estar vigente a la vez.
    /// </summary>
    public class AvisoPrivacidad
    {
        /// <summary>Identificador único (UUID)</summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Versión del aviso. Ej: "2026-01-19", "v1.0".
        /// Sirve para histórico y comunicación.
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Contenido del aviso en texto plano o HTML.
        /// Ej: "Recolectamos datos personales para..."
        /// </summary>
        public string Contenido { get; set; } = string.Empty;

        /// <summary>
        /// Indica si este aviso está actualmente vigente.
        /// Solo uno debe tener Vigente=true.
        /// </summary>
        public bool Vigente { get; set; } = false;

        /// <summary>Timestamp en UTC cuando fue publicado (puesto en vigencia)</summary>
        public DateTime PublicadoEnUtc { get; set; }

        /// <summary>Timestamp de creación en UTC</summary>
        public DateTime CreatedAtUtc { get; set; }

        // Relación de navegación
        public ICollection<AceptacionAvisoPrivacidad> Aceptaciones { get; set; } = new List<AceptacionAvisoPrivacidad>();
    }

    /// <summary>
    /// Registro de aceptación del Aviso de Privacidad por parte del usuario.
    /// Permite auditoría: quién aceptó qué versión en qué momento.
    /// </summary>
    public class AceptacionAvisoPrivacidad
    {
        /// <summary>Identificador único (UUID)</summary>
        public Guid Id { get; set; }

        /// <summary>Aviso que fue aceptado (FK)</summary>
        public Guid AvisoPrivacidadId { get; set; }

        /// <summary>Usuario que aceptó (FK)</summary>
        public Guid UsuarioId { get; set; }

        /// <summary>Timestamp en UTC cuando fue aceptado</summary>
        public DateTime AceptadoEnUtc { get; set; }

        /// <summary>
        /// Dirección IP del cliente al aceptar (opcional, para auditoría).
        /// Ej: "192.168.1.100"
        /// </summary>
        public string? Ip { get; set; }

        /// <summary>
        /// User-Agent del navegador (opcional, para auditoría).
        /// Ej: "Mozilla/5.0..."
        /// </summary>
        public string? UserAgent { get; set; }

        // Relaciones de navegación
        public AvisoPrivacidad? AvisoPrivacidad { get; set; }
        public User? Usuario { get; set; }
    }
}
