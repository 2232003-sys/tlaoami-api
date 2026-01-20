using System;
using System.Linq;
using System.Threading.Tasks;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain;

namespace Tlaoami.Infrastructure
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(TlaoamiDbContext context)
        {
            // Seed users if none exist
            if (!context.Users.Any())
            {
                var userAdmin = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin",
                    PasswordHash = "admin123", // Demo only - in production use BCrypt.HashPassword
                    Role = Roles.Admin,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                var userAdministrativo = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "admin1",
                    PasswordHash = "admin123",
                    Role = Roles.Administrativo,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                var userConsulta = new User
                {
                    Id = Guid.NewGuid(),
                    Username = "consulta",
                    PasswordHash = "consulta123",
                    Role = Roles.Consulta,
                    Activo = true,
                    FechaCreacion = DateTime.UtcNow
                };

                context.Users.AddRange(userAdmin, userAdministrativo, userConsulta);
                await context.SaveChangesAsync();
            }

            if (context.Alumnos.Any())
            {
                return; // DB has been seeded
            }

            var alumno1 = new Alumno
            {
                Id = Guid.NewGuid(),
                Matricula = "LEG-001",
                Nombre = "Juan",
                Apellido = "Pérez",
                Email = "juan.perez@example.com",
                Activo = true,
                FechaInscripcion = DateTime.UtcNow.AddMonths(-6)
            };

            var alumno2 = new Alumno
            {
                Id = Guid.NewGuid(),
                Matricula = "LEG-002",
                Nombre = "María",
                Apellido = "García",
                Email = "maria.garcia@example.com",
                Activo = true,
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
                Metodo = MetodoPago.Tarjeta,
                IdempotencyKey = Guid.NewGuid().ToString()
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
