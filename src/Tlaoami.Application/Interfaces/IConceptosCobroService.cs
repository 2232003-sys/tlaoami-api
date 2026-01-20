using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    /// <summary>
    /// Servicio de Conceptos de Cobro.
    /// Gestiona el catálogo de conceptos (colegiatura, reinscripción, etc.)
    /// que pueden ser referenciados al generar facturas/cargos.
    /// </summary>
    public interface IConceptosCobroService
    {
        /// <summary>
        /// Obtiene todos los conceptos, opcionalmente filtrados por estado activo.
        /// </summary>
        /// <param name="activo">null = todos; true = solo activos; false = solo inactivos</param>
        /// <returns>Lista de conceptos (vacía si no hay coincidencias)</returns>
        Task<List<ConceptoCobroDto>> GetAllAsync(bool? activo = null);

        /// <summary>
        /// Obtiene un concepto por su ID.
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">Si el concepto no existe</exception>
        Task<ConceptoCobroDto> GetByIdAsync(Guid id);

        /// <summary>
        /// Obtiene un concepto por su Clave (código único).
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">Si no existe concepto con esa clave</exception>
        Task<ConceptoCobroDto> GetByClaveAsync(string clave);

        /// <summary>
        /// Crea un nuevo concepto de cobro.
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.BusinessException">
        /// Si la clave ya existe (código: CLAVE_DUPLICADA, HTTP 409)
        /// Si validación falla (código: VALIDACION_FALLIDA, HTTP 400)
        /// </exception>
        Task<ConceptoCobroDto> CreateAsync(ConceptoCobroCreateDto dto);

        /// <summary>
        /// Actualiza un concepto de cobro (excepto Clave, que es inmutable).
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">Si el concepto no existe</exception>
        /// <exception cref="Tlaoami.Application.Exceptions.BusinessException">Si validación falla</exception>
        Task<ConceptoCobroDto> UpdateAsync(Guid id, ConceptoCobroUpdateDto dto);

        /// <summary>
        /// Inactiva un concepto (soft delete).
        /// Si el concepto ya está inactivo, es un no-op (idempotente).
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">Si el concepto no existe</exception>
        Task InactivateAsync(Guid id);

        /// <summary>
        /// Elimina un concepto de forma permanente (hard delete).
        /// Solo elimina si el concepto no está referenciado por reglas de cobro.
        /// De lo contrario, se recomienda usar InactivateAsync en su lugar.
        /// </summary>
        /// <exception cref="Tlaoami.Application.Exceptions.NotFoundException">Si el concepto no existe</exception>
        /// <exception cref="Tlaoami.Application.Exceptions.BusinessException">
        /// Si está referenciado (código: CONCEPTO_REFERENCIADO, HTTP 409)
        /// </exception>
        Task DeleteAsync(Guid id);
    }
}
