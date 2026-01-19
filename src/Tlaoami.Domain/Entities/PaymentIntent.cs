namespace Tlaoami.Domain.Entities;

public class PaymentIntent
{
    public Guid Id { get; set; }
    public Guid FacturaId { get; set; }

    public decimal Monto { get; set; }

    public MetodoPagoIntent Metodo { get; set; }
    public EstadoPagoIntent Estado { get; set; }

    public string? Proveedor { get; set; }
    public string? ProveedorReferencia { get; set; }

    public string? ReferenciaSpei { get; set; }
    public string? ClabeDestino { get; set; }
    public DateTime? ExpiraEnUtc { get; set; }

    public DateTime CreadoEnUtc { get; set; }
    public DateTime ActualizadoEnUtc { get; set; }
}
