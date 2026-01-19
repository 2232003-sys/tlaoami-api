using System;

namespace Tlaoami.Application.Dtos
{
    public class CicloEscolarDto
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; }
    }

    public class CicloEscolarCreateDto
    {
        public string? Nombre { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
    }
}
