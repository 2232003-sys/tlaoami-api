using Microsoft.AspNetCore.Mvc;
using Tlaoami.API.Kpi.Dtos;
using Tlaoami.API.Kpi.Queries;

namespace Tlaoami.API.Controllers;

/// <summary>
/// Controller para KPI financiero
/// 
/// Responsabilidad: Orquestar queries de solo lectura
/// NO contiene: lógica de negocio, validaciones complejas, modificaciones
/// 
/// Propósito: Exponer métricas financieras clave sin efectos secundarios
/// </summary>
[ApiController]
[Route("api/v1/kpi")]
public class KpiController : ControllerBase
{
    private readonly DashboardFinancieroQueries _queries;
    private readonly ILogger<KpiController> _logger;

    public KpiController(DashboardFinancieroQueries queries, ILogger<KpiController> logger)
    {
        _queries = queries ?? throw new ArgumentNullException(nameof(queries));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Obtiene el dashboard financiero con todas las métricas KPI
    /// 
    /// Métricas incluidas:
    /// - Ingresos del mes (suma de pagos)
    /// - Ingresos de hoy (suma de pagos)
    /// - Adeudo total (saldo pendiente)
    /// - Cantidad de alumnos con adeudo
    /// - Gastos del mes (movimientos bancarios tipo egreso)
    /// - Movimientos sin conciliar
    /// - Pagos detectados automáticamente (FIFO)
    /// 
    /// Endpoint: GET /api/v1/kpi/dashboard
    /// 
    /// Ejemplo de respuesta:
    /// {
    ///   "ingresosMes": 125000.00,
    ///   "ingresosHoy": 5500.00,
    ///   "adeudoTotal": 45000.00,
    ///   "alumnosConAdeudo": 12,
    ///   "gastosMes": 8000.00,
    ///   "movimientosSinConciliar": 3,
    ///   "pagosDetectadosAutomaticamente": 8
    /// }
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardFinancieroKpiDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DashboardFinancieroKpiDto>> ObtenerDashboard()
    {
        try
        {
            _logger.LogInformation("Consultando dashboard financiero KPI");
            
            var dashboard = await _queries.ObtenerDashboardFinancieroAsync();
            
            _logger.LogInformation("Dashboard financiero consultado exitosamente");
            
            return Ok(dashboard);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener dashboard financiero KPI");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                new { mensaje = "Error al consultar el dashboard financiero" });
        }
    }
}
