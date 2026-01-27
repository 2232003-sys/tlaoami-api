using System.ComponentModel.DataAnnotations.Schema;
using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Entities;

/// <summary>
/// Movimiento bancario importado (una línea del estado de cuenta de Afrime)
/// </summary>
public class MovimientoBancario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EscuelaId { get; set; }
    public Guid CuentaBancariaId { get; set; }
    public Guid? ImportBatchId { get; set; }

    /// <summary>
    /// Fecha del movimiento (columna real)
    /// </summary>
    public DateTime Fecha { get; set; }

    /// <summary>
    /// Alias para compatibilidad Afrime (no mapea nueva columna)
    /// </summary>
    [NotMapped]
    public DateTime FechaMovimiento
    {
        get => Fecha;
        set => Fecha = value;
    }

    /// <summary>
    /// Monto absoluto
    /// </summary>
    public decimal Monto { get; set; }

    /// <summary>
    /// Saldo posterior al movimiento (legacy)
    /// </summary>
    public decimal Saldo { get; set; }

    /// <summary>
    /// Abono (ingreso) o Cargo (egreso) - usa TipoMovimiento legacy
    /// </summary>
    public TipoMovimiento Tipo { get; set; }

    /// <summary>
    /// Descripción del banco (ej: "TRANSFERENCIA RECIBIDA", "PAGO CON TARJETA", etc.)
    /// </summary>
    public string Descripcion { get; set; } = string.Empty;

    /// <summary>
    /// Referencia bancaria (si existe)
    /// </summary>
    public string? ReferenciaBanco { get; set; }

    /// <summary>
    /// Folio o número de movimiento del banco
    /// </summary>
    public string? Folio { get; set; }

    /// <summary>
    /// Hash único para deduplicación: SHA256(Fecha|Monto|Descripcion|CuentaBancariaId|Folio)
    /// </summary>
    public string? HashUnico { get; set; }

    /// <summary>
    /// Alias legacy para compatibilidad
    /// </summary>
    [NotMapped]
    public string? HashMovimiento
    {
        get => HashUnico;
        set => HashUnico = value;
    }

    /// <summary>
    /// Estado legacy
    /// </summary>
    public EstadoConciliacion Estado { get; set; } = EstadoConciliacion.NoConciliado;

    /// <summary>
    /// Alias no mapeado para el nuevo flujo (no usar en LINQ EF)
    /// </summary>
    [NotMapped]
    public EstatusMovimientoBancario Estatus
    {
        get => MapEstadoToEstatus(Estado);
        set => Estado = MapEstatusToEstado(value);
    }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public CuentaBancaria? CuentaBancaria { get; set; }
    public ImportBancarioBatch? ImportBatch { get; set; }
    public ICollection<ConciliacionMatch> Matches { get; set; } = new List<ConciliacionMatch>();

    private static EstatusMovimientoBancario MapEstadoToEstatus(EstadoConciliacion estado) => estado switch
    {
        EstadoConciliacion.NoConciliado => EstatusMovimientoBancario.Nuevo,
        EstadoConciliacion.Conciliado => EstatusMovimientoBancario.Conciliado,
        EstadoConciliacion.Ignorado => EstatusMovimientoBancario.Ignorado,
        _ => EstatusMovimientoBancario.Nuevo,
    };

    private static EstadoConciliacion MapEstatusToEstado(EstatusMovimientoBancario estatus) => estatus switch
    {
        EstatusMovimientoBancario.Nuevo => EstadoConciliacion.NoConciliado,
        EstatusMovimientoBancario.MatchPropuesto => EstadoConciliacion.NoConciliado,
        EstatusMovimientoBancario.Conciliado => EstadoConciliacion.Conciliado,
        EstatusMovimientoBancario.Ignorado => EstadoConciliacion.Ignorado,
        _ => EstadoConciliacion.NoConciliado,
    };
}
