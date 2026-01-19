namespace Tlaoami.Application.Dtos;

public class PagoCreateDto
{
    public Guid FacturaId { get; set; }
    public decimal Monto { get; set; }
    public DateTime FechaPago { get; set; }
    public string Metodo { get; set; } = "Efectivo";
}
