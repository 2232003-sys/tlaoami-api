namespace Tlaoami.Application.Interfaces;

public interface IConciliacionBancariaService
{
    Task ConciliarMovimientoAsync(Guid movimientoBancarioId, Guid? alumnoId, Guid? facturaId, string? comentario);
    Task RevertirConciliacionAsync(Guid movimientoBancarioId);
}
