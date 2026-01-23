namespace Tlaoami.API.Kpi.Dtos;

/// <summary>
/// DTO de solo lectura para dashboard financiero KPI
/// Contiene métricas clave del estado financiero de la institución
/// 
/// Propósito: Proporcionar visibilidad en tiempo real sin lógica de negocio
/// Características:
/// - Lectura directa de base de datos
/// - Sin cálculos ni transformaciones complejas
/// - Valores actualizados al momento de la consulta
/// - No contiene validaciones
/// </summary>
public class DashboardFinancieroKpiDto
{
    /// <summary>
    /// Suma total de pagos recibidos en el mes actual
    /// </summary>
    public decimal IngresosMes { get; set; }

    /// <summary>
    /// Suma total de pagos recibidos en el día actual
    /// </summary>
    public decimal IngresosHoy { get; set; }

    /// <summary>
    /// Suma total de adeudos pendientes (saldo > 0 en estados de cuenta)
    /// </summary>
    public decimal AdeudoTotal { get; set; }

    /// <summary>
    /// Cantidad de alumnos con adeudo pendiente (saldo > 0)
    /// </summary>
    public int AlumnosConAdeudo { get; set; }

    /// <summary>
    /// Suma total de egresos bancarios del mes actual
    /// Incluye todos los movimientos bancarios tipo egreso
    /// </summary>
    public decimal GastosMes { get; set; }

    /// <summary>
    /// Cantidad de movimientos bancarios que no han sido conciliados
    /// Estado: pendiente de vincular a facturas o pagos
    /// </summary>
    public int MovimientosSinConciliar { get; set; }

    /// <summary>
    /// Cantidad de conciliaciones FIFO exitosas
    /// Pagos que fueron asignados automáticamente a facturas
    /// </summary>
    public int PagosDetectadosAutomaticamente { get; set; }
}
