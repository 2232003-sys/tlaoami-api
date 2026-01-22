namespace Tlaoami.Application.Dtos;

public class SugerenciaConciliacionDto
{
    public Guid AlumnoId { get; set; }
    public string Matricula { get; set; } = string.Empty;
    public string NombreAlumno { get; set; } = string.Empty;
    public string EmailAlumno { get; set; } = string.Empty;
    
    public Guid? FacturaId { get; set; }
    public string? NumeroFactura { get; set; }
    public decimal? MontoFactura { get; set; }
    
    // False = factura exacta por monto; True = aplicar a cuenta por FIFO
    public bool AplicarACuenta { get; set; }
    
    // Confianza de la sugerencia (0-1)
    public decimal Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
}
