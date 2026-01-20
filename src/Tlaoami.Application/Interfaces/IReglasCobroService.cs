using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    /// <summary>
    /// Servicio de Reglas de Cobro por Ciclo.
    /// Define cuándo y cómo se cobran conceptos en ciclos específicos.
    /// NO genera facturas/cargos automáticamente.
    /// </summary>
    public interface IReglasCobroService
    {
        /// <summary>
        /// Obtiene todas las reglas, opcionalmente filtradas.
        /// </summary>
        /// <param name="cicloId">Filtrar por ciclo (opcional)</param>
        /// <param name="grado">Filtrar por grado (opcional)</param>
        /// <param name="activa">null = todos; true = solo activas; false = solo inactivas</param>
        Task<List<ReglaCobroDto>> GetAllAsync(Guid? cicloId = null, int? grado = null, bool? activa = null);

        /// <summary>Obtiene una regla por su ID</summary>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">Si la regla no existe</exception>
        Task<ReglaCobroDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene reglas para un ciclo específico.
        /// </summary>
        Task<List<ReglaCobroDto>> GetByCicloAsync(Guid cicloId, bool? activa = null);

        /// <summary>
        /// Crea una nueva regla de cobro.
        /// Valida que el ciclo y concepto existan.
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">Si ciclo o concepto no existen</exception>
        /// <exception cref="Tlaoami.Application.Exceptions.BusinessException">
        /// Si validación falla (código: REGLA_INVALIDA, HTTP 400)
        /// Si ya existe una regla con la misma clave lógica (código: REGLA_DUPLICADA, HTTP 409)
        /// </exception>
        Task<ReglaCobroDto> CreateAsync(ReglaCobroCreateDto dto);

        /// <summary>
        /// Actualiza una regla de cobro.
        /// Los campos CicloId y ConceptoCobroId son inmutables.
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">Si la regla no existe</exception>
        /// <exception cref="Tlaoami.Application.Exceptions.BusinessException">Si validación falla</exception>
        Task<ReglaCobroDto> UpdateAsync(Guid id, ReglaCobroUpdateDto dto);

        /// <summary>
        /// Inactiva una regla (soft delete).
        /// Si ya está inactiva, es un no-op (idempotente).
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">Si la regla no existe</exception>
        Task InactivateAsync(Guid id);

        /// <summary>
        /// Elimina una regla de forma permanente (hard delete).
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">Si la regla no existe</exception>
        Task DeleteAsync(Guid id);
    }
}
