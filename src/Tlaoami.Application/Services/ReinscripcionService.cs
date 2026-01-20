using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class ReinscripcionService : IReinscripcionService
    {
        private readonly TlaoamiDbContext _context;
        private readonly IAlumnoService _alumnoService;
        private readonly ICicloEscolarService _cicloService;
        private readonly IAsignacionGrupoService _asignacionService;

        public ReinscripcionService(
            TlaoamiDbContext context,
            IAlumnoService alumnoService,
            ICicloEscolarService cicloService,
            IAsignacionGrupoService asignacionService)
        {
            _context = context;
            _alumnoService = alumnoService;
            _cicloService = cicloService;
            _asignacionService = asignacionService;
        }

        public async Task<ReinscripcionResultDto> ReinscribirAsync(ReinscripcionRequestDto dto)
        {
            // Validar existencia de alumno y grupo
            var alumno = await _alumnoService.GetAlumnoByIdAsync(dto.AlumnoId);
            if (alumno == null)
                throw new NotFoundException("Alumno no encontrado", code: "ALUMNO_NO_ENCONTRADO");

            var grupo = await _context.Grupos
                .Include(g => g.CicloEscolar)
                .FirstOrDefaultAsync(g => g.Id == dto.GrupoId);
            if (grupo == null)
                throw new NotFoundException("Grupo no encontrado", code: "GRUPO_NO_ENCONTRADO");

            // Obtener ciclo activo
            var cicloActivo = await _cicloService.GetCicloActivoAsync();
            if (cicloActivo == null)
                throw new BusinessException("No existe ciclo activo", code: "CICLO_NO_ACTIVO");

            // Validar que el grupo pertenezca al ciclo activo
            if (grupo.CicloEscolarId != cicloActivo.Id)
                throw new ValidationException("El grupo no pertenece al ciclo activo", code: "GRUPO_FUERA_DE_CICLO");

            // Validar adeudo mediante Estado de Cuenta
            var estadoCuenta = await _alumnoService.GetEstadoCuentaAsync(dto.AlumnoId);
            if (estadoCuenta == null)
                throw new NotFoundException("Estado de cuenta no disponible", code: "ESTADO_CUENTA_NO_DISPONIBLE");
            if (estadoCuenta.SaldoPendiente > 0)
                throw new BusinessException("El alumno tiene adeudo pendiente", code: "ADEUDO_PENDIENTE");

            // Reutilizar AsignarAlumnoAGrupoAsync para cerrar asignación actual y crear nueva de forma atómica
            var fechaInicio = DateTime.UtcNow;
            await _asignacionService.AsignarAlumnoAGrupoAsync(new AsignarAlumnoGrupoDto
            {
                AlumnoId = dto.AlumnoId,
                GrupoId = dto.GrupoId,
                FechaInicio = fechaInicio
            });

            return new ReinscripcionResultDto
            {
                AlumnoId = dto.AlumnoId,
                GrupoId = dto.GrupoId,
                CicloId = cicloActivo.Id,
                Status = "REINSCRITO"
            };
        }
    }
}
