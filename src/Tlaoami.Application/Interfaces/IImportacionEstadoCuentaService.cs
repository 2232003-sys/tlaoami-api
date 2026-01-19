using Microsoft.AspNetCore.Http;
using Tlaoami.Application.Contracts;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Interfaces;

public interface IImportacionEstadoCuentaService
{
    Task<ImportacionResultadoDto> ImportarAsync(IFormFile archivoCsv);
    Task<IEnumerable<MovimientoBancarioDto>> GetMovimientosBancariosAsync(
        EstadoConciliacion? estado,
        TipoMovimiento? tipo,
        DateTime? desde,
        DateTime? hasta,
        int page = 1,
        int pageSize = 50
    );
}
