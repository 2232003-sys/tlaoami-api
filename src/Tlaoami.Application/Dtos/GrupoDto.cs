using System;

namespace Tlaoami.Application.Dtos
{
    public class GrupoDto
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }
        public int Grado { get; set; }
        public string? Turno { get; set; }
        public Guid CicloEscolarId { get; set; }
        public string? CicloNombre { get; set; }
    }

    public class GrupoCreateDto
    {
        public string? Nombre { get; set; }
        public int Grado { get; set; }
        public string? Turno { get; set; }
        public Guid CicloEscolarId { get; set; }
    }
}
