namespace Tlaoami.Application.Dtos
{
    /// <summary>DTO de respuesta para Aviso de Privacidad vigente</summary>
    public class AvisoPrivacidadDto
    {
        public Guid Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
        public DateTime PublicadoEnUtc { get; set; }
    }

    /// <summary>DTO para crear/publicar un nuevo Aviso de Privacidad (admin)</summary>
    public class AvisoPrivacidadCreateDto
    {
        public string Version { get; set; } = string.Empty;
        public string Contenido { get; set; } = string.Empty;
    }

    /// <summary>DTO de respuesta para estado de aceptación del usuario</summary>
    public class EstadoAceptacionDto
    {
        /// <summary>Si hay aviso vigente y el usuario no lo ha aceptado</summary>
        public bool RequiereAceptacion { get; set; }

        /// <summary>Versión del aviso vigente (si existe)</summary>
        public string? VersionActual { get; set; }

        /// <summary>Cuándo el usuario aceptó (null si no ha aceptado)</summary>
        public DateTime? AceptadoEnUtc { get; set; }
    }

    /// <summary>DTO para aceptación (POST body, puede estar vacío)</summary>
    public class AceptarAvisoDto
    {
        // Por ahora vacío, pero puede tener campos en el futuro
        // Ej: "leído": true, "aceptadoComoConsultado": true
    }
}
