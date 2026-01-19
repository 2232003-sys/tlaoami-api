using System;
using System.Collections.Generic;

namespace Tlaoami.Domain.Entities
{
    public class CicloEscolar
    {
        public Guid Id { get; set; }
        public string? Nombre { get; set; }  // Ej: "2025-2026", "Ciclo I 2025"
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public bool Activo { get; set; } = true;

        public ICollection<Grupo> Grupos { get; set; } = new List<Grupo>();
    }
}
