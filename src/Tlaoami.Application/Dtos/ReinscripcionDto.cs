using System;

namespace Tlaoami.Application.Dtos
{
    /// <summary>DTO para solicitar reinscripción de alumno</summary>
    public class ReinscripcionCreateDto
    {
        public Guid AlumnoId { get; set; }
        public Guid CicloDestinoId { get; set; }
        public Guid GrupoDestinoId { get; set; }
    }

    /// <summary>DTO de respuesta con detalles del proceso</summary>
    public class ReinscripcionDto
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public Guid? CicloOrigenId { get; set; }
        public Guid? GrupoOrigenId { get; set; }
        public Guid CicloDestinoId { get; set; }
        public Guid GrupoDestinoId { get; set; }
        public string Estado { get; set; } = string.Empty;
        public string? MotivoBloqueo { get; set; }
        public decimal? SaldoAlMomento { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? CompletedAtUtc { get; set; }
    }

    /// <summary>DTO para respuesta de bloqueo por adeudo (409)</summary>
    public class ReinscripcionBloqueadaDto
    {
        public string Code { get; set; } = "REINSCRIPCION_BLOQUEADA_ADEUDO";
        public string Message { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public string? DetalleAdeudo { get; set; }
    }

    public class ReinscripcionPreviewItemDto
    {
        public Guid AlumnoId { get; set; }
        public string NombreAlumno { get; set; } = string.Empty;
        public Guid? GrupoOrigenId { get; set; }
        public string? GrupoOrigenCodigo { get; set; }
        public decimal SaldoPendiente { get; set; }
        public bool Bloqueado { get; set; }
        public string? MotivoBloqueo { get; set; }
        public bool YaReinscrito { get; set; }
    }

    public class ReinscripcionEjecutarDto
    {
        public Guid CicloOrigenId { get; set; }
        public Guid CicloDestinoId { get; set; }
        public List<ReinscripcionItemDto> Items { get; set; } = new();
    }

    public class ReinscripcionItemDto
    {
        public Guid AlumnoId { get; set; }
        public Guid GrupoDestinoId { get; set; }
    }
}
