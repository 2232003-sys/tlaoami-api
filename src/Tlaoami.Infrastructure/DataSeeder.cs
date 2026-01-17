using System;
using System.Linq;
using System.Threading.Tasks;
using Tlaoami.Domain.Entities;

namespace Tlaoami.Infrastructure
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(TlaoamiDbContext context)
        {
            if (context.Alumnos.Any())
            {
                return; // DB has been seeded
            }

            var alumno1 = new Alumno
            {
                Id = Guid.NewGuid(),
                Nombre = "Juan",
                Apellido = "Pérez",
                Email = "juan.perez@example.com",
                FechaInscripcion = DateTime.UtcNow.AddMonths(-6)
            };

            var alumno2 = new Alumno
            {
                Id = Guid.NewGuid(),
                Nombre = "María",
                Apellido = "García",
                Email = "maria.garcia@example.com",
                FechaInscripcion = DateTime.UtcNow.AddMonths(-12)
            };

            var factura1 = new Factura
            {
                Id = Guid.NewGuid(),
                AlumnoId = alumno1.Id,
                NumeroFactura = "F001",
                Monto = 100.00m,
                FechaEmision = DateTime.UtcNow.AddDays(-30),
                FechaVencimiento = DateTime.UtcNow.AddDays(-15),
                Estado = EstadoFactura.Pagada
            };

            var pago1 = new Pago
            {
                Id = Guid.NewGuid(),
                FacturaId = factura1.Id,
                Monto = 100.00m,
                FechaPago = DateTime.UtcNow.AddDays(-20),
                Metodo = MetodoPago.Tarjeta
            };

            var factura2 = new Factura
            {
                Id = Guid.NewGuid(),
                AlumnoId = alumno1.Id,
                NumeroFactura = "F002",
                Monto = 75.50m,
                FechaEmision = DateTime.UtcNow.AddDays(-10),
                FechaVencimiento = DateTime.UtcNow.AddDays(5),
                Estado = EstadoFactura.Pendiente
            };

            var factura3 = new Factura
            {
                Id = Guid.NewGuid(),
                AlumnoId = alumno2.Id,
                NumeroFactura = "F003",
                Monto = 250.00m,
                FechaEmision = DateTime.UtcNow.AddDays(-60),
                FechaVencimiento = DateTime.UtcNow.AddDays(-45),
                Estado = EstadoFactura.Vencida
            };

            factura1.Pagos.Add(pago1);

            context.Alumnos.AddRange(alumno1, alumno2);
            context.Facturas.AddRange(factura1, factura2, factura3);
            context.Pagos.Add(pago1);

            await context.SaveChangesAsync();
        }
    }
}
