using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Ventas;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain.Enums;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    /// <summary>
    /// Servicio para gestión de órdenes de venta.
    /// Implementa el hook a Finanzas: al confirmar, genera 1 Factura con origen rastreado.
    /// </summary>
    public class OrdenVentaService : IOrdenVentaService
    {
        private readonly TlaoamiDbContext _context;

        public OrdenVentaService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<OrdenVentaDto> CrearOrdenAsync(OrdenVentaCreateDto dto)
        {
            var alumno = await _context.Alumnos.FindAsync(dto.AlumnoId);
            if (alumno == null || !alumno.Activo)
                throw new NotFoundException("ALUMNO_NOT_FOUND", $"Alumno {dto.AlumnoId} no encontrado o inactivo");

            var orden = new OrdenVenta
            {
                Id = Guid.NewGuid(),
                AlumnoId = dto.AlumnoId,
                Fecha = DateTime.UtcNow,
                Estatus = EstatusOrdenVenta.Borrador,
                Total = 0m,
                Notas = dto.Notas,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.OrdenesVenta.Add(orden);
            await _context.SaveChangesAsync();

            return MapToDto(orden);
        }

        public async Task<OrdenVentaDto> AgregarLineaAsync(Guid ordenId, AgregarLineaDto dto)
        {
            var orden = await _context.OrdenesVenta
                .Include(o => o.Lineas)
                .FirstOrDefaultAsync(o => o.Id == ordenId);

            if (orden == null)
                throw new NotFoundException("ORDEN_NOT_FOUND", $"Orden {ordenId} no encontrada");

            if (orden.Estatus != EstatusOrdenVenta.Borrador)
                throw new BusinessException("ORDEN_NO_EDITABLE", "Solo se pueden agregar líneas a órdenes en Borrador");

            var producto = await _context.ConceptosCobro.FindAsync(dto.ProductoId);
            if (producto == null || !producto.Activo)
                throw new NotFoundException("PRODUCTO_NOT_FOUND", $"Producto/concepto {dto.ProductoId} no encontrado o inactivo");

            if (producto.TipoConcepto != TipoConcepto.Producto)
                throw new BusinessException("CONCEPTO_NO_PRODUCTO", "El concepto debe ser de tipo Producto para agregarse a una orden de venta");

            if (dto.Cantidad <= 0)
                throw new ValidationException("Cantidad debe ser mayor a 0", code: "CANTIDAD_INVALIDA");

            // Determinar precio: override o precio del producto (no implementado aún, usar 0 por defecto)
            var precioUnitario = dto.PrecioUnitario ?? 0m;
            if (precioUnitario <= 0m)
                throw new ValidationException("PrecioUnitario debe ser mayor a 0 o definirse en el concepto", code: "PRECIO_INVALIDO");

            var subtotal = dto.Cantidad * precioUnitario;

            var linea = new OrdenVentaLinea
            {
                Id = Guid.NewGuid(),
                OrdenVentaId = ordenId,
                ProductoId = dto.ProductoId,
                Cantidad = dto.Cantidad,
                PrecioUnitario = precioUnitario,
                Subtotal = subtotal,
                CreatedAtUtc = DateTime.UtcNow
            };

            _context.OrdenesVentaLineas.Add(linea);
            orden.Lineas.Add(linea);

            // Recalcular total de la orden
            orden.Total = orden.Lineas.Sum(l => l.Subtotal);
            orden.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Recargar con navegaciones
            await _context.Entry(orden).Collection(o => o.Lineas).LoadAsync();
            foreach (var l in orden.Lineas)
            {
                await _context.Entry(l).Reference(x => x.Producto).LoadAsync();
            }

            return MapToDto(orden);
        }

        public async Task<ConfirmarOrdenResultDto> ConfirmarOrdenAsync(Guid ordenId)
        {
            var orden = await _context.OrdenesVenta
                .Include(o => o.Lineas)
                    .ThenInclude(l => l.Producto)
                .FirstOrDefaultAsync(o => o.Id == ordenId);

            if (orden == null)
                throw new NotFoundException("ORDEN_NOT_FOUND", $"Orden {ordenId} no encontrada");

            if (orden.Estatus == EstatusOrdenVenta.Confirmada)
                throw new BusinessException("ORDEN_YA_CONFIRMADA", "La orden ya está confirmada");

            if (orden.Estatus == EstatusOrdenVenta.Cancelada)
                throw new BusinessException("ORDEN_CANCELADA", "No se puede confirmar una orden cancelada");

            if (!orden.Lineas.Any())
                throw new BusinessException("ORDEN_SIN_LINEAS", "La orden no tiene líneas, no se puede confirmar");

            // === HOOK A FINANZAS: Crear 1 Factura ===
            var siguienteNumero = await ObtenerSiguienteNumeroFacturaAsync();
            var numeroFactura = $"FAC-{siguienteNumero:D6}";

            var factura = new Factura
            {
                Id = Guid.NewGuid(),
                AlumnoId = orden.AlumnoId,
                NumeroFactura = numeroFactura,
                Concepto = $"Orden de Venta #{orden.Id.ToString().Substring(0, 8)}",
                Periodo = null, // Órdenes de venta no tienen periodo
                ConceptoCobroId = null, // Múltiples conceptos en las líneas
                OrigenTipo = OrigenFactura.OrdenVenta,
                OrigenId = orden.Id,
                TipoDocumento = TipoDocumento.Factura,
                Monto = orden.Total,
                FechaEmision = DateTime.UtcNow,
                FechaVencimiento = DateTime.UtcNow.AddDays(30), // Default 30 días
                Estado = EstadoFactura.Pendiente,
                IssuedAt = DateTime.UtcNow,
                Lineas = new System.Collections.Generic.List<FacturaLinea>(),
                Pagos = new System.Collections.Generic.List<Pago>()
            };

            // Crear líneas de factura a partir de líneas de orden
            foreach (var lineaOrden in orden.Lineas)
            {
                factura.Lineas.Add(new FacturaLinea
                {
                    Id = Guid.NewGuid(),
                    Factura = factura,
                    ConceptoCobroId = lineaOrden.ProductoId,
                    Descripcion = $"{lineaOrden.Producto?.Nombre ?? "Producto"} x{lineaOrden.Cantidad}",
                    Subtotal = lineaOrden.Subtotal,
                    Descuento = 0m,
                    Impuesto = 0m,
                    CreatedAtUtc = DateTime.UtcNow
                });
            }

            // Recalcular totales de factura
            factura.RecalculateFrom(
                factura.Lineas.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)),
                factura.Pagos ?? System.Linq.Enumerable.Empty<Pago>());

            _context.Facturas.Add(factura);

            // Actualizar orden
            orden.Estatus = EstatusOrdenVenta.Confirmada;
            orden.ConfirmadaAtUtc = DateTime.UtcNow;
            orden.FacturaId = factura.Id;
            orden.UpdatedAtUtc = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return new ConfirmarOrdenResultDto
            {
                OrdenVentaId = orden.Id,
                FacturaId = factura.Id,
                TotalFacturado = factura.Monto,
                Message = $"Orden confirmada. Factura {factura.NumeroFactura} generada."
            };
        }

        public async Task<OrdenVentaDto> GetOrdenAsync(Guid ordenId)
        {
            var orden = await _context.OrdenesVenta
                .Include(o => o.Lineas)
                    .ThenInclude(l => l.Producto)
                .FirstOrDefaultAsync(o => o.Id == ordenId);

            if (orden == null)
                throw new NotFoundException("ORDEN_NOT_FOUND", $"Orden {ordenId} no encontrada");

            return MapToDto(orden);
        }

        private OrdenVentaDto MapToDto(OrdenVenta orden)
        {
            return new OrdenVentaDto
            {
                Id = orden.Id,
                AlumnoId = orden.AlumnoId,
                Fecha = orden.Fecha,
                Estatus = orden.Estatus.ToString(),
                Total = orden.Total,
                Notas = orden.Notas,
                FacturaId = orden.FacturaId,
                CreatedAtUtc = orden.CreatedAtUtc,
                ConfirmadaAtUtc = orden.ConfirmadaAtUtc,
                Lineas = orden.Lineas?.Select(l => new OrdenVentaLineaDto
                {
                    Id = l.Id,
                    ProductoId = l.ProductoId,
                    ProductoNombre = l.Producto?.Nombre ?? "Producto",
                    Cantidad = l.Cantidad,
                    PrecioUnitario = l.PrecioUnitario,
                    Subtotal = l.Subtotal
                }).ToList() ?? new System.Collections.Generic.List<OrdenVentaLineaDto>()
            };
        }

        private async Task<int> ObtenerSiguienteNumeroFacturaAsync()
        {
            var ultima = await _context.Facturas
                .OrderByDescending(f => f.NumeroFactura)
                .Select(f => f.NumeroFactura)
                .FirstOrDefaultAsync();

            var siguiente = 1;
            if (!string.IsNullOrEmpty(ultima) && ultima!.StartsWith("FAC-"))
            {
                var parte = ultima.Substring(4);
                if (int.TryParse(parte, out var numero))
                    siguiente = numero + 1;
            }
            return siguiente;
        }
    }
}
