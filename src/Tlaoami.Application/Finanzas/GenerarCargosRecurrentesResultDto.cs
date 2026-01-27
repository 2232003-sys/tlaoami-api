using System;
using System.Collections.Generic;

namespace Tlaoami.Application.Finanzas
{
    /// <summary>
    /// Resultado de la generación de cargos recurrentes.
    /// </summary>
    public class GenerarCargosRecurrentesResultDto
    {
        public int TotalAsignaciones { get; set; }
        public int CargosCreados { get; set; }
        public int OmitidasPorExistir { get; set; }
        public List<string> Errores { get; set; } = new List<string>();
    }
}
