using System;
using Tlaoami.Domain.Enums;

namespace Tlaoami.Application.Dtos
{
    public class BecaAlumnoCreateDto
    {
        public Guid AlumnoId { get; set; }
        public Guid CicloId { get; set; }
        public BecaTipo Tipo { get; set; }
        public decimal Valor { get; set; }
        public bool Activa { get; set; } = true;
    }

    public class BecaAlumnoUpdateDto
    {
        public BecaTipo? Tipo { get; set; }
        public decimal? Valor { get; set; }
        public bool? Activa { get; set; }
    }

    public class BecaAlumnoDto
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public Guid CicloId { get; set; }
        public BecaTipo Tipo { get; set; }
        public decimal Valor { get; set; }
        public bool Activa { get; set; }
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }
}
