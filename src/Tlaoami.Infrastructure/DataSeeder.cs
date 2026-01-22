using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain;
using Tlaoami.Domain.Enums;

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
                    PasswordHash = "admin123",
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

            // Check if 2026 ciclo already exists
            var ciclo2026Existing = context.CiclosEscolares.FirstOrDefault(c => c.Nombre == "2026");
            if (ciclo2026Existing != null)
            {
                return; // 2026 already seeded
            }

            // === CREATE CICLO ESCOLAR 2026 ===
            var ciclo2026 = new CicloEscolar
            {
                Id = Guid.NewGuid(),
                Nombre = "2026",
                FechaInicio = new DateTime(2026, 01, 19),
                FechaFin = new DateTime(2026, 12, 31),
                Activo = true
            };
            context.CiclosEscolares.Add(ciclo2026);
            await context.SaveChangesAsync();

            // === CREATE GRUPOS ===
            var grupos = new List<Grupo>
            {
                new Grupo { Id = Guid.NewGuid(), Nombre = "1A", Grado = 1, Turno = "Mañana", CicloEscolarId = ciclo2026.Id },
                new Grupo { Id = Guid.NewGuid(), Nombre = "1B", Grado = 1, Turno = "Mañana", CicloEscolarId = ciclo2026.Id },
                new Grupo { Id = Guid.NewGuid(), Nombre = "2A", Grado = 2, Turno = "Mañana", CicloEscolarId = ciclo2026.Id },
                new Grupo { Id = Guid.NewGuid(), Nombre = "2B", Grado = 2, Turno = "Tarde", CicloEscolarId = ciclo2026.Id },
                new Grupo { Id = Guid.NewGuid(), Nombre = "3A", Grado = 3, Turno = "Mañana", CicloEscolarId = ciclo2026.Id },
                new Grupo { Id = Guid.NewGuid(), Nombre = "3B", Grado = 3, Turno = "Tarde", CicloEscolarId = ciclo2026.Id },
            };
            context.Grupos.AddRange(grupos);
            await context.SaveChangesAsync();

            // === CREATE ALUMNOS (38 from CSV) ===
            var matriculas = new[] 
            { 
                "A1018", "A1046", "A1057", "A1064", "A1075", "A1076", "A1107", "A1109", 
                "A1113", "A1117", "A1121", "A1123", "A1126", "A1130", "A1131", "A1132", 
                "A1143", "A1145", "A1153", "A1157", "A1190", "A1200", "A1214", "A1221", 
                "A1237", "A1239", "A1246", "A1250", "A1251", "A1260", "A1262", "A1273", 
                "A1289", "A1293", "A1307", "A1320", "A1322", "A1341"
            };

            var nombres = new[] { "Juan", "María", "Carlos", "Ana", "Luis", "Elena", "Pedro", "Sofia", "Miguel", "Isabel", "Diego", "Laura", "Ricardo", "Beatriz", "Fernando", "Rosa", "Andrés", "Catalina", "Javier", "Valentina", "Roberto", "Mariana", "Alfonso", "Gabriela", "Sergio", "Vanessa", "Ramón", "Patricia", "Gustavo", "Mercedes", "Enrique", "Adriana", "Arturo", "Dolores", "Julio", "Nora", "Fabio", "Silvia" };
            var apellidos = new[] { "Pérez", "García", "López", "Hernández", "González", "Martínez", "Sánchez", "Rodríguez", "Torres", "Flores", "Morales", "Reyes", "Cruz", "Castillo", "Medina", "Jiménez", "Santos", "Ramírez", "Vargas", "Campos", "Ruiz", "Ortiz", "Herrera", "Domínguez", "Vega", "Sosa", "Aguilar", "Cortés", "Alvarado", "Rivera", "Delgado", "Guerrero", "Navarro", "Carrillo", "Camacho", "Calvo", "Benítez", "Fuentes" };

            var alumnos = new List<Alumno>();
            var random = new Random(42);

            foreach (var matricula in matriculas)
            {
                var nombre = nombres[random.Next(nombres.Length)];
                var apellido = apellidos[random.Next(apellidos.Length)];

                var alumno = new Alumno
                {
                    Id = Guid.NewGuid(),
                    Matricula = matricula,
                    Nombre = nombre,
                    Apellido = apellido,
                    Email = $"{nombre.ToLower()}.{apellido.ToLower()}@colegio.mx",
                    Telefono = $"55{random.Next(10000000, 99999999)}",
                    Activo = true,
                    FechaInscripcion = DateTime.UtcNow.AddMonths(-6)
                };
                alumnos.Add(alumno);
            }

            context.Alumnos.AddRange(alumnos);
            await context.SaveChangesAsync();

            // === ASSIGN ALUMNOS TO GRUPOS ===
            int grupoIndex = 0;
            foreach (var alumno in alumnos)
            {
                var grupo = grupos[grupoIndex % grupos.Count];
                var asignacion = new AlumnoGrupo
                {
                    Id = Guid.NewGuid(),
                    AlumnoId = alumno.Id,
                    GrupoId = grupo.Id,
                    FechaInicio = DateTime.UtcNow.AddMonths(-6),
                    Activo = true
                };
                context.AsignacionesGrupo.Add(asignacion);
                grupoIndex++;
            }
            await context.SaveChangesAsync();

            // === CREATE CONCEPTOS DE COBRO ===
            var conceptos = new List<ConceptoCobro>
            {
                new ConceptoCobro
                {
                    Id = Guid.NewGuid(),
                    Clave = "COLEGIATURA",
                    Nombre = "Colegiatura Mensual",
                    Periodicidad = Periodicidad.Mensual,
                    Activo = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new ConceptoCobro
                {
                    Id = Guid.NewGuid(),
                    Clave = "SERVICIOS",
                    Nombre = "Servicios Académicos",
                    Periodicidad = Periodicidad.Anual,
                    Activo = true,
                    CreatedAtUtc = DateTime.UtcNow
                },
                new ConceptoCobro
                {
                    Id = Guid.NewGuid(),
                    Clave = "RECARGO",
                    Nombre = "Recargo por Mora",
                    Periodicidad = Periodicidad.Mensual,
                    Activo = true,
                    CreatedAtUtc = DateTime.UtcNow
                }
            };
            context.ConceptosCobro.AddRange(conceptos);
            await context.SaveChangesAsync();

            var conceptoColegiatura = conceptos[0];
            var conceptoRecargo = conceptos[2];

            // === CREATE REGLAS DE COLEGIATURAS ===
            var reglasColegiaturas = new List<ReglaColegiatura>
            {
                new ReglaColegiatura { Id = Guid.NewGuid(), CicloId = ciclo2026.Id, GrupoId = grupos[0].Id, Grado = 1, Turno = "Mañana", ConceptoCobroId = conceptoColegiatura.Id, MontoBase = 5500 },
                new ReglaColegiatura { Id = Guid.NewGuid(), CicloId = ciclo2026.Id, GrupoId = grupos[1].Id, Grado = 1, Turno = "Mañana", ConceptoCobroId = conceptoColegiatura.Id, MontoBase = 5500 },
                new ReglaColegiatura { Id = Guid.NewGuid(), CicloId = ciclo2026.Id, GrupoId = grupos[2].Id, Grado = 2, Turno = "Mañana", ConceptoCobroId = conceptoColegiatura.Id, MontoBase = 5800 },
                new ReglaColegiatura { Id = Guid.NewGuid(), CicloId = ciclo2026.Id, GrupoId = grupos[3].Id, Grado = 2, Turno = "Tarde", ConceptoCobroId = conceptoColegiatura.Id, MontoBase = 4800 },
                new ReglaColegiatura { Id = Guid.NewGuid(), CicloId = ciclo2026.Id, GrupoId = grupos[4].Id, Grado = 3, Turno = "Mañana", ConceptoCobroId = conceptoColegiatura.Id, MontoBase = 6200 },
                new ReglaColegiatura { Id = Guid.NewGuid(), CicloId = ciclo2026.Id, GrupoId = grupos[5].Id, Grado = 3, Turno = "Tarde", ConceptoCobroId = conceptoColegiatura.Id, MontoBase = 5200 }
            };
            context.ReglasColegiatura.AddRange(reglasColegiaturas);
            await context.SaveChangesAsync();

            // === CREATE BECAS ===
            var alumnosConBeca = alumnos.Skip(2).Take(8).ToList();
            foreach (var alumno in alumnosConBeca)
            {
                var porcentajeBeca = random.Next(20, 31);
                var beca = new BecaAlumno
                {
                    Id = Guid.NewGuid(),
                    AlumnoId = alumno.Id,
                    CicloId = ciclo2026.Id,
                    Tipo = BecaTipo.Porcentaje,
                    Valor = porcentajeBeca
                };
                context.BecasAlumno.Add(beca);
            }
            await context.SaveChangesAsync();

            // === CREATE REGLAS DE RECARGO ===
            var reglaRecargo = new ReglaRecargo
            {
                Id = Guid.NewGuid(),
                CicloId = ciclo2026.Id,
                ConceptoCobroId = conceptoRecargo.Id,
                Porcentaje = 5.00m
            };
            context.ReglasRecargo.Add(reglaRecargo);
            await context.SaveChangesAsync();

            // === AVISO DE PRIVACIDAD ===
            if (!context.AvisosPrivacidad.Any())
            {
                var avisoPrivacidad = new AvisoPrivacidad
                {
                    Id = Guid.NewGuid(),
                    Version = "2026-01-19",
                    Contenido = @"AVISO DE PRIVACIDAD - Tlaoami",
                    Vigente = true,
                    PublicadoEnUtc = DateTime.UtcNow,
                    CreatedAtUtc = DateTime.UtcNow
                };

                context.AvisosPrivacidad.Add(avisoPrivacidad);
                await context.SaveChangesAsync();
            }
        }
    }
}
