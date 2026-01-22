namespace Tlaoami.Application.Dtos;

public class ConciliacionDetalleDto
{
    public Guid ConciliacionId { get; set; }
    public DateTime FechaConciliacion { get; set; }
    public string Comentario { get; set; } = string.Empty;
    
    // Movimiento Bancario
    public MovimientoBancarioSimpleDto MovimientoBancario { get; set; } = null!;
    
    // Alumno (opcional)
    public AlumnoSimpleDto? Alumno { get; set; }
    
    // Factura (opcional)
    public FacturaSimpleDto? Factura { get; set; }
}

public class MovimientoBancarioSimpleDto
{
    public Guid Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public decimal Saldo { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
}

public class AlumnoSimpleDto
{
    public Guid Id { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Apellido { get; set; } = string.Empty;
    public string? Email { get; set; }
}

public class FacturaSimpleDto
{
    public Guid Id { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public DateTime FechaEmision { get; set; }
    public DateTime FechaVencimiento { get; set; }
    public string Estado { get; set; } = string.Empty;
}
