namespace Tlaoami.Domain.Entities;

public class MovimientoBancario
{
    public Guid Id { get; set; }

    public DateTime Fecha { get; set; }

    public string Descripcion { get; set; } = string.Empty;

    public decimal Monto { get; set; }

    public decimal Saldo { get; set; }

    public TipoMovimiento Tipo { get; set; }

    public EstadoConciliacion Estado { get; set; }

    public string HashMovimiento { get; set; } = string.Empty;
}
