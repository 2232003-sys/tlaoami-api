using System;
using System.Threading;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos.Facturacion;

namespace Tlaoami.Application.Facturacion;

public sealed class DummyFacturacionProvider : IFacturacionProvider
{
    public Task<EmitirCfdiResult> EmitirAsync(EmitirCfdiRequest request, CancellationToken cancellationToken = default)
    {
        var uuid = Guid.NewGuid().ToString();
        var result = new EmitirCfdiResult(
            Uuid: uuid,
            Serie: "A",
            Folio: "123",
            CfdiId: uuid,
            Provider: "Dummy",
            PdfBase64: Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("PDF_PLACEHOLDER")),
            XmlBase64: Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes("<xml>PLACEHOLDER</xml>")),
            IssuedAt: DateTime.UtcNow
        );
        return Task.FromResult(result);
    }
}
