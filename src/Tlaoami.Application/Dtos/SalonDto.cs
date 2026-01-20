using System;

namespace Tlaoami.Application.Dtos
{
    public class SalonCreateDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int? Capacidad { get; set; }
    }

    public class SalonUpdateDto
    {
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public int? Capacidad { get; set; }
        public bool? Activo { get; set; }
    }

    public class SalonDto
    {
        public Guid Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public int? Capacidad { get; set; }
        public bool Activo { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
