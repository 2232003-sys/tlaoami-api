using System;

namespace Tlaoami.Domain.Entities
{
    /// <summary>
    /// Registro de proceso de reinscripción de alumno a nuevo ciclo/grupo.
    /// Auditoría + control de bloqueos por adeudo.
    /// </summary>
    public class Reinscripcion
    {
        /// <summary>Identificador único (UUID)</summary>
        public Guid Id { get; set; }

        /// <summary>Alumno siendo reinscrito (FK)</summary>
        public Guid AlumnoId { get; set; }

        /// <summary>Ciclo de origen (puede ser null si no había ciclo activo)</summary>
        public Guid? CicloOrigenId { get; set; }

        /// <summary>Grupo de origen (puede ser null)</summary>
        public Guid? GrupoOrigenId { get; set; }

        /// <summary>Ciclo destino (obligatorio)</summary>
        public Guid CicloDestinoId { get; set; }

        /// <summary>Grupo destino (obligatorio)</summary>
        public Guid GrupoDestinoId { get; set; }

        /// <summary>
        /// Estado del proceso: Solicitud, Aprobada, Rechazada, Completada, Bloqueada
        /// Fase 1: Completada (éxito) o Bloqueada (adeudo)
        /// </summary>
        public string Estado { get; set; } = "Solicitud"; // "Completada", "Bloqueada"

        /// <summary>Motivo de bloqueo si aplica (ej: "ADEUDO", "ALUMNO_YA_INSCRITO")</summary>
        public string? MotivoBloqueo { get; set; }

        /// <summary>Saldo de cuenta al momento de la solicitud (para auditoría)</summary>
        public decimal? SaldoAlMomento { get; set; }

        /// <summary>Timestamp cuando fue creada la solicitud (UTC)</summary>
        public DateTime CreatedAtUtc { get; set; }

        /// <summary>Timestamp cuando fue completada o rechazada (UTC)</summary>
        public DateTime? CompletedAtUtc { get; set; }

        /// <summary>Usuario que solicitó la reinscripción (si JWT disponible)</summary>
        public Guid? CreatedByUserId { get; set; }

        // Relaciones de navegación
        public Alumno? Alumno { get; set; }
        public CicloEscolar? CicloOrigen { get; set; }
        public Grupo? GrupoOrigen { get; set; }
        public CicloEscolar? CicloDestino { get; set; }
        public Grupo? GrupoDestino { get; set; }
    }
}
