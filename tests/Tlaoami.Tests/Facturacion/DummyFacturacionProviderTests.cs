using System;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos.Facturacion;
using Tlaoami.Application.Facturacion;
using Xunit;

namespace Tlaoami.Tests.Facturacion;

public class DummyFacturacionProviderTests
{
    [Fact]
    public async Task EmitirCfdi_ReturnsUuidAndBase64()
    {
        IFacturacionProvider provider = new DummyFacturacionProvider();
        var req = new EmitirCfdiRequest(
            FacturaId: Guid.NewGuid(),
            Rfc: "XAXX010101000",
            Nombre: "PUBLICO GENERAL",
            CodigoPostal: "01000",
            RegimenFiscal: "601",
            UsoCfdi: "G03",
            Monto: 123.45m,
            Concepto: "Servicios"
        );

        var res = await provider.EmitirAsync(req);
        Assert.Equal("Dummy", res.Provider);
        Assert.False(string.IsNullOrWhiteSpace(res.Uuid));
        Assert.False(string.IsNullOrWhiteSpace(res.PdfBase64));
        Assert.False(string.IsNullOrWhiteSpace(res.XmlBase64));
    }
}
