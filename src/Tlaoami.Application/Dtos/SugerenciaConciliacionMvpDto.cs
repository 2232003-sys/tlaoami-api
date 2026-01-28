namespace Tlaoami.Application.Dtos;

public class SugerenciaConciliacionMvpDto
{
    public Guid PagoId { get; set; }
    public Guid AlumnoId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public int Score { get; set; }
    public string ReglaMatch { get; set; } = string.Empty;
    public string? Referencia { get; set; }
}
