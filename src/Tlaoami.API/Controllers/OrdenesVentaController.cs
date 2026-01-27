using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Ventas;
using Tlaoami.Application.Exceptions;

namespace Tlaoami.API.Controllers
{
    /// <summary>
    /// Gestión de órdenes de venta de productos/servicios.
    /// Al confirmar, genera automáticamente un cargo financiero (Factura).
    /// </summary>
    [ApiController]
    [Route("api/v1/ordenes-venta")]
    public class OrdenesVentaController : ControllerBase
    {
        private readonly IOrdenVentaService _service;

        public OrdenesVentaController(IOrdenVentaService service)
        {
            _service = service;
        }

        /// <summary>
        /// POST /ordenes-venta
        /// Crea una nueva orden de venta en estado Borrador.
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<OrdenVentaDto>> CrearOrden([FromBody] OrdenVentaCreateDto dto)
        {
            try
            {
                var orden = await _service.CrearOrdenAsync(dto);
                return CreatedAtAction(nameof(GetOrden), new { id = orden.Id }, orden);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// GET /ordenes-venta/{id}
        /// Obtiene una orden de venta con sus líneas.
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<OrdenVentaDto>> GetOrden(Guid id)
        {
            try
            {
                var orden = await _service.GetOrdenAsync(id);
                return Ok(orden);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /ordenes-venta/{id}/lineas
        /// Agrega una línea de producto/servicio a la orden.
        /// Solo permite agregar si la orden está en Borrador.
        /// </summary>
        [HttpPost("{id}/lineas")]
        public async Task<ActionResult<OrdenVentaDto>> AgregarLinea(Guid id, [FromBody] AgregarLineaDto dto)
        {
            try
            {
                var orden = await _service.AgregarLineaAsync(id, dto);
                return Ok(orden);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message, code = ex.Code });
            }
            catch (BusinessException ex)
            {
                return Conflict(new { error = ex.Message, code = ex.Code });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// POST /ordenes-venta/{id}/confirmar
        /// Confirma la orden y genera UN cargo financiero (Factura).
        /// Hook a Finanzas: OrigenTipo=OrdenVenta, OrigenId=OrdenVentaId.
        /// </summary>
        [HttpPost("{id}/confirmar")]
        public async Task<ActionResult<ConfirmarOrdenResultDto>> ConfirmarOrden(Guid id)
        {
            try
            {
                var result = await _service.ConfirmarOrdenAsync(id);
                return Ok(result);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = ex.Message, code = ex.Code });
            }
            catch (BusinessException ex)
            {
                return Conflict(new { error = ex.Message, code = ex.Code });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
