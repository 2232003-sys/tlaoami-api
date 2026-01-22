namespace Tlaoami.Application.Dtos;

public class MovimientoDetalleDto
{
    public MovimientoDetalleMovimientoDto Movimiento { get; set; } = null!;
    public MovimientoDetalleConciliacionDto? Conciliacion { get; set; }
    public List<PagoDetalleDto> PagosRelacionados { get; set; } = new();
    public List<FacturaSimpleDto> FacturasAfectadas { get; set; } = new();
    public OutcomeDto Outcome { get; set; } = new OutcomeDto { Status = "Info", Code = "DEFAULT", Message = "Sin evaluación" };
}

public class MovimientoDetalleMovimientoDto
{
    public Guid Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class MovimientoDetalleConciliacionDto
{
    public Guid ConciliacionId { get; set; }
    public DateTime FechaConciliacion { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public AlumnoSimpleDto? Alumno { get; set; }
    public FacturaSimpleDto? Factura { get; set; }
}

public class PagoDetalleDto
{
    public Guid Id { get; set; }
    public Guid? FacturaId { get; set; }
    public Guid? AlumnoId { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string Metodo { get; set; } = string.Empty;
    public AlumnoSimpleDto? Alumno { get; set; }
}

public class OutcomeDto
{
    public string Status { get; set; } = string.Empty; // Success | Warning | Error | Info
    public string Code { get; set; } = string.Empty;   // e.g., CONCILIADO, OLD_UNCONCILED
    public string Message { get; set; } = string.Empty;
}
