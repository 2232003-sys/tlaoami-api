using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Tlaoami.Application.Rules;
using Tlaoami.Application.Services;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain.Enums;
using Tlaoami.Infrastructure;
using Xunit;

namespace Tlaoami.Application.Tests;

public class ConciliacionBancariaServiceTests
{
    private TlaoamiDbContext CreateTestContext()
    {
        var options = new DbContextOptionsBuilder<TlaoamiDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new TlaoamiDbContext(options);
    }

    private ConciliacionBancariaService CreateService(TlaoamiDbContext context)
    {
        var mockLogger = new Mock<ILogger<ConciliacionBancariaService>>();
        var reglas = new List<IConciliacionRule>
        {
            new MontoMultiploCientoRule(),
            new FechaDiaEspecialRule(),
            new MontoRangoTipicoRule()
        };
        var matchRules = new List<IMatchRule>
        {
            new MontoMatchRule(),
            new FechaMatchRule(),
            new ReferenciaMatchRule()
        };
        return new ConciliacionBancariaService(context, mockLogger.Object, reglas, matchRules);
    }

    private Alumno CrearAlumno(TlaoamiDbContext context)
    {
        var alumno = new Alumno
        {
            Id = Guid.NewGuid(),
            Matricula = $"A{new Random().Next(1000, 9999)}",
            Nombre = "Juan",
            Apellido = "Pérez",
            Email = "juan@example.com",
            Telefono = "1234567890",
            Activo = true,
            FechaInscripcion = DateTime.UtcNow
        };
        context.Alumnos.Add(alumno);
        context.SaveChanges();
        return alumno;
    }

    private Factura CrearFactura(TlaoamiDbContext context, Guid alumnoId, decimal monto, 
        DateTime? fechaVencimiento = null, EstadoFactura estado = EstadoFactura.Pendiente)
    {
        var factura = new Factura
        {
            Id = Guid.NewGuid(),
            AlumnoId = alumnoId,
            NumeroFactura = $"F{Guid.NewGuid().ToString().Substring(0, 8)}",
            Concepto = "Colegiatura",
            Periodo = "2026-01",
            Monto = monto,
            FechaEmision = DateTime.UtcNow,
            FechaVencimiento = fechaVencimiento ?? DateTime.UtcNow.AddDays(30),
            Estado = estado,
            IssuedAt = DateTime.UtcNow,
            Pagos = new List<Pago>(),
            Lineas = new List<FacturaLinea>()
        };
        context.Facturas.Add(factura);
        context.SaveChanges();
        return factura;
    }

    private MovimientoBancario CrearMovimiento(TlaoamiDbContext context, decimal monto,
        EstadoConciliacion estado = EstadoConciliacion.NoConciliado)
    {
        var movimiento = new MovimientoBancario
        {
            Id = Guid.NewGuid(),
            Monto = monto,
            Fecha = DateTime.UtcNow,
            Descripcion = "Depósito bancario",
            Tipo = TipoMovimiento.Deposito,
            Estado = estado,
            Saldo = monto,
            HashMovimiento = Guid.NewGuid().ToString()
        };
        context.MovimientosBancarios.Add(movimiento);
        context.SaveChanges();
        return movimiento;
    }

    [Fact]
    public async Task AplicarAbono_ACuenta_FIFO_PorFechaVencimiento()
    {
        // Arrange
        var context = CreateTestContext();
        var service = CreateService(context);
        var alumno = CrearAlumno(context);

        // Crear 3 facturas: vencida, próxima a vencer, futura
        var facturaVencida = CrearFactura(context, alumno.Id, 1000m,
            DateTime.UtcNow.AddDays(-10)); // Vencida
        var facturaPorVencer = CrearFactura(context, alumno.Id, 500m,
            DateTime.UtcNow.AddDays(5)); // Próxima a vencer
        var facturaFutura = CrearFactura(context, alumno.Id, 800m,
            DateTime.UtcNow.AddDays(30)); // Futura

        var movimiento = CrearMovimiento(context, 1200m);

        // Act
        await service.ConciliarMovimientoAsync(
            movimiento.Id,
            alumnoId: alumno.Id,
            facturaId: null,
            comentario: "Abono a cuenta",
            crearPago: true);

        // Assert - Factura vencida debe pagarse completamente primero
        context.Entry(facturaVencida).Reload();
        Assert.Equal(EstadoFactura.Pagada, facturaVencida.Estado);
        Assert.Equal(1000m, facturaVencida.Pagos.Sum(p => p.Monto));

        // Factura por vencer recibe el sobrante
        context.Entry(facturaPorVencer).Reload();
        Assert.Equal(EstadoFactura.ParcialmentePagada, facturaPorVencer.Estado);
        Assert.Equal(200m, facturaPorVencer.Pagos.Sum(p => p.Monto));

        // Factura futura sin pagos
        context.Entry(facturaFutura).Reload();
        Assert.Equal(EstadoFactura.Pendiente, facturaFutura.Estado);
        Assert.Empty(facturaFutura.Pagos);

        // Verificar que se crearon 2 pagos (uno por factura) + anticipo
        var pagos = context.Pagos.Where(p => p.AlumnoId == alumno.Id).ToList();
        Assert.Equal(2, pagos.Count); // F0 a vencida, F1 a por vencer (sin anticipo porque se consumió todo)
    }

