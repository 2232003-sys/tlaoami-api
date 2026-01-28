using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces;

public interface IConciliacionBancariaService
{
    Task ConciliarMovimientoAsync(
        Guid movimientoBancarioId,
        Guid? alumnoId,
        Guid? facturaId,
        string? comentario,
        bool crearPago = false,
        string metodo = "Transferencia",
        DateTime? fechaPago = null,
        bool aplicarACuenta = false);

    Task RevertirConciliacionAsync(Guid movimientoBancarioId);

    Task<Guid> ReportarPagoManualAsync(ReportarPagoManualDto dto);

    Task<IReadOnlyList<PagoDto>> GetPagosManualesAsync(Guid escuelaId);

    Task ConciliarPagoManualAsync(Guid pagoId);

    Task RevertirConciliacionManualAsync(Guid pagoId);

    Task<IReadOnlyList<SugerenciaConciliacionMvpDto>> GetSugerenciasConciliacionAsync(Guid escuelaId);

    Task<KpiConciliacionDto> GetKpisConciliacionAsync(Guid escuelaId);
}
