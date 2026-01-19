namespace Tlaoami.Application.Dtos.PagosOnline;

public class WebhookSimuladoDto
{
    public string Estado { get; set; } = string.Empty; // "pagado" o "cancelado"
    public string? Comentario { get; set; }
}
