using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
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
        private readonly AsignacionGrupoService _asignacionService;

        public ReinscripcionServiceTests()
        {
            var options = new DbContextOptionsBuilder<TlaoamiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new TlaoamiDbContext(options);
            _alumnoService = new AlumnoService(_context);
            _asignacionService = new AsignacionGrupoService(_context);
            
            _reinscripcionService = new ReinscripcionService(
                _context, 
                _asignacionService,
                _alumnoService
            );
        }

        [Fact]
        public async Task CrearReinscripcionAsync_ConAdeudoPendiente_LanzaBusinessException()
        {
            // Arrange: Crear alumno, ciclo y grupo
            var alumno = new Alumno
            {
                Id = Guid.NewGuid(),
                Matricula = "TEST001",
                Nombre = "Juan",
                Apellido = "Pérez",
                Activo = true,
                FechaInscripcion = DateTime.UtcNow
            };

            var ciclo = new CicloEscolar
            {
                Id = Guid.NewGuid(),
                Nombre = "2026",
                FechaInicio = DateTime.UtcNow.AddMonths(-1),
                FechaFin = DateTime.UtcNow.AddMonths(11),
                Activo = true
            };

            var grupo = new Grupo
            {
                Id = Guid.NewGuid(),
                Codigo = "1A-TEST",
                Nombre = "1A",
                Grado = 1,
                Turno = "Matutino",
                Capacidad = 30,
                CicloEscolarId = ciclo.Id
            };

            _context.Alumnos.Add(alumno);
            _context.CiclosEscolares.Add(ciclo);
            _context.Grupos.Add(grupo);
            
            // Crear factura pendiente para simular adeudo
            var factura = new Factura
            {
                Id = Guid.NewGuid(),
                AlumnoId = alumno.Id,
                NumeroFactura = "FAC-001",
                Monto = 100.00m,
                FechaEmision = DateTime.UtcNow,
                Estado = EstadoFactura.Pendiente
            };
            _context.Facturas.Add(factura);
            
            await _context.SaveChangesAsync();

            var dto = new ReinscripcionCreateDto
            {
                AlumnoId = alumno.Id,
                CicloDestinoId = ciclo.Id,
                GrupoDestinoId = grupo.Id
            };

            // Act & Assert: Debe lanzar BusinessException con código REINSCRIPCION_BLOQUEADA_ADEUDO
            var exception = await Assert.ThrowsAsync<BusinessException>(() => 
                _reinscripcionService.CrearReinscripcionAsync(dto, Guid.NewGuid()));
            
            Assert.Equal("REINSCRIPCION_BLOQUEADA_ADEUDO", exception.Code);
        }

        [Fact]
        public async Task CrearReinscripcionAsync_SinAdeudo_CreaBloqueadaExitosamente()
        {
            // Arrange: Crear alumno, ciclos y grupos
            var alumno = new Alumno
            {
                Id = Guid.NewGuid(),
                Matricula = "TEST002",
                Nombre = "María",
                Apellido = "García",
                Activo = true,
                FechaInscripcion = DateTime.UtcNow
            };

            var cicloActual = new CicloEscolar
            {
                Id = Guid.NewGuid(),
                Nombre = "2025",
                FechaInicio = DateTime.UtcNow.AddMonths(-1),
                FechaFin = DateTime.UtcNow.AddMonths(11),
                Activo = true
            };

            var cicloDestino = new CicloEscolar
            {
                Id = Guid.NewGuid(),
                Nombre = "2026",
                FechaInicio = DateTime.UtcNow.AddMonths(11),
                FechaFin = DateTime.UtcNow.AddMonths(23),
                Activo = true
            };

            var grupoActual = new Grupo
            {
                Id = Guid.NewGuid(),
                Codigo = "1A-TEST",
                Nombre = "1A",
                Grado = 1,
                Turno = "Matutino",
                Capacidad = 30,
                CicloEscolarId = cicloActual.Id
            };

            var grupoDestino = new Grupo
            {
                Id = Guid.NewGuid(),
                Codigo = "2A-TEST",
                Nombre = "2A",
                Grado = 2,
                Turno = "Matutino",
                Capacidad = 30,
                CicloEscolarId = cicloDestino.Id
            };

            // Asignación actual
            var asignacionActual = new AlumnoGrupo
            {
                Id = Guid.NewGuid(),
                AlumnoId = alumno.Id,
                GrupoId = grupoActual.Id,
                FechaInicio = DateTime.UtcNow.AddMonths(-1),
                Activo = true
            };

            _context.Alumnos.Add(alumno);
            _context.CiclosEscolares.AddRange(cicloActual, cicloDestino);
            _context.Grupos.AddRange(grupoActual, grupoDestino);
            _context.AsignacionesGrupo.Add(asignacionActual);
            await _context.SaveChangesAsync();

            var dto = new ReinscripcionCreateDto
            {
                AlumnoId = alumno.Id,
                CicloDestinoId = cicloDestino.Id,
                GrupoDestinoId = grupoDestino.Id
            };

            // Act
            var result = await _reinscripcionService.CrearReinscripcionAsync(dto, Guid.NewGuid());

            // Assert: Debe crear una Reinscripción con estado Completada
            Assert.NotNull(result);
            Assert.Equal(alumno.Id, result.AlumnoId);
            Assert.Equal(cicloDestino.Id, result.CicloDestinoId);
            Assert.Equal(grupoDestino.Id, result.GrupoDestinoId);
            Assert.Equal("Completada", result.Estado);

            // Verificar que se creó el registro en BD
            var reinscripcionCreada = await _context.Reinscripciones
                .FirstOrDefaultAsync(r => r.AlumnoId == alumno.Id && r.CicloDestinoId == cicloDestino.Id);
            Assert.NotNull(reinscripcionCreada);
        }

        [Fact]
        public async Task CrearReinscripcionAsync_AlumnoYaInscritoEnCiclo_LanzaBusinessException()
        {
            // Arrange: Crear alumno que ya tiene asignación en ciclo destino
            var alumno = new Alumno
            {
                Id = Guid.NewGuid(),
                Matricula = "TEST003",
                Nombre = "Pedro",
                Apellido = "López",
                Activo = true,
                FechaInscripcion = DateTime.UtcNow
            };

            var ciclo = new CicloEscolar
            {
                Id = Guid.NewGuid(),
                Nombre = "2026",
                FechaInicio = DateTime.UtcNow.AddMonths(-1),
                FechaFin = DateTime.UtcNow.AddMonths(11),
                Activo = true
            };

            var grupo1 = new Grupo
            {
                Id = Guid.NewGuid(),
                Codigo = "1A-TEST",
                Nombre = "1A",
                Grado = 1,
                Turno = "Matutino",
                Capacidad = 30,
                CicloEscolarId = ciclo.Id
            };

            var grupo2 = new Grupo
            {
                Id = Guid.NewGuid(),
                Codigo = "1B-TEST",
                Nombre = "1B",
                Grado = 1,
                Turno = "Vespertino",
                Capacidad = 30,
                CicloEscolarId = ciclo.Id
            };

            // Alumno ya tiene asignación activa en el ciclo destino (grupo1)
            var asignacionExistente = new AlumnoGrupo
            {
                Id = Guid.NewGuid(),
                AlumnoId = alumno.Id,
                GrupoId = grupo1.Id,
                FechaInicio = DateTime.UtcNow.AddDays(-10),
                Activo = true
            };

            _context.Alumnos.Add(alumno);
            _context.CiclosEscolares.Add(ciclo);
            _context.Grupos.AddRange(grupo1, grupo2);
            _context.AsignacionesGrupo.Add(asignacionExistente);
            await _context.SaveChangesAsync();

            var dto = new ReinscripcionCreateDto
            {
                AlumnoId = alumno.Id,
                CicloDestinoId = ciclo.Id,
                GrupoDestinoId = grupo2.Id  // Intentar inscribirse en otro grupo del mismo ciclo
            };

            // Act & Assert: Debe lanzar BusinessException porque ya está inscrito en ese ciclo
            var exception = await Assert.ThrowsAsync<BusinessException>(() => 
                _reinscripcionService.CrearReinscripcionAsync(dto, Guid.NewGuid()));

            Assert.Equal("ALUMNO_YA_INSCRITO_EN_CICLO", exception.Code);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
