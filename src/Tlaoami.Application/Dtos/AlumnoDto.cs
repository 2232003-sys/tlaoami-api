using System;
using System.Collections.Generic;

namespace Tlaoami.Application.Dtos
{
    public class AlumnoDto
    {
        public Guid Id { get; set; }
        public string? Matricula { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public bool Activo { get; set; }
        public DateTime FechaInscripcion { get; set; }
        public GrupoDto? GrupoActual { get; set; }
        
        // Extended properties for frontend
        public Guid? GrupoId { get; set; }
        public string? GrupoNombre { get; set; }
        public Guid? CicloId { get; set; }
        public string? CicloNombre { get; set; }
        public decimal? SaldoPendiente { get; set; }
    }

    public class AlumnoCreateDto
    {
        public string? Matricula { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
    }

    public class AlumnoUpdateDto
    {
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }
        public string? Email { get; set; }
        public string? Telefono { get; set; }
        public bool? Activo { get; set; }
    }
}
