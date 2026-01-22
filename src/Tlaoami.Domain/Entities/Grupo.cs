using System;
using System.Collections.Generic;

namespace Tlaoami.Domain.Entities
{
    public class Grupo
    {
        public Guid Id { get; set; }
        public required string Codigo { get; set; }  // Código único del grupo, ej: "2A-MAT-2026"
        public string? Nombre { get; set; }  // Ej: "2A", "2B"
        public int Grado { get; set; }       // Ej: 1, 2, 3
        public string? Seccion { get; set; }  // Ej: "A", "B", "C"
        public string? Turno { get; set; }   // Ej: "Matutino", "Vespertino"
        public int? Capacidad { get; set; }  // Número máximo de alumnos; null = sin límite
        public bool Activo { get; set; } = true;  // Soft delete
        public Guid CicloEscolarId { get; set; }
        public CicloEscolar? CicloEscolar { get; set; }
        
        // Relación con Salón (Fase 1 - Primaria)
        public Guid? SalonId { get; set; }
        public Salon? Salon { get; set; }

        // Relación con Docente Titular (Fase 1 - Primaria)
        public Guid? DocenteTitularId { get; set; }
        public User? DocenteTitular { get; set; }

        public ICollection<AlumnoGrupo> Alumnos { get; set; } = new List<AlumnoGrupo>();
    }
}
