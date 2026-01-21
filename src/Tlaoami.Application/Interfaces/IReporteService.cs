using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface IReporteService
    {
        Task<IEnumerable<AdeudoDto>> GetAdeudosAsync(
            Guid? cicloId = null,
            Guid? grupoId = null,
            int? grado = null,
            DateTime? fechaCorte = null);

        Task<IEnumerable<PagoReporteDto>> GetPagosAsync(
            DateTime from,
            DateTime to,
            Guid? grupoId = null,
            string? metodo = null);

        Task<string> ExportAdeudosToCsvAsync(
            Guid? cicloId = null,
            Guid? grupoId = null,
            int? grado = null,
            DateTime? fechaCorte = null);

        Task<string> ExportPagosToCsvAsync(
            DateTime from,
            DateTime to,
            Guid? grupoId = null,
            string? metodo = null);
    }
}
