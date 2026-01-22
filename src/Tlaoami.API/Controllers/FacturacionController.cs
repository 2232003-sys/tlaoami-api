using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Tlaoami.Application.Dtos.Facturacion;
using Tlaoami.Application.Services;

namespace Tlaoami.API.Controllers;

[ApiController]
[Route("api/v1/facturacion")]
public class FacturacionController : ControllerBase
{
    private readonly FacturacionService _service;

    public FacturacionController(FacturacionService service)
    {
        _service = service;
    }

    [HttpPost("facturas/{facturaId}/emitir-cfdi")]
    public async Task<ActionResult<EmitirCfdiResult>> Emitir(Guid facturaId, [FromBody] EmitirCfdiRequest request, CancellationToken ct)
    {
        var result = await _service.EmitirCfdiParaFacturaAsync(facturaId, request, ct);
        return Ok(result);
    }

    // Endpoints de descarga de PDF/XML se delegan al proveedor; si se tienen ids se podrían redirigir.
    // Por simplicidad devolvemos directamente el base64 del último CFDI emitido usando el request.
    [HttpPost("facturas/{facturaId}/descargar-pdf")]
    public async Task<ActionResult> DescargarPdf(Guid facturaId, [FromBody] EmitirCfdiRequest request, CancellationToken ct)
    {
        var result = await _service.EmitirCfdiParaFacturaAsync(facturaId, request, ct);
        if (string.IsNullOrEmpty(result.PdfBase64)) return NotFound();
        return Ok(new { pdfBase64 = result.PdfBase64 });
    }

    [HttpPost("facturas/{facturaId}/descargar-xml")]
    public async Task<ActionResult> DescargarXml(Guid facturaId, [FromBody] EmitirCfdiRequest request, CancellationToken ct)
    {
        var result = await _service.EmitirCfdiParaFacturaAsync(facturaId, request, ct);
        if (string.IsNullOrEmpty(result.XmlBase64)) return NotFound();
        return Ok(new { xmlBase64 = result.XmlBase64 });
    }
}
