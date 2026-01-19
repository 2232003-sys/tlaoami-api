namespace Tlaoami.Application.Dtos;

public class SugerenciaConciliacionDto
{
    public Guid AlumnoId { get; set; }
    public string NombreAlumno { get; set; } = string.Empty;
    public string EmailAlumno { get; set; } = string.Empty;
    
    public Guid? FacturaId { get; set; }
    public string? NumeroFactura { get; set; }
    public decimal? MontoFactura { get; set; }
    
    public decimal Similitud { get; set; }
    public string Razon { get; set; } = string.Empty;
}
