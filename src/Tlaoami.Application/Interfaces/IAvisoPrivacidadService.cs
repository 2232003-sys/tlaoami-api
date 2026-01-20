using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    /// <summary>
    /// Servicio de Avisos de Privacidad y Cumplimiento normativo.
    /// Gestiona publicación de avisos y registro de aceptaciones.
    /// </summary>
    public interface IAvisoPrivacidadService
    {
        /// <summary>
        /// Obtiene el aviso vigente actualmente.
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">
        /// Si no hay aviso vigente (código: AVISO_NO_VIGENTE)
        /// </exception>
        Task<AvisoPrivacidadDto> GetAvisoVigenteAsync();

        /// <summary>
        /// Obtiene el estado de aceptación del usuario actual.
        /// Requiere JWT con UsuarioId en claims.
        /// </summary>
        /// <returns>
        /// EstadoAceptacionDto con RequiereAceptacion y timestamps.
        /// </returns>
        Task<EstadoAceptacionDto> GetEstadoAceptacionAsync(Guid usuarioId);

        /// <summary>
        /// Registra la aceptación del aviso vigente por parte del usuario.
        /// Idempotente: si ya aceptó este aviso, retorna 200 sin duplicar.
        /// </summary>
        /// <param name="usuarioId">Usuario que acepta</param>
        /// <param name="ip">IP del cliente (opcional)</param>
        /// <param name="userAgent">User-Agent del navegador (opcional)</param>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">
        /// Si no hay aviso vigente (código: AVISO_NO_VIGENTE)
        /// </exception>
        Task<EstadoAceptacionDto> AceptarAvisoAsync(Guid usuarioId, string? ip = null, string? userAgent = null);

        /// <summary>
        /// Publica un nuevo aviso de privacidad (Admin only).
        /// Desactiva el aviso vigente anterior y marca este como vigente.
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.ValidationException">
        /// Si validación falla (código: AVISO_INVALIDO)
        /// </exception>
        Task<AvisoPrivacidadDto> PublicarAvisoAsync(AvisoPrivacidadCreateDto dto);

        /// <summary>
        /// Verifica si un usuario ha aceptado el aviso vigente.
        /// Útil para middleware/filters de cumplimiento.
        /// </summary>
        Task<bool> UsuarioHaAceptadoVigenteAsync(Guid usuarioId);
    }
}
