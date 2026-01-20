using System;

namespace Tlaoami.Application.Dtos
{
    public class GrupoDto
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public int Grado { get; set; }
        public string? Turno { get; set; }
        public int? Capacidad { get; set; }
        public Guid CicloEscolarId { get; set; }
        public string? CicloNombre { get; set; }
        public Guid? SalonId { get; set; }
        public string? SalonCodigo { get; set; }
        public Guid? DocenteTitularId { get; set; }
        public string? DocenteTitularNombre { get; set; }
    }

    public class GrupoCreateDto
    {
        public string? Nombre { get; set; }
        public int Grado { get; set; }
        public string? Turno { get; set; }
        public int? Capacidad { get; set; }
        public Guid CicloEscolarId { get; set; }
    }

    public class GrupoUpdateDocenteTitularDto
    {
        public Guid? DocenteTitularId { get; set; }
    }
}
