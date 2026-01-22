using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Tlaoami.Application.Configuration;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Services;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain.Enums;
using Tlaoami.Infrastructure;

namespace Tlaoami.Tests.Facturacion;

public class FacturaFiscalServiceTests
{
    [Fact]
    public async Task TimbrarAsync_ReturnsExisting_WhenAlreadyTimbrado()
    {
        var options = CreateOptions();
        using var context = new TlaoamiDbContext(options);
        var alumnoId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();

        var alumno = new Alumno { Id = alumnoId, Matricula = "MAT1", Nombre = "Juan", Apellido = "Perez", FechaInscripcion = DateTime.UtcNow };
        var factura = new Factura
        {
            Id = facturaId,
            AlumnoId = alumnoId,
            Alumno = alumno,
            NumeroFactura = "F-1",
            Concepto = "Colegiatura",
            Monto = 1000m,
            FechaEmision = DateTime.UtcNow,
            FechaVencimiento = DateTime.UtcNow.AddDays(5),
            Estado = EstadoFactura.Pendiente
        };
        var fiscal = new FacturaFiscal
        {
            FacturaId = facturaId,
            Proveedor = "Dummy",
            EstadoTimbrado = "Timbrado",
            CfdiUuid = "UUID-123",
            CfdiXmlBase64 = "xml",
            CfdiPdfBase64 = "pdf",
            TimbradoAtUtc = DateTime.UtcNow
        };

        context.Alumnos.Add(alumno);
        context.Facturas.Add(factura);
        context.FacturasFiscales.Add(fiscal);
        context.SaveChanges();

        var cfdiProvider = new CountingCfdiProvider();
        var receptorService = new ReceptorFiscalService(context);
        var service = new FacturaFiscalService(context, cfdiProvider, receptorService, DefaultEmisor());

        var result = await service.TimbrarAsync(facturaId, new TimbrarCfdiRequest());

        Assert.Equal("UUID-123", result.CfdiUuid);
        Assert.Equal(0, cfdiProvider.CallCount);
    }

    [Fact]
    public async Task TimbrarAsync_ThrowsWhenReceptorMissing()
    {
        var options = CreateOptions();
        using var context = new TlaoamiDbContext(options);
        var alumnoId = Guid.NewGuid();
        var facturaId = Guid.NewGuid();

        context.Alumnos.Add(new Alumno { Id = alumnoId, Matricula = "MAT2", Nombre = "Ana", Apellido = "Lopez", FechaInscripcion = DateTime.UtcNow });
        context.Facturas.Add(new Factura
        {
            Id = facturaId,
            AlumnoId = alumnoId,
            NumeroFactura = "F-2",
            Concepto = "Inscripcion",
            Monto = 500m,
            FechaEmision = DateTime.UtcNow,
            FechaVencimiento = DateTime.UtcNow.AddDays(3),
            Estado = EstadoFactura.Pendiente
        });
        context.SaveChanges();

        var cfdiProvider = new CountingCfdiProvider();
        var receptorService = new ReceptorFiscalService(context);
        var service = new FacturaFiscalService(context, cfdiProvider, receptorService, DefaultEmisor());

        var ex = await Assert.ThrowsAsync<BusinessException>(() => service.TimbrarAsync(facturaId, new TimbrarCfdiRequest()));
        Assert.Equal("RECEPTOR_FISCAL_FALTANTE", ex.Code);
        Assert.Equal(0, cfdiProvider.CallCount);
    }

    [Fact]
    public async Task UpsertReceptorFiscal_IsIdempotentAndValidatesCp()
    {
        var options = CreateOptions();
        using var context = new TlaoamiDbContext(options);
        var alumnoId = Guid.NewGuid();
        context.Alumnos.Add(new Alumno { Id = alumnoId, Matricula = "MAT3", Nombre = "Luis", Apellido = "Rojas", FechaInscripcion = DateTime.UtcNow });
        context.SaveChanges();

        var receptorService = new ReceptorFiscalService(context);
        var dto = new ReceptorFiscalUpsertDto
        {
            Rfc = "ROJL800101ABC",
            NombreFiscal = "Luis Rojas",
            CodigoPostalFiscal = "01234",
            RegimenFiscal = "601",
            UsoCfdiDefault = "P01",
            Email = "test@example.com"
        };

        var first = await receptorService.UpsertAsync(alumnoId, dto);

        // Update with different data, should reuse Id
        dto.Email = "otro@example.com";
        dto.CodigoPostalFiscal = "56789";
        var second = await receptorService.UpsertAsync(alumnoId, dto);

        Assert.Equal(first.Id, second.Id);
        Assert.Equal("56789", second.CodigoPostalFiscal);
        Assert.Equal("otro@example.com", second.Email);

        // Invalid CP triggers BusinessException
        dto.CodigoPostalFiscal = "12A";
        await Assert.ThrowsAsync<BusinessException>(() => receptorService.UpsertAsync(alumnoId, dto));
    }

    private static IOptions<EmisorFiscalOptions> DefaultEmisor() => Options.Create(new EmisorFiscalOptions
    {
        Rfc = "EMI123456ABC",
        Nombre = "Emisor Pruebas",
        Regimen = "601"
    });

    private static DbContextOptions<TlaoamiDbContext> CreateOptions()
    {
        return new DbContextOptionsBuilder<TlaoamiDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }

    private class CountingCfdiProvider : ICfdiProvider
    {
        public int CallCount { get; private set; }

        public Task<CfdiResult> TimbrarAsync(CfdiRequest request)
        {
            CallCount++;
            return Task.FromResult(new CfdiResult
            {
                Exitoso = true,
                CfdiUuid = "NEW-UUID",
                CfdiXmlBase64 = "xml",
                CfdiPdfBase64 = "pdf",
                TimbradoAtUtc = DateTime.UtcNow
            });
        }
    }
}