    [Fact]
    public async Task AplicarAbono_Excedente_CreaAnticipo()
    {
        // Arrange
        var context = CreateTestContext();
        var service = CreateService(context);
        var alumno = CrearAlumno(context);

        var factura = CrearFactura(context, alumno.Id, 1000m);
        var movimiento = CrearMovimiento(context, 1500m); // 500m más

        // Act
        await service.ConciliarMovimientoAsync(
            movimiento.Id,
            alumnoId: alumno.Id,
            facturaId: null,
            comentario: "Abono excedente",
            crearPago: true);

        // Assert
        context.Entry(factura).Reload();
        Assert.Equal(EstadoFactura.Pagada, factura.Estado);

        var pagos = context.Pagos.Where(p => p.AlumnoId == alumno.Id).ToList();
        Assert.Equal(2, pagos.Count);

        // Verificar pago anticipado
        var pagoAnticipo = pagos.FirstOrDefault(p => p.FacturaId == null);
        Assert.NotNull(pagoAnticipo);
        Assert.Equal(500m, pagoAnticipo.Monto);
        Assert.EndsWith(":ANTICIPO", pagoAnticipo.IdempotencyKey);
    }

    [Fact]
    public async Task AplicarPago_Parcial_ActualizaEstado()
    {
        // Arrange
        var context = CreateTestContext();
        var service = CreateService(context);
        var alumno = CrearAlumno(context);

        var factura = CrearFactura(context, alumno.Id, 1000m);
        var movimiento = CrearMovimiento(context, 300m); // Pago parcial

        // Act
        await service.ConciliarMovimientoAsync(
            movimiento.Id,
            alumnoId: null,
            facturaId: factura.Id,
            comentario: "Pago parcial",
            crearPago: true);

        // Assert
        context.Entry(factura).Reload();
        Assert.Equal(EstadoFactura.ParcialmentePagada, factura.Estado);
        Assert.Equal(300m, factura.Pagos.Sum(p => p.Monto));
        Assert.Equal(700m, factura.Monto - factura.Pagos.Sum(p => p.Monto));
    }

    [Fact]
    public async Task AplicarAbono_Idempotencia_NoCreaDuplicados()
    {
        // Arrange
        var context = CreateTestContext();
        var service = CreateService(context);
        var alumno = CrearAlumno(context);

        var factura = CrearFactura(context, alumno.Id, 1000m);
        var movimiento = CrearMovimiento(context, 1000m);

        // Act - Aplicar dos veces el mismo movimiento
        await service.ConciliarMovimientoAsync(
            movimiento.Id,
            alumnoId: null,
            facturaId: factura.Id,
            comentario: "Intento 1",
            crearPago: true);

        await service.ConciliarMovimientoAsync(
            movimiento.Id,
            alumnoId: null,
            facturaId: factura.Id,
            comentario: "Intento 2",
            crearPago: true);

        // Assert - Solo debe existir un pago
        context.Entry(factura).Reload();
        Assert.Single(factura.Pagos);
        Assert.Equal(1000m, factura.Pagos.Sum(p => p.Monto));
    }

