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
                DiaCorteColegiatura = s.DiaCorteColegiatura,
                BloquearReinscripcionConSaldo = s.BloquearReinscripcionConSaldo,
                ZonaHoraria = s.ZonaHoraria,
                Moneda = s.Moneda,
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
                    DiaCorteColegiatura = dto.DiaCorteColegiatura,
                    BloquearReinscripcionConSaldo = dto.BloquearReinscripcionConSaldo,
                    ZonaHoraria = dto.ZonaHoraria,
                    Moneda = dto.Moneda,
                    CreatedAtUtc = DateTime.UtcNow,
                    UpdatedAtUtc = null
                };
                _context.EscuelaSettings.Add(s);
            }
            else
            {
                s.DiaCorteColegiatura = dto.DiaCorteColegiatura;
                s.BloquearReinscripcionConSaldo = dto.BloquearReinscripcionConSaldo;
                s.ZonaHoraria = dto.ZonaHoraria;
                s.Moneda = dto.Moneda;
                s.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            return new EscuelaSettingsDto
            {
                EscuelaId = s.EscuelaId,
                DiaCorteColegiatura = s.DiaCorteColegiatura,
                BloquearReinscripcionConSaldo = s.BloquearReinscripcionConSaldo,
                ZonaHoraria = s.ZonaHoraria,
                Moneda = s.Moneda,
                CreatedAtUtc = s.CreatedAtUtc,
                UpdatedAtUtc = s.UpdatedAtUtc
            };
        }
    }
}
