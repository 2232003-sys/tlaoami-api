using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class AvisoPrivacidadService : IAvisoPrivacidadService
    {
        private readonly TlaoamiDbContext _context;

        public AvisoPrivacidadService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<AvisoPrivacidadDto> GetAvisoVigenteAsync()
        {
            var aviso = await _context.AvisosPrivacidad
                .FirstOrDefaultAsync(a => a.Vigente);

            if (aviso == null)
                throw new NotFoundException(
                    "No hay aviso de privacidad vigente publicado.",
                    code: "AVISO_NO_VIGENTE");

            return MapToDto(aviso);
        }

        public async Task<EstadoAceptacionDto> GetEstadoAceptacionAsync(Guid usuarioId)
        {
            var avisoVigente = await _context.AvisosPrivacidad
                .FirstOrDefaultAsync(a => a.Vigente);

            if (avisoVigente == null)
            {
                // No hay aviso vigente, no se requiere aceptación
                return new EstadoAceptacionDto
                {
                    RequiereAceptacion = false,
                    VersionActual = null,
                    AceptadoEnUtc = null
                };
            }

            // Verificar si el usuario ya aceptó este aviso
            var aceptacion = await _context.AceptacionesAvisoPrivacidad
                .FirstOrDefaultAsync(a =>
                    a.UsuarioId == usuarioId &&
                    a.AvisoPrivacidadId == avisoVigente.Id);

            return new EstadoAceptacionDto
            {
                RequiereAceptacion = aceptacion == null,
                VersionActual = avisoVigente.Version,
                AceptadoEnUtc = aceptacion?.AceptadoEnUtc
            };
        }

        public async Task<EstadoAceptacionDto> AceptarAvisoAsync(Guid usuarioId, string? ip = null, string? userAgent = null)
        {
            var avisoVigente = await _context.AvisosPrivacidad
                .FirstOrDefaultAsync(a => a.Vigente);

            if (avisoVigente == null)
                throw new NotFoundException(
                    "No hay aviso de privacidad vigente para aceptar.",
                    code: "AVISO_NO_VIGENTE");

            // Verificar si ya existe aceptación (idempotencia)
            var aceptacionExistente = await _context.AceptacionesAvisoPrivacidad
                .FirstOrDefaultAsync(a =>
                    a.UsuarioId == usuarioId &&
                    a.AvisoPrivacidadId == avisoVigente.Id);

            if (aceptacionExistente == null)
            {
                // Crear nueva aceptación
                var aceptacion = new AceptacionAvisoPrivacidad
                {
                    Id = Guid.NewGuid(),
                    AvisoPrivacidadId = avisoVigente.Id,
                    UsuarioId = usuarioId,
                    AceptadoEnUtc = DateTime.UtcNow,
                    Ip = ip,
                    UserAgent = userAgent
                };

                _context.AceptacionesAvisoPrivacidad.Add(aceptacion);
                await _context.SaveChangesAsync();
            }

            // Retornar estado actualizado
            return new EstadoAceptacionDto
            {
                RequiereAceptacion = false,
                VersionActual = avisoVigente.Version,
                AceptadoEnUtc = aceptacionExistente?.AceptadoEnUtc ?? DateTime.UtcNow
            };
        }

        public async Task<AvisoPrivacidadDto> PublicarAvisoAsync(AvisoPrivacidadCreateDto dto)
        {
            // Validar
            if (string.IsNullOrWhiteSpace(dto.Version))
                throw new ValidationException("Version es requerida.", code: "AVISO_INVALIDO");

            if (string.IsNullOrWhiteSpace(dto.Contenido))
                throw new ValidationException("Contenido es requerido.", code: "AVISO_INVALIDO");

            // Desactivar aviso vigente anterior
            var avisoAnterior = await _context.AvisosPrivacidad
                .FirstOrDefaultAsync(a => a.Vigente);

            if (avisoAnterior != null)
            {
                avisoAnterior.Vigente = false;
                _context.AvisosPrivacidad.Update(avisoAnterior);
            }

            // Crear nuevo aviso vigente
            var nuevoAviso = new AvisoPrivacidad
            {
                Id = Guid.NewGuid(),
                Version = dto.Version.Trim(),
                Contenido = dto.Contenido.Trim(),
                Vigente = true,
                PublicadoEnUtc = DateTime.UtcNow,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.AvisosPrivacidad.Add(nuevoAviso);
            await _context.SaveChangesAsync();

            return MapToDto(nuevoAviso);
        }

        public async Task<bool> UsuarioHaAceptadoVigenteAsync(Guid usuarioId)
        {
            var avisoVigente = await _context.AvisosPrivacidad
                .FirstOrDefaultAsync(a => a.Vigente);

            if (avisoVigente == null)
                return true; // No hay aviso, considera como aceptado

            var aceptacion = await _context.AceptacionesAvisoPrivacidad
                .AnyAsync(a =>
                    a.UsuarioId == usuarioId &&
                    a.AvisoPrivacidadId == avisoVigente.Id);

            return aceptacion;
        }

        // === Privados ===

        private static AvisoPrivacidadDto MapToDto(AvisoPrivacidad entity)
        {
            return new AvisoPrivacidadDto
            {
                Id = entity.Id,
                Version = entity.Version,
                Contenido = entity.Contenido,
                PublicadoEnUtc = entity.PublicadoEnUtc
            };
        }
    }
}
