namespace Tlaoami.Application.Dtos;

public class ReportarPagoManualDto
{
    public Guid EscuelaId { get; set; }
    public Guid AlumnoId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string? Referencia { get; set; }
}
