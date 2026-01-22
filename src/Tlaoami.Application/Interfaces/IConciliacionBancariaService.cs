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
}
