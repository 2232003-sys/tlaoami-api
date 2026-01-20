using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Services;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;
using Xunit;

namespace Tlaoami.Tests
{
    public class ReinscripcionServiceTests : IDisposable
    {
        private readonly TlaoamiDbContext _context;
        private readonly ReinscripcionService _reinscripcionService;
        private readonly AlumnoService _alumnoService;
        private readonly CicloEscolarService _cicloService;
        private readonly AsignacionGrupoService _asignacionService;

        public ReinscripcionServiceTests()
        {
            var options = new DbContextOptionsBuilder<TlaoamiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new TlaoamiDbContext(options);

            _alumnoService = new AlumnoService(_context);
            _cicloService = new CicloEscolarService(_context);
            _asignacionService = new AsignacionGrupoService(_context);
            _reinscripcionService = new ReinscripcionService(_context, _alumnoService, _cicloService, _asignacionService);
        }

        [Fact]
        public async Task ReinscribirAsync_ConAdeudoPendiente_LanzaBusinessException()
        {
            // Arrange: Crear alumno con adeudo
            var alumno = new Alumno
            {
                Id = Guid.NewGuid(),
                Matricula = "TEST001",
                Nombre = "Juan",
                Apellido = "Pérez",
                Activo = true,
                FechaInscripcion = DateTime.UtcNow
            };
            _context.Alumnos.Add(alumno);

            // Crear factura pendiente (adeudo > 0)
            var factura = new Factura
            {
                Id = Guid.NewGuid(),
                AlumnoId = alumno.Id,
                NumeroFactura = "FAC-001",
                Monto = 1000m,
                FechaEmision = DateTime.UtcNow,
                Estado = EstadoFactura.Pendiente
            };
            _context.Facturas.Add(factura);

            // Crear ciclo activo y grupo
            var cicloActivo = new CicloEscolar
            {
                Id = Guid.NewGuid(),
                Nombre = "2026",
                FechaInicio = DateTime.UtcNow.AddMonths(-1),
                FechaFin = DateTime.UtcNow.AddMonths(11),
                Activo = true
            };
            _context.CiclosEscolares.Add(cicloActivo);

            var grupo = new Grupo
            {
                Id = Guid.NewGuid(),
                Nombre = "1A",
                Grado = 1,
                Turno = "Matutino",
                Capacidad = 30,
                CicloEscolarId = cicloActivo.Id
            };
            _context.Grupos.Add(grupo);

            await _context.SaveChangesAsync();

            var request = new ReinscripcionRequestDto
            {
                AlumnoId = alumno.Id,
                GrupoId = grupo.Id
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(() => _reinscripcionService.ReinscribirAsync(request));
            Assert.Equal("ADEUDO_PENDIENTE", exception.Code);
        }

        [Fact]
        public async Task ReinscribirAsync_SinAdeudo_ReinscribeExitosamente()
        {
            // Arrange: Crear alumno sin adeudo
            var alumno = new Alumno
            {
                Id = Guid.NewGuid(),
                Matricula = "TEST002",
                Nombre = "María",
                Apellido = "García",
                Activo = true,
                FechaInscripcion = DateTime.UtcNow
            };
            _context.Alumnos.Add(alumno);

            // Crear ciclo activo
            var cicloActivo = new CicloEscolar
            {
                Id = Guid.NewGuid(),
                Nombre = "2026",
                FechaInicio = DateTime.UtcNow.AddMonths(-1),
                FechaFin = DateTime.UtcNow.AddMonths(11),
                Activo = true
            };
            _context.CiclosEscolares.Add(cicloActivo);

            // Crear grupo con cupo disponible
            var grupoNuevo = new Grupo
            {
                Id = Guid.NewGuid(),
                Nombre = "2A",
                Grado = 2,
                Turno = "Matutino",
                Capacidad = 30,
                CicloEscolarId = cicloActivo.Id
            };
            _context.Grupos.Add(grupoNuevo);

            // Crear asignación previa (ciclo anterior)
            var cicloAnterior = new CicloEscolar
            {
                Id = Guid.NewGuid(),
                Nombre = "2025",
                FechaInicio = DateTime.UtcNow.AddMonths(-13),
                FechaFin = DateTime.UtcNow.AddMonths(-1),
                Activo = false
            };
            _context.CiclosEscolares.Add(cicloAnterior);

            var grupoAnterior = new Grupo
            {
                Id = Guid.NewGuid(),
                Nombre = "1A",
                Grado = 1,
                Turno = "Matutino",
                Capacidad = 30,
                CicloEscolarId = cicloAnterior.Id
            };
            _context.Grupos.Add(grupoAnterior);

            var asignacionAnterior = new AlumnoGrupo
            {
                Id = Guid.NewGuid(),
                AlumnoId = alumno.Id,
                GrupoId = grupoAnterior.Id,
                FechaInicio = DateTime.UtcNow.AddMonths(-12),
                FechaFin = null,
                Activo = true
            };
            _context.AsignacionesGrupo.Add(asignacionAnterior);

            await _context.SaveChangesAsync();

            var request = new ReinscripcionRequestDto
            {
                AlumnoId = alumno.Id,
                GrupoId = grupoNuevo.Id
            };

            // Act
            var result = await _reinscripcionService.ReinscribirAsync(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("REINSCRITO", result.Status);
            Assert.Equal(alumno.Id, result.AlumnoId);
            Assert.Equal(grupoNuevo.Id, result.GrupoId);
            Assert.Equal(cicloActivo.Id, result.CicloId);

            // Verificar que asignación anterior fue cerrada
            var asignacionCerrada = await _context.AsignacionesGrupo.FindAsync(asignacionAnterior.Id);
            Assert.NotNull(asignacionCerrada);
            Assert.False(asignacionCerrada.Activo);
            Assert.NotNull(asignacionCerrada.FechaFin);

            // Verificar que se creó nueva asignación activa
            var nuevaAsignacion = await _context.AsignacionesGrupo
                .FirstOrDefaultAsync(a => a.AlumnoId == alumno.Id && a.GrupoId == grupoNuevo.Id && a.Activo);
            Assert.NotNull(nuevaAsignacion);
        }

        [Fact]
        public async Task ReinscribirAsync_GrupoSinCupo_LanzaBusinessException()
        {
            // Arrange: Crear alumno sin adeudo
            var alumno = new Alumno
            {
                Id = Guid.NewGuid(),
                Matricula = "TEST003",
                Nombre = "Pedro",
                Apellido = "López",
                Activo = true,
                FechaInscripcion = DateTime.UtcNow
            };
            _context.Alumnos.Add(alumno);

            // Crear ciclo activo
            var cicloActivo = new CicloEscolar
            {
                Id = Guid.NewGuid(),
                Nombre = "2026",
                FechaInicio = DateTime.UtcNow.AddMonths(-1),
                FechaFin = DateTime.UtcNow.AddMonths(11),
                Activo = true
            };
            _context.CiclosEscolares.Add(cicloActivo);

            // Crear grupo con capacidad = 1
            var grupo = new Grupo
            {
                Id = Guid.NewGuid(),
                Nombre = "3A",
                Grado = 3,
                Turno = "Matutino",
                Capacidad = 1,
                CicloEscolarId = cicloActivo.Id
            };
            _context.Grupos.Add(grupo);

            // Llenar el grupo (capacidad completa)
            var otroAlumno = new Alumno
            {
                Id = Guid.NewGuid(),
                Matricula = "TEST999",
                Nombre = "Otro",
                Apellido = "Alumno",
                Activo = true,
                FechaInscripcion = DateTime.UtcNow
            };
            _context.Alumnos.Add(otroAlumno);

            var asignacionExistente = new AlumnoGrupo
            {
                Id = Guid.NewGuid(),
                AlumnoId = otroAlumno.Id,
                GrupoId = grupo.Id,
                FechaInicio = DateTime.UtcNow.AddDays(-5),
                FechaFin = null,
                Activo = true
            };
            _context.AsignacionesGrupo.Add(asignacionExistente);

            await _context.SaveChangesAsync();

            var request = new ReinscripcionRequestDto
            {
                AlumnoId = alumno.Id,
                GrupoId = grupo.Id
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(() => _reinscripcionService.ReinscribirAsync(request));
            Assert.Equal("GRUPO_SIN_CUPO", exception.Code);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
