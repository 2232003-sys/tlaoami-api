using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Contracts;

public class MovimientoBancarioDto
{
    public Guid Id { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; }
    public decimal Monto { get; set; }
    public TipoMovimiento Tipo { get; set; }
    public EstadoConciliacion Estado { get; set; }
}
