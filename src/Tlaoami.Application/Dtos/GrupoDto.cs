using System;

namespace Tlaoami.Application.Dtos
{
    public class GrupoDto
    {
        public Guid Id { get; set; }
        public required string Codigo { get; set; }
        public string? Nombre { get; set; }
        public int Grado { get; set; }
        public string? Seccion { get; set; }
        public string? Turno { get; set; }
        public int? Capacidad { get; set; }
        public bool Activo { get; set; }
        public Guid CicloEscolarId { get; set; }
        public string? CicloNombre { get; set; }
        public Guid? SalonId { get; set; }
        public string? SalonCodigo { get; set; }
        public Guid? DocenteTitularId { get; set; }
        public string? DocenteTitularNombre { get; set; }
        public int AlumnosInscritos { get; set; }
    }

    public class GrupoCreateDto
    {
        public required string Codigo { get; set; }
        public string? Nombre { get; set; }
        public int Grado { get; set; }
        public string? Seccion { get; set; }
        public string? Turno { get; set; }
        public int? Capacidad { get; set; }
        public Guid CicloEscolarId { get; set; }
        public Guid? SalonId { get; set; }
        public Guid? DocenteTitularId { get; set; }
    }

    public class GrupoUpdateDto
    {
        public string? Codigo { get; set; }
        public string? Nombre { get; set; }
        public int? Grado { get; set; }
        public string? Seccion { get; set; }
        public string? Turno { get; set; }
        public int? Capacidad { get; set; }
        public bool? Activo { get; set; }
        public Guid? CicloEscolarId { get; set; }
        public Guid? SalonId { get; set; }
        public Guid? DocenteTitularId { get; set; }
    }

    public class GrupoUpdateDocenteTitularDto
    {
        public Guid? DocenteTitularId { get; set; }
    }

    public class AlumnoEnGrupoDto
    {
        public Guid AlumnoId { get; set; }
        public required string Matricula { get; set; }
        public required string Nombre { get; set; }
        public required string Apellido { get; set; }
        public DateTime FechaAsignacion { get; set; }
    }
}
