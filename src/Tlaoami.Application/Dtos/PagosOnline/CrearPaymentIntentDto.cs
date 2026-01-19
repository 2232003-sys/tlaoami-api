using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Dtos.PagosOnline;

public class CrearPaymentIntentDto
{
    public Guid FacturaId { get; set; }
    public MetodoPagoIntent Metodo { get; set; }
}
