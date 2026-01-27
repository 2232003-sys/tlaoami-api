using System;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Configuración por escuela. Debe existir una sola fila por escuela.
    /// Controla reglas de negocio como día de corte de colegiatura y bloqueos de reinscripción.
    /// </summary>
    public class EscuelaSettings
    {
        public Guid Id { get; set; }

        /// <summary>Identificador de la escuela a la que aplican estas settings.</summary>
        public Guid EscuelaId { get; set; }

        /// <summary>Día de corte de colegiatura (1-31).</summary>
        public int DiaCorteColegiatura { get; set; }

        /// <summary>Si true, bloquea la reinscripción cuando existe saldo pendiente &gt; 0.01.</summary>
        public bool BloquearReinscripcionConSaldo { get; set; }

        /// <summary>Zona horaria (IANA TZ id, ej: "America/Mexico_City").</summary>
        public string ZonaHoraria { get; set; } = "America/Mexico_City";

        /// <summary>Moneda (ISO 4217, ej: "MXN").</summary>
        public string Moneda { get; set; } = "MXN";

        /// <summary>Timestamp de creación en UTC.</summary>
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        /// <summary>Timestamp de última actualización en UTC.</summary>
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
