using System;
using System.Collections.Generic;

namespace Tlaoami.Application.Dtos
{
    public class ColegiaturaGeneracionRequestDto
    {
        public Guid CicloId { get; set; }
        public string Periodo { get; set; } = string.Empty; // YYYY-MM
        public Guid? GrupoId { get; set; }
        public bool Emitir { get; set; } = false;
        public bool DryRun { get; set; } = false;
    }

    public class ColegiaturaGeneracionResultDto
    {
        public int TotalAlumnos { get; set; }
        public int Creadas { get; set; }
        public int OmitidasPorExistir { get; set; }
        public List<string> Errores { get; set; } = new();
    }

    public class RecargoAplicacionRequestDto
    {
        public Guid CicloId { get; set; }
        public string Periodo { get; set; } = string.Empty; // YYYY-MM
        public bool DryRun { get; set; } = false;
    }

    public class RecargoAplicacionResultDto
    {
        public int FacturasEvaluadas { get; set; }
        public int Aplicadas { get; set; }
        public int OmitidasPorExistir { get; set; }
        public List<string> Errores { get; set; } = new();
    }
}
