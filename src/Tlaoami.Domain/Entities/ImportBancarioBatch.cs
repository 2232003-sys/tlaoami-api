namespace Tlaoami.Domain.Entities;

/// <summary>
/// Lote de importación de movimientos bancarios (para auditoría y rollback)
/// </summary>
public class ImportBancarioBatch
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EscuelaId { get; set; }
    public Guid CuentaBancariaId { get; set; }

    /// <summary>
    /// Nombre del archivo importado (ej: "AFRIME_2026-01.csv")
    /// </summary>
    public string NombreArchivo { get; set; } = string.Empty;

    /// <summary>
    /// Período de inicio (si viene en el archivo)
    /// </summary>
    public DateTime? PeriodoInicio { get; set; }

    /// <summary>
    /// Período fin
    /// </summary>
    public DateTime? PeriodoFin { get; set; }

    /// <summary>
    /// Total de movimientos en el batch
    /// </summary>
    public int TotalMovimientos { get; set; }

    /// <summary>
    /// Total de abonos (ingresos)
    /// </summary>
    public int TotalAbonos { get; set; }

    /// <summary>
    /// Total de cargos (egresos)
    /// </summary>
    public int TotalCargos { get; set; }

    public Guid? CreadoPorUsuarioId { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public CuentaBancaria? CuentaBancaria { get; set; }
    public ICollection<MovimientoBancario> Movimientos { get; set; } = new List<MovimientoBancario>();
}
