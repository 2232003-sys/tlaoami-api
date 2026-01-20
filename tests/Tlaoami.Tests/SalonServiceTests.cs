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
    public class SalonServiceTests : IDisposable
    {
        private readonly TlaoamiDbContext _context;
        private readonly SalonService _salonService;

        public SalonServiceTests()
        {
            var options = new DbContextOptionsBuilder<TlaoamiDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _context = new TlaoamiDbContext(options);
            _salonService = new SalonService(_context);
        }

        [Fact]
        public async Task CreateAsync_CodigoUnico_CreaSalon()
        {
            // Arrange
            var dto = new SalonCreateDto
            {
                Codigo = "A101",
                Nombre = "Aula 101",
                Capacidad = 30
            };

            // Act
            var result = await _salonService.CreateAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("A101", result.Codigo);
            Assert.Equal("Aula 101", result.Nombre);
            Assert.Equal(30, result.Capacidad);
            Assert.True(result.Activo);

            var salonEnBd = await _context.Salones.FindAsync(result.Id);
            Assert.NotNull(salonEnBd);
        }

        [Fact]
        public async Task CreateAsync_CodigoDuplicado_LanzaBusinessException()
        {
            // Arrange: Crear salón existente
            var salonExistente = new Salon
            {
                Id = Guid.NewGuid(),
                Codigo = "A101",
                Nombre = "Aula Original",
                Activo = true
            };
            _context.Salones.Add(salonExistente);
            await _context.SaveChangesAsync();

            var dto = new SalonCreateDto
            {
                Codigo = "A101",
                Nombre = "Intento Duplicado",
                Capacidad = 25
            };

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(() => 
                _salonService.CreateAsync(dto));

            Assert.Equal("SALON_CODIGO_DUPLICADO", exception.Code);
        }

        [Fact]
        public async Task GetAllAsync_FiltroActivo_RetornaSoloActivos()
        {
            // Arrange
            var salonActivo = new Salon
            {
                Id = Guid.NewGuid(),
                Codigo = "A101",
                Nombre = "Activo",
                Activo = true
            };

            var salonInactivo = new Salon
            {
                Id = Guid.NewGuid(),
                Codigo = "A102",
                Nombre = "Inactivo",
                Activo = false
            };

            _context.Salones.AddRange(salonActivo, salonInactivo);
            await _context.SaveChangesAsync();

            // Act
            var result = await _salonService.GetAllAsync(activo: true);

            // Assert
            var lista = result.ToList();
            Assert.Single(lista);
            Assert.Equal("A101", lista[0].Codigo);
        }

        [Fact]
        public async Task UpdateAsync_CambiarCodigo_Actualiza()
        {
            // Arrange
            var salon = new Salon
            {
                Id = Guid.NewGuid(),
                Codigo = "A101",
                Nombre = "Original",
                Capacidad = 30,
                Activo = true
            };
            _context.Salones.Add(salon);
            await _context.SaveChangesAsync();

            var updateDto = new SalonUpdateDto
            {
                Codigo = "A102",
                Nombre = "Actualizado"
            };

            // Act
            var result = await _salonService.UpdateAsync(salon.Id, updateDto);

            // Assert
            Assert.Equal("A102", result.Codigo);
            Assert.Equal("Actualizado", result.Nombre);
            Assert.NotNull(result.UpdatedAt);
        }

        [Fact]
        public async Task DeleteAsync_SalonConGrupos_LanzaBusinessException()
        {
            // Arrange: Crear salón con grupo asignado
            var salon = new Salon
            {
                Id = Guid.NewGuid(),
                Codigo = "A101",
                Nombre = "Con Grupos",
                Activo = true
            };

            var ciclo = new CicloEscolar
            {
                Id = Guid.NewGuid(),
                Nombre = "2026",
                FechaInicio = DateTime.UtcNow,
                FechaFin = DateTime.UtcNow.AddMonths(12),
                Activo = true
            };

            var grupo = new Grupo
            {
                Id = Guid.NewGuid(),
                Nombre = "1A",
                Grado = 1,
                CicloEscolarId = ciclo.Id,
                SalonId = salon.Id
            };

            _context.Salones.Add(salon);
            _context.CiclosEscolares.Add(ciclo);
            _context.Grupos.Add(grupo);
            await _context.SaveChangesAsync();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<BusinessException>(() => 
                _salonService.DeleteAsync(salon.Id));

            Assert.Equal("SALON_EN_USO", exception.Code);
        }

        [Fact]
        public async Task DeleteAsync_SalonSinGrupos_Inactiva()
        {
            // Arrange
            var salon = new Salon
            {
                Id = Guid.NewGuid(),
                Codigo = "A101",
                Nombre = "Sin Grupos",
                Activo = true
            };
            _context.Salones.Add(salon);
            await _context.SaveChangesAsync();

            // Act
            await _salonService.DeleteAsync(salon.Id);

            // Assert: Soft delete - debe estar inactivo, no eliminado
            var salonInactivo = await _context.Salones.FindAsync(salon.Id);
            Assert.NotNull(salonInactivo);
            Assert.False(salonInactivo.Activo);
            Assert.NotNull(salonInactivo.UpdatedAt);
        }

        public void Dispose()
        {
            _context?.Dispose();
        }
    }
}
