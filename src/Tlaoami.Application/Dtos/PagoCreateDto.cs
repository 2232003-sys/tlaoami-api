using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace Tlaoami.Application.Dtos;

public class PagoCreateDto
{
    [Required]
    public Guid FacturaId { get; set; }

    [Required]
    [StringLength(128, MinimumLength = 8)]
    public string IdempotencyKey { get; set; } = string.Empty;

    public decimal Monto { get; set; }

    [JsonPropertyName("fecha")]
    public DateTime FechaPago { get; set; }

    public string Metodo { get; set; } = "Efectivo";
}
