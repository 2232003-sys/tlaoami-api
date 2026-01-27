using System;
using System.Threading.Tasks;
using Tlaoami.Application.Ventas;

namespace Tlaoami.Application.Interfaces
{
    /// <summary>
    /// Servicio para gestión de órdenes de venta de productos/servicios.
    /// Al confirmar una orden, se genera automáticamente un cargo financiero (Factura).
    /// </summary>
    public interface IOrdenVentaService
    {
        /// <summary>
        /// Crea una nueva orden de venta en estado Borrador.
        /// </summary>
        Task<OrdenVentaDto> CrearOrdenAsync(OrdenVentaCreateDto dto);

        /// <summary>
        /// Agrega una línea de producto/servicio a una orden existente.
        /// Solo permite agregar líneas si la orden está en Borrador.
        /// </summary>
        Task<OrdenVentaDto> AgregarLineaAsync(Guid ordenId, AgregarLineaDto dto);

        /// <summary>
        /// Confirma la orden y genera UN cargo financiero (Factura).
        /// Transición: Borrador → Confirmada.
        /// Hook a Finanzas: crea Factura con OrigenTipo=OrdenVenta y OrigenId=OrdenVentaId.
        /// </summary>
        Task<ConfirmarOrdenResultDto> ConfirmarOrdenAsync(Guid ordenId);

        /// <summary>
        /// Obtiene una orden por ID con sus líneas.
        /// </summary>
        Task<OrdenVentaDto> GetOrdenAsync(Guid ordenId);
    }
}
