namespace Tlaoami.Application.Dtos.Facturacion;

public sealed record EmitirCfdiResult(
    string? Uuid,
    string? Serie,
    string? Folio,
    string? CfdiId = null,
    string? Provider = null,
    string? PdfBase64 = null,
    string? XmlBase64 = null,
    DateTime? IssuedAt = null
);
