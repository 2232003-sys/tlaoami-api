using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public interface IReceptorFiscalService
    {
        Task<ReceptorFiscalDto?> GetByAlumnoIdAsync(Guid alumnoId);
        Task<ReceptorFiscalDto> UpsertAsync(Guid alumnoId, ReceptorFiscalUpsertDto dto);
    }

    public class ReceptorFiscalService : IReceptorFiscalService
    {
        private readonly TlaoamiDbContext _context;

        public ReceptorFiscalService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<ReceptorFiscalDto?> GetByAlumnoIdAsync(Guid alumnoId)
        {
            var receptor = await _context.ReceptoresFiscales
                .FirstOrDefaultAsync(rf => rf.AlumnoId == alumnoId && rf.Activo);

            if (receptor == null)
                return null;

            return MapToDto(receptor);
        }

        public async Task<ReceptorFiscalDto> UpsertAsync(Guid alumnoId, ReceptorFiscalUpsertDto dto)
        {
            // Validar que alumno existe
            var alumno = await _context.Alumnos.FindAsync(alumnoId);
            if (alumno == null)
                throw new NotFoundException("Alumno no encontrado", code: "ALUMNO_NO_ENCONTRADO");

            // Validar formato RFC
            ValidarRfc(dto.Rfc);

            // Validar CP (5 dígitos)
            if (!dto.CodigoPostalFiscal.ToString().All(char.IsDigit) || dto.CodigoPostalFiscal.Length != 5)
                throw new BusinessException("Código postal debe ser 5 dígitos", code: "CP_INVALIDO");

            // Buscar o crear
            var receptor = await _context.ReceptoresFiscales
                .FirstOrDefaultAsync(rf => rf.AlumnoId == alumnoId);

            if (receptor == null)
            {
                receptor = new ReceptorFiscal
                {
                    Id = Guid.NewGuid(),
                    AlumnoId = alumnoId,
                    Rfc = dto.Rfc,
                    NombreFiscal = dto.NombreFiscal,
                    CodigoPostalFiscal = dto.CodigoPostalFiscal,
                    RegimenFiscal = dto.RegimenFiscal,
                    UsoCfdiDefault = dto.UsoCfdiDefault,
                    Email = dto.Email,
                    Activo = true
                };
                _context.ReceptoresFiscales.Add(receptor);
            }
            else
            {
                // Actualizar
                receptor.Rfc = dto.Rfc;
                receptor.NombreFiscal = dto.NombreFiscal;
                receptor.CodigoPostalFiscal = dto.CodigoPostalFiscal;
                receptor.RegimenFiscal = dto.RegimenFiscal;
                receptor.UsoCfdiDefault = dto.UsoCfdiDefault;
                receptor.Email = dto.Email;
                receptor.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
            return MapToDto(receptor);
        }

        private void ValidarRfc(string rfc)
        {
            // RFC debe ser 12 o 13 caracteres (personas físicas 13, morales 12)
            // Formato simple: 3-4 letras + 6 dígitos + 3 alfanuméricos
            if (string.IsNullOrEmpty(rfc) || (rfc.Length != 12 && rfc.Length != 13))
                throw new BusinessException("RFC debe tener 12 o 13 caracteres", code: "RFC_INVALIDO");

            var rfcRegex = new System.Text.RegularExpressions.Regex(@"^[A-ZÑ&]{3,4}\d{6}[A-Z0-9]{3}$");
            if (!rfcRegex.IsMatch(rfc))
                throw new BusinessException("Formato de RFC inválido", code: "RFC_FORMATO_INVALIDO");
        }

        private ReceptorFiscalDto MapToDto(ReceptorFiscal receptor)
        {
            return new ReceptorFiscalDto
            {
                Id = receptor.Id,
                AlumnoId = receptor.AlumnoId,
                Rfc = receptor.Rfc,
                NombreFiscal = receptor.NombreFiscal,
                CodigoPostalFiscal = receptor.CodigoPostalFiscal,
                RegimenFiscal = receptor.RegimenFiscal,
                UsoCfdiDefault = receptor.UsoCfdiDefault,
                Email = receptor.Email,
                Activo = receptor.Activo,
                CreatedAtUtc = receptor.CreatedAtUtc,
                UpdatedAtUtc = receptor.UpdatedAtUtc
            };
        }
    }
}
