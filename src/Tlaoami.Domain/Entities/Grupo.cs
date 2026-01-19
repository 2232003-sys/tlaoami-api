using System;
using System.Collections.Generic;

namespace Tlaoami.Domain.Entities
{
    public class Grupo
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }  // Ej: "2A", "2B"
        public int Grado { get; set; }       // Ej: 1, 2, 3
        public string? Turno { get; set; }   // Ej: "Matutino", "Vespertino"
        public Guid CicloEscolarId { get; set; }
        public CicloEscolar? CicloEscolar { get; set; }

        public ICollection<AlumnoGrupo> Alumnos { get; set; } = new List<AlumnoGrupo>();
    }
}
