namespace Tlaoami.Application.Dtos.PagosOnline;

public class PaymentIntentDto
{
    public Guid Id { get; set; }
    public Guid FacturaId { get; set; }
    public decimal Monto { get; set; }
    public string Metodo { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;

    public string? Proveedor { get; set; }
    public string? ProveedorReferencia { get; set; }

    public string? ReferenciaSpei { get; set; }
    public string? ClabeDestino { get; set; }
    public DateTime? ExpiraEnUtc { get; set; }

    public DateTime CreadoEnUtc { get; set; }
}
