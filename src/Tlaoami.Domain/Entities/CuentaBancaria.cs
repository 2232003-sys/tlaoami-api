using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Entities;

/// <summary>
/// Cuenta bancaria (Afrime o proveedor)
/// </summary>
public class CuentaBancaria
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid EscuelaId { get; set; }
    
    /// <summary>
    /// Banco (ej: "Afrime", "Banorte", etc.)
    /// </summary>
    public string Banco { get; set; } = string.Empty;
    
    /// <summary>
    /// Alias de la cuenta (ej: "Cuenta Principal")
    /// </summary>
    public string Alias { get; set; } = string.Empty;
    
    /// <summary>
    /// Últimos 4 dígitos de la cuenta
    /// </summary>
    public string Ultimos4 { get; set; } = string.Empty;
    
    /// <summary>
    /// CLABE (18 dígitos), nullable
    /// </summary>
    public string? Clabe { get; set; }
    
    /// <summary>
    /// ¿Está activa para importar movimientos?
    /// </summary>
    public bool Activa { get; set; } = true;
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation
    public ICollection<ImportBancarioBatch> ImportBatches { get; set; } = new List<ImportBancarioBatch>();
    public ICollection<MovimientoBancario> Movimientos { get; set; } = new List<MovimientoBancario>();
}
