using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos.Facturacion;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Facturacion;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Application.Services;

public class FacturacionService
{
    private readonly IFacturacionProvider _provider;
    private readonly TlaoamiDbContext _db;

    public FacturacionService(IFacturacionProvider provider, TlaoamiDbContext db)
    {
        _provider = provider;
        _db = db;
    }

    public async Task<EmitirCfdiResult> EmitirCfdiParaFacturaAsync(Guid facturaId, EmitirCfdiRequest request, CancellationToken ct = default)
    {
        var factura = await _db.Facturas.Include(f => f.Alumno).FirstOrDefaultAsync(f => f.Id == facturaId, ct);
        if (factura == null)
        {
            throw new BusinessException("FacturaNotFound", $"Factura {facturaId} no encontrada");
        }

        // Validación de datos fiscales mínimos
        var faltantes = new System.Collections.Generic.List<string>();
        if (string.IsNullOrWhiteSpace(request.Rfc)) faltantes.Add("Rfc");
        if (string.IsNullOrWhiteSpace(request.Nombre)) faltantes.Add("Nombre");
        if (string.IsNullOrWhiteSpace(request.CodigoPostal)) faltantes.Add("CodigoPostal");
        if (string.IsNullOrWhiteSpace(request.RegimenFiscal)) faltantes.Add("RegimenFiscal");
        if (string.IsNullOrWhiteSpace(request.UsoCfdi)) faltantes.Add("UsoCfdi");
        if (faltantes.Any())
        {
            throw new BusinessException("DatosFiscalesIncompletos", $"Faltan datos fiscales: {string.Join(", ", faltantes)}");
        }

        // Completar datos del request con info de la factura si faltan
        var req = request with { FacturaId = facturaId, Monto = request.Monto ?? factura.Monto, Concepto = request.Concepto ?? factura.Concepto };

        var resultado = await _provider.EmitirAsync(req, ct);

        // Persistir mínimos disponibles sin modificar el esquema (IssuedAt existente)
        factura.IssuedAt = resultado.IssuedAt;
        // Nota: La persistencia de Uuid/ProviderRef requiere columnas. Se hará en una migración posterior.
        await _db.SaveChangesAsync(ct);

        return resultado;
    }
}
