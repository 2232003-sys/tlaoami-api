using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Tlaoami.Application.Dtos.Facturacion;

namespace Tlaoami.Application.Facturacion;

public class FacturamaClient : IFacturacionProvider
{
    private readonly HttpClient _http;
    private readonly string _baseUrl;

    public FacturamaClient(IConfiguration config, IHttpClientFactory httpClientFactory)
    {
        _http = httpClientFactory.CreateClient(nameof(FacturamaClient));
        _baseUrl = config["Facturama:BaseUrl"] ?? "https://apisandbox.facturama.mx";

        var user = config["Facturama:User"] ?? string.Empty;
        var password = config["Facturama:Password"] ?? string.Empty;
        var token = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{user}:{password}"));
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", token);
    }

    public async Task<EmitirCfdiResult> EmitirAsync(EmitirCfdiRequest request, CancellationToken cancellationToken = default)
    {
        var payload = new
        {
            Receptor = new { Rfc = request.Rfc, Nombre = request.Nombre, CodigoPostal = request.CodigoPostal, RegimenFiscal = request.RegimenFiscal, UsoCfdi = request.UsoCfdi },
            Conceptos = new[] { new { ClaveProdServ = "01010101", Cantidad = 1, ClaveUnidad = "ACT", Descripcion = request.Concepto ?? "Servicios escolares", ValorUnitario = request.Monto ?? 0m, Importe = request.Monto ?? 0m } },
            MetodoPago = "PUE",
            FormaPago = "03",
            Serie = "A",
            Folio = DateTime.UtcNow.ToString("yyyyMMddHHmmss")
        };

        var url = _baseUrl.TrimEnd('/') + "/api-lite/cfdis";
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var resp = await _http.PostAsync(url, content, cancellationToken);
        resp.EnsureSuccessStatusCode();

        using var stream = await resp.Content.ReadAsStreamAsync(cancellationToken);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        var root = doc.RootElement;
        var cfdiId = root.TryGetProperty("Id", out var idEl) ? idEl.GetString() : null;
        var uuid = root.TryGetProperty("Uuid", out var uuidEl) ? uuidEl.GetString() : null;

        string? pdfB64 = null, xmlB64 = null;
        if (!string.IsNullOrEmpty(cfdiId))
        {
            var pdfUrl = _baseUrl.TrimEnd('/') + $"/api-lite/cfdis/{cfdiId}/pdf";
            var pdfResp = await _http.GetAsync(pdfUrl, cancellationToken);
            if (pdfResp.IsSuccessStatusCode)
            {
                var pdfBytes = await pdfResp.Content.ReadAsByteArrayAsync(cancellationToken);
                pdfB64 = Convert.ToBase64String(pdfBytes);
            }

            var xmlUrl = _baseUrl.TrimEnd('/') + $"/api-lite/cfdis/{cfdiId}/xml";
            var xmlResp = await _http.GetAsync(xmlUrl, cancellationToken);
            if (xmlResp.IsSuccessStatusCode)
            {
                var xmlBytes = await xmlResp.Content.ReadAsByteArrayAsync(cancellationToken);
                xmlB64 = Convert.ToBase64String(xmlBytes);
            }
        }

        return new EmitirCfdiResult(
            Uuid: uuid,
            Serie: "A",
            Folio: DateTime.UtcNow.ToString("yyyyMMddHHmmss"),
            CfdiId: cfdiId,
            Provider: "Facturama",
            PdfBase64: pdfB64,
            XmlBase64: xmlB64,
            IssuedAt: DateTime.UtcNow
        );
    }
}
