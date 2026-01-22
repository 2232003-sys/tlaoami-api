using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using Tlaoami.Application.Dtos.Facturacion;
using Tlaoami.Application.Facturacion;
using Xunit;

namespace Tlaoami.Tests.Facturacion;

public class FacturamaPayloadTests
{
    [Fact]
    public void BuildBasicAuthHeader_DoesNotThrow()
    {
        var inMemory = new System.Collections.Generic.Dictionary<string, string?>
        {
            ["Facturama:BaseUrl"] = "https://apisandbox.facturama.mx",
            ["Facturama:User"] = "user",
            ["Facturama:Password"] = "pass"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(inMemory!).Build();

        var httpFactory = new Mock<IHttpClientFactory>();
        httpFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(new HttpClient());

        var client = new FacturamaClient(config, httpFactory.Object);
        // No assertion on network; just ensure instance builds without exception.
        Assert.NotNull(client);
    }
}