    [Fact]
    public async Task AplicarAbono_SinFacturasPendientes_LanzaExcepcion()
    {
        // Arrange
        var context = CreateTestContext();
        var service = CreateService(context);
        var alumno = CrearAlumno(context);

        // Alumno sin facturas
        var movimiento = CrearMovimiento(context, 1000m);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await service.ConciliarMovimientoAsync(
                movimiento.Id,
                alumnoId: alumno.Id,
                facturaId: null,
                comentario: "Sin facturas",
                crearPago: true);
        });

        Assert.Contains("No hay facturas pendientes", ex.Message);
    }

    [Fact]
    public async Task RevertirConciliacion_EliminaMultiplesPagos()
    {
        // Arrange
        var context = CreateTestContext();
        var service = CreateService(context);
        var alumno = CrearAlumno(context);

        var factura1 = CrearFactura(context, alumno.Id, 500m);
        var factura2 = CrearFactura(context, alumno.Id, 300m);
        var movimiento = CrearMovimiento(context, 800m);

        // Aplicar abono a cuenta
        await service.ConciliarMovimientoAsync(
            movimiento.Id,
            alumnoId: alumno.Id,
            facturaId: null,
            comentario: "Abono",
            crearPago: true);

        context.Entry(factura1).Reload();
        context.Entry(factura2).Reload();
        Assert.Equal(EstadoFactura.Pagada, factura1.Estado);
        Assert.Equal(EstadoFactura.ParcialmentePagada, factura2.Estado);

        // Act - Revertir
        await service.RevertirConciliacionAsync(movimiento.Id);

        // Assert
        context.Entry(factura1).Reload();
        context.Entry(factura2).Reload();
        Assert.Equal(EstadoFactura.Pendiente, factura1.Estado);
        Assert.Equal(EstadoFactura.Pendiente, factura2.Estado);

        var pagos = context.Pagos.Where(p => p.IdempotencyKey.StartsWith($"BANK:{movimiento.Id}")).ToList();
        Assert.Empty(pagos);
    }

    [Fact]
    public async Task AplicarAbono_IdempotenciaSequence_VerificaKeysUnicos()
    {
        // Arrange
        var context = CreateTestContext();
        var service = CreateService(context);
        var alumno = CrearAlumno(context);

        var factura1 = CrearFactura(context, alumno.Id, 500m);
        var factura2 = CrearFactura(context, alumno.Id, 300m);
        var factura3 = CrearFactura(context, alumno.Id, 200m);

        var movimiento = CrearMovimiento(context, 1000m);

        // Act
        await service.ConciliarMovimientoAsync(
            movimiento.Id,
            alumnoId: alumno.Id,
            facturaId: null,
            comentario: "Abono múltiple",
            crearPago: true);

        // Assert
        var pagos = context.Pagos
            .Where(p => p.IdempotencyKey.StartsWith($"BANK:{movimiento.Id}"))
            .OrderBy(p => p.IdempotencyKey)
            .ToList();

        Assert.NotEmpty(pagos);
        
        var keys = pagos.Select(p => p.IdempotencyKey).ToList();
        var uniqueKeys = keys.Distinct().ToList();
        
        // Todas las keys deben ser únicas
        Assert.Equal(keys.Count, uniqueKeys.Count);
        
        // Formato correcto de keys
        foreach (var key in keys)
        {
            Assert.True(key.StartsWith($"BANK:{movimiento.Id}"));
            Assert.True(key.Contains(":F") || key.Contains(":ANTICIPO"));
        }
    }

    [Fact]
    public async Task AplicarAbono_MontoExacto_SinAnticipo()
    {
        // Arrange
        var context = CreateTestContext();
        var service = CreateService(context);
        var alumno = CrearAlumno(context);

        var factura1 = CrearFactura(context, alumno.Id, 600m);
        var factura2 = CrearFactura(context, alumno.Id, 400m);

        var movimiento = CrearMovimiento(context, 1000m); // Exacto

        // Act
        await service.ConciliarMovimientoAsync(
            movimiento.Id,
            alumnoId: alumno.Id,
            facturaId: null,
            comentario: "Abono exacto",
            crearPago: true);

        // Assert
        var pagos = context.Pagos.Where(p => p.AlumnoId == alumno.Id).ToList();
        
        // No debe haber anticipo
        Assert.DoesNotContain(pagos, p => p.FacturaId == null);
        
        // Todas las facturas pagadas
        context.Entry(factura1).Reload();
        context.Entry(factura2).Reload();
        Assert.Equal(EstadoFactura.Pagada, factura1.Estado);
        Assert.Equal(EstadoFactura.Pagada, factura2.Estado);
    }
}
