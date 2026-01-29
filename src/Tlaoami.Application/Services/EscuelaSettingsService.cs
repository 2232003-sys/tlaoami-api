using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Settings;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class EscuelaSettingsService : IEscuelaSettingsService
    {
        private readonly TlaoamiDbContext _context;

        public EscuelaSettingsService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<EscuelaSettingsDto?> GetSettingsAsync()
        {
            var s = await _context.EscuelaSettings
                .OrderBy(x => x.CreatedAtUtc)
                .FirstOrDefaultAsync();
            if (s == null) return null;
            return new EscuelaSettingsDto
            {
                EscuelaId = s.EscuelaId,
                Nombre = s.Nombre,
                RazonSocial = s.RazonSocial,
                Direccion = s.Direccion,
                Telefono = s.Telefono,
                Email = s.Email,
                LogoUrl = s.LogoUrl,
                TextoRecibos = s.TextoRecibos,
                Moneda = s.Moneda,
                ZonaHoraria = s.ZonaHoraria,
                DiaCorteColegiatura = s.DiaCorteColegiatura,
                BloquearReinscripcionConSaldo = s.BloquearReinscripcionConSaldo,
                CreatedAtUtc = s.CreatedAtUtc,
                UpdatedAtUtc = s.UpdatedAtUtc
            };
        }

        public async Task<EscuelaSettingsDto> UpdateSettingsAsync(EscuelaSettingsDto dto)
        {
            // Busca por EscuelaId; si no existe, crea
            var s = await _context.EscuelaSettings
                .FirstOrDefaultAsync(x => x.EscuelaId == dto.EscuelaId);

            if (s == null)
            {
                s = new EscuelaSettings
                {
                    Id = Guid.NewGuid(),
                    EscuelaId = dto.EscuelaId,
                    Nombre = dto.Nombre,
                    RazonSocial = dto.RazonSocial,
                    Direccion = dto.Direccion,
                    Telefono = dto.Telefono,
                    Email = dto.Email,
                    LogoUrl = dto.LogoUrl,
                    TextoRecibos = dto.TextoRecibos,
                    Moneda = dto.Moneda,
                    ZonaHoraria = dto.ZonaHoraria,
                    DiaCorteColegiatura = dto.DiaCorteColegiatura,
                    BloquearReinscripcionConSaldo = dto.BloquearReinscripcionConSaldo,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = null
                };
                _context.EscuelaSettings.Add(s);
            }
            else
            {
                s.Nombre = dto.Nombre;
                s.RazonSocial = dto.RazonSocial;
                s.Direccion = dto.Direccion;
                s.Telefono = dto.Telefono;
                s.Email = dto.Email;
                s.LogoUrl = dto.LogoUrl;
                s.TextoRecibos = dto.TextoRecibos;
                s.Moneda = dto.Moneda;
                s.ZonaHoraria = dto.ZonaHoraria;
                s.DiaCorteColegiatura = dto.DiaCorteColegiatura;
                s.BloquearReinscripcionConSaldo = dto.BloquearReinscripcionConSaldo;
                s.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new EscuelaSettingsDto
            {
                EscuelaId = s.EscuelaId,
                Nombre = s.Nombre,
                RazonSocial = s.RazonSocial,
                Direccion = s.Direccion,
                Telefono = s.Telefono,
                Email = s.Email,
                LogoUrl = s.LogoUrl,
                TextoRecibos = s.TextoRecibos,
                Moneda = s.Moneda,
                ZonaHoraria = s.ZonaHoraria,
                DiaCorteColegiatura = s.DiaCorteColegiatura,
                BloquearReinscripcionConSaldo = s.BloquearReinscripcionConSaldo,
                CreatedAtUtc = s.CreatedAtUtc,
                UpdatedAtUtc = s.UpdatedAtUtc
            };
        }
    }
}
