using System;
using System.Threading.Tasks;
using Tlaoami.Application.Finanzas;

namespace Tlaoami.Application.Interfaces
{
    /// <summary>
    /// Servicio para generar cargos recurrentes (colegiaturas, actividades) 
    /// basados en AlumnoAsignaciones activas.
    /// </summary>
    public interface IGenerarCargosRecurrentesService
    {
        /// <summary>
        /// Genera cargos mensuales para todas las asignaciones activas del periodo.
        /// </summary>
        /// <param name="periodo">Periodo en formato YYYY-MM (ej: "2026-01")</param>
        /// <param name="cicloId">ID del ciclo escolar</param>
        /// <param name="emitir">Si true, emite facturas (estado Pendiente); si false, deja en Borrador</param>
        Task<GenerarCargosRecurrentesResultDto> GenerarCargosAsync(string periodo, Guid cicloId, bool emitir = true);
    }
}
