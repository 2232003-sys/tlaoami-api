using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain.Enums;

namespace Tlaoami.Infrastructure
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(TlaoamiDbContext context)
        {
            // Ensure the database is created.
            await context.Database.MigrateAsync();

            // Check if there is already data.
            if (context.Alumnos.Any())
            {
                return; // DB has been seeded
            }

            var alumno1 = new Alumno
            {
                Id = Guid.NewGuid(),
                Nombre = "Carlos",
                Apellido = "Rodriguez",
                Email = "carlos.rodriguez@example.com",
                FechaNacimiento = new DateTime(1995, 5, 20),
                Facturas = new[]
                {
                    new Factura
                    {
                        Id = Guid.NewGuid(),
                        NumeroFactura = "F-2024-001",
                        Monto = 150.00m,
                        FechaEmision = DateTime.UtcNow.AddDays(-30),
                        FechaVencimiento = DateTime.UtcNow.AddDays(-15),
                        Estado = EstadoFactura.Pagada,
                        Pagos = new[]
                        {
                            new Pago
                            {
                                Id = Guid.NewGuid(),
                                Monto = 150.00m,
                                FechaPago = DateTime.UtcNow.AddDays(-20),
                                Metodo = MetodoPago.TarjetaCredito
                            }
                        }
                    }
                }
            };

            var alumno2 = new Alumno
            {
                Id = Guid.NewGuid(),
                Nombre = "Ana",
                Apellido = "Lopez",
                Email = "ana.lopez@example.com",
                FechaNacimiento = new DateTime(1998, 8, 12),
                Facturas = new[]
                {
                    new Factura
                    {
                        Id = Guid.NewGuid(),
                        NumeroFactura = "F-2024-002",
                        Monto = 300.50m,
                        FechaEmision = DateTime.UtcNow.AddDays(-10),
                        FechaVencimiento = DateTime.UtcNow.AddDays(5),
                        Estado = EstadoFactura.Pendiente
                    },
                    new Factura
                    {
                        Id = Guid.NewGuid(),
                        NumeroFactura = "F-2024-003",
                        Monto = 75.00m,
                        FechaEmision = DateTime.UtcNow.AddDays(-60),
                        FechaVencimiento = DateTime.UtcNow.AddDays(-45),
                        Estado = EstadoFactura.Vencida
                    }
                }
            };
            
            await context.Alumnos.AddRangeAsync(alumno1, alumno2);
            await context.SaveChangesAsync();
        }
    }
}
