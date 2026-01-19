using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly TlaoamiDbContext _context;

        public SetupController(TlaoamiDbContext context)
        {
            _context = context;
        }

        [HttpPost("test-data")]
        public async Task<IActionResult> CreateTestData()
        {
            var alumno = new Alumno { Id = Guid.NewGuid(), Nombre = "Juan", Apellido = "Perez" };
            _context.Alumnos.Add(alumno);
            await _context.SaveChangesAsync();

            var factura = new Factura { AlumnoId = alumno.Id, Monto = 1000, Estado = EstadoFactura.Pendiente };
            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            return Ok(new { facturaId = factura.Id, alumnoId = alumno.Id });
        }

        /// <summary>
        /// Solo para DEV: crea ciclo 2025-2026, grupos 2A/2B y alumnos Juan/Ana/Luis con asignación activa.
        /// Idempotente: no duplica si ya existen por nombre/matrícula.
        /// </summary>
        [HttpPost("seed-alumnos-grupos")]
        public async Task<IActionResult> SeedAlumnosGrupos()
        {
            var created = new List<string>();

            // Ciclo escolar 2025-2026
            var ciclo = await _context.CiclosEscolares.FirstOrDefaultAsync(c => c.Nombre == "2025-2026");
            if (ciclo == null)
            {
                ciclo = new CicloEscolar
                {
                    Id = Guid.NewGuid(),
                    Nombre = "2025-2026",
                    FechaInicio = new DateTime(2025, 8, 1),
                    FechaFin = new DateTime(2026, 6, 30),
                    Activo = true
                };
                _context.CiclosEscolares.Add(ciclo);
                created.Add("ciclo-escolar");
            }

            // Grupos 2A y 2B
            var grupo2A = await _context.Grupos.FirstOrDefaultAsync(g => g.Nombre == "2A" && g.CicloEscolarId == ciclo.Id);
            if (grupo2A == null)
            {
                grupo2A = new Grupo
                {
                    Id = Guid.NewGuid(),
                    Nombre = "2A",
                    Grado = 2,
                    Turno = "Matutino",
                    CicloEscolarId = ciclo.Id
                };
                _context.Grupos.Add(grupo2A);
                created.Add("grupo-2A");
            }

            var grupo2B = await _context.Grupos.FirstOrDefaultAsync(g => g.Nombre == "2B" && g.CicloEscolarId == ciclo.Id);
            if (grupo2B == null)
            {
                grupo2B = new Grupo
                {
                    Id = Guid.NewGuid(),
                    Nombre = "2B",
                    Grado = 2,
                    Turno = "Vespertino",
                    CicloEscolarId = ciclo.Id
                };
                _context.Grupos.Add(grupo2B);
                created.Add("grupo-2B");
            }

            // Alumnos
            var seedAlumnos = new[]
            {
                new { Matricula = "A2025001", Nombre = "Juan", Apellido = "Pérez", Email = "juan@example.com" },
                new { Matricula = "A2025002", Nombre = "Ana", Apellido = "López", Email = "ana@example.com" },
                new { Matricula = "A2025003", Nombre = "Luis", Apellido = "Martínez", Email = "luis@example.com" }
            };

            var alumnos = new List<Alumno>();

            foreach (var seed in seedAlumnos)
            {
                var alumno = await _context.Alumnos.FirstOrDefaultAsync(a => a.Matricula == seed.Matricula);
                if (alumno == null)
                {
                    alumno = new Alumno
                    {
                        Id = Guid.NewGuid(),
                        Matricula = seed.Matricula,
                        Nombre = seed.Nombre,
                        Apellido = seed.Apellido,
                        Email = seed.Email,
                        Activo = true,
                        FechaInscripcion = DateTime.UtcNow
                    };
                    _context.Alumnos.Add(alumno);
                    created.Add($"alumno-{seed.Matricula}");
                }
                alumnos.Add(alumno);
            }

            await _context.SaveChangesAsync();

            // Asignar alumnos a grupos (activos)
            var asignaciones = new List<(Alumno alumno, Grupo grupo)>
            {
                (alumnos.First(a => a.Matricula == "A2025001"), grupo2A),
                (alumnos.First(a => a.Matricula == "A2025002"), grupo2A),
                (alumnos.First(a => a.Matricula == "A2025003"), grupo2B)
            };

            foreach (var (alumno, grupo) in asignaciones)
            {
                var activa = await _context.AsignacionesGrupo.FirstOrDefaultAsync(ag => ag.AlumnoId == alumno.Id && ag.Activo && ag.FechaFin == null);
                if (activa == null)
                {
                    _context.AsignacionesGrupo.Add(new AlumnoGrupo
                    {
                        Id = Guid.NewGuid(),
                        AlumnoId = alumno.Id,
                        GrupoId = grupo.Id,
                        FechaInicio = DateTime.UtcNow.Date,
                        FechaFin = null,
                        Activo = true
                    });
                    created.Add($"asignacion-{alumno.Matricula}-{grupo.Nombre}");
                }
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Seed aplicado (idempotente)",
                created
            });
        }
    }
}
