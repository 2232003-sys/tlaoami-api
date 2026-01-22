namespace Tlaoami.Application.Dtos.Facturacion;

public sealed record EmitirCfdiRequest(
    Guid FacturaId,
    string? Rfc,
    string? Nombre,
    string? CodigoPostal,
    string? RegimenFiscal,
    string? UsoCfdi,
    decimal? Monto,
    string? Concepto
);
