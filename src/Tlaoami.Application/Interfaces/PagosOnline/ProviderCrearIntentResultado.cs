namespace Tlaoami.Application.Interfaces.PagosOnline;

public record ProviderCrearIntentResultado(
    string? ProveedorReferencia,
    string? ReferenciaSpei,
    string? ClabeDestino,
    DateTime? ExpiraEnUtc
);
