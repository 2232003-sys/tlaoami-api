using Microsoft.EntityFrameworkCore;
using Tlaoami.API.Kpi.Dtos;
using Tlaoami.Infrastructure;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain.Enums;

namespace Tlaoami.API.Kpi.Queries;

/// <summary>
/// Queries de solo lectura para KPI financiero
/// 
/// Características:
/// - Ejecuta queries directas contra DbContext
/// - Sin lógica de negocio
/// - Sin modificación de datos
/// - Apenas ejecuta, no cachea ni transforma
/// 
/// Regla: Si necesitas lógica, no va aquí
/// </summary>
public class DashboardFinancieroQueries
{
    private readonly TlaoamiDbContext _dbContext;

    public DashboardFinancieroQueries(TlaoamiDbContext dbContext)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
    }

    /// <summary>
    /// Obtiene el dashboard financiero completo con todas las métricas
    /// </summary>
    public async Task<DashboardFinancieroKpiDto> ObtenerDashboardFinancieroAsync()
    {
        var ahora = DateTime.UtcNow;
        var inicioMes = new DateTime(ahora.Year, ahora.Month, 1);
        var inicioDia = ahora.Date;

        var dto = new DashboardFinancieroKpiDto
        {
            IngresosMes = await ObtenerIngresosMesAsync(inicioMes),
            IngresosHoy = await ObtenerIngresosHoyAsync(inicioDia),
            AdeudoTotal = await ObtenerAdeudoTotalAsync(),
            AlumnosConAdeudo = await ObtenerAlumnosConAdeudoAsync(),
            GastosMes = await ObtenerGastosMesAsync(inicioMes),
            MovimientosSinConciliar = await ObtenerMovimientosSinConciliarAsync(),
            PagosDetectadosAutomaticamente = await ObtenerPagosDetectadosAutomaticamenteAsync()
        };

        return dto;
    }

    /// <summary>
    /// Suma de pagos del mes actual
    /// SELECT SUM(Monto) FROM Pago WHERE FechaPago >= inicio del mes
    /// </summary>
    private async Task<decimal> ObtenerIngresosMesAsync(DateTime inicioMes)
    {
        return await _dbContext.Set<Pago>()
            .Where(p => p.FechaPago >= inicioMes)
            .SumAsync(p => (decimal?)p.Monto) ?? 0m;
    }

    /// <summary>
    /// Suma de pagos del día actual
    /// SELECT SUM(Monto) FROM Pago WHERE FechaPago >= hoy a las 00:00
    /// </summary>
    private async Task<decimal> ObtenerIngresosHoyAsync(DateTime inicioDia)
    {
        return await _dbContext.Set<Pago>()
            .Where(p => p.FechaPago >= inicioDia)
            .SumAsync(p => (decimal?)p.Monto) ?? 0m;
    }

    /// <summary>
    /// Suma total de adeudos calculada desde facturas
    /// Adeudo = Monto de factura - Total de pagos realizados
    /// Consideramos solo facturas con estado pendiente o parcialmente pagada
    /// </summary>
    private async Task<decimal> ObtenerAdeudoTotalAsync()
    {
        var facturas = await _dbContext.Set<Factura>()
            .Where(f => f.Estado == EstadoFactura.Pendiente || f.Estado == EstadoFactura.ParcialmentePagada)
            .Include(f => f.Pagos)
            .ToListAsync();

        var adeudoTotal = facturas
            .AsEnumerable()
            .Select(f => 
            {
                var totalPagado = f.Pagos?.Sum(p => p.Monto) ?? 0m;
                return f.Monto - totalPagado;
            })
            .Where(saldo => saldo > 0)
            .Sum();

        return adeudoTotal;
    }

    /// <summary>
    /// Conteo de alumnos con adeudo pendiente
    /// Alumnos que tienen al menos una factura con saldo pendiente
    /// </summary>
    private async Task<int> ObtenerAlumnosConAdeudoAsync()
    {
        var alumnosConAdeudo = await _dbContext.Set<Factura>()
            .Where(f => f.Estado == EstadoFactura.Pendiente || f.Estado == EstadoFactura.ParcialmentePagada)
            .Select(f => f.AlumnoId)
            .Distinct()
            .CountAsync();

        return alumnosConAdeudo;
    }

    /// <summary>
    /// Suma de movimientos bancarios tipo retiro del mes
    /// Los retiros representan gastos/egresos del banco
    /// </summary>
    private async Task<decimal> ObtenerGastosMesAsync(DateTime inicioMes)
    {
        return await _dbContext.Set<MovimientoBancario>()
            .Where(mb => mb.Tipo == TipoMovimiento.Retiro && mb.Fecha >= inicioMes)
            .SumAsync(mb => (decimal?)mb.Monto) ?? 0m;
    }

    /// <summary>
    /// Conteo de movimientos bancarios sin conciliar
    /// Identifica movimientos que no tienen una conciliación asociada
    /// </summary>
    private async Task<int> ObtenerMovimientosSinConciliarAsync()
    {
        // Obtenemos IDs de movimientos que SÍ están conciliados
        var movimientosConciliados = await _dbContext.Set<MovimientoConciliacion>()
            .Select(mc => mc.MovimientoBancarioId)
            .Distinct()
            .ToListAsync();

        // Contamos movimientos que NO están en la lista de conciliados
        return await _dbContext.Set<MovimientoBancario>()
            .Where(mb => !movimientosConciliados.Contains(mb.Id))
            .CountAsync();
    }

    /// <summary>
    /// Conteo de conciliaciones exitosas de pagos
    /// Se cuentan las conciliaciones que tienen FacturaId (pagos asignados automáticamente a facturas)
    /// </summary>
    private async Task<int> ObtenerPagosDetectadosAutomaticamenteAsync()
    {
        return await _dbContext.Set<MovimientoConciliacion>()
            .Where(mc => mc.FacturaId.HasValue)
            .CountAsync();
    }
}
