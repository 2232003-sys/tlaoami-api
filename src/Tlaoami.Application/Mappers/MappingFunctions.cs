using Tlaoami.Application.Dtos;
using Tlaoami.Domain.Entities;
using System.Linq;
using System.Collections.Generic;

namespace Tlaoami.Application.Mappers
{
    public static class MappingFunctions
    {
        public static AlumnoDto ToAlumnoDto(Alumno alumno)
        {
            return new AlumnoDto
            {
                Id = alumno.Id,
                Matricula = alumno.Matricula,
                Nombre = alumno.Nombre,
                Apellido = alumno.Apellido,
                Email = alumno.Email,
                Telefono = alumno.Telefono,
                Activo = alumno.Activo,
                FechaInscripcion = alumno.FechaInscripcion
            };
        }

        public static FacturaDto ToFacturaDto(Factura factura)
        {
            var totalPagado = factura.Pagos?.Sum(p => p.Monto) ?? 0;
            return new FacturaDto
            {
                Id = factura.Id,
                AlumnoId = factura.AlumnoId,
                NumeroFactura = factura.NumeroFactura,
                Concepto = factura.Concepto,
                Monto = factura.Monto,
                Saldo = factura.Monto - totalPagado,
                FechaEmision = factura.FechaEmision,
                FechaVencimiento = factura.FechaVencimiento,
                Estado = factura.Estado.ToString()
            };
        }

        public static PagoDto ToPagoDto(Pago pago)
        {
            return new PagoDto
            {
                Id = pago.Id,
                FacturaId = pago.FacturaId,
                AlumnoId = pago.AlumnoId,
                IdempotencyKey = pago.IdempotencyKey,
                Monto = pago.Monto,
                FechaPago = pago.FechaPago,
                Metodo = pago.Metodo.ToString()
            };
        }

        public static FacturaDetalleDto ToFacturaDetalleDto(Factura factura)
        {
            var totalPagado = factura.Pagos?.Sum(p => p.Monto) ?? 0;
            return new FacturaDetalleDto
            {
                Id = factura.Id,
                AlumnoId = factura.AlumnoId,
                AlumnoNombreCompleto = factura.Alumno != null 
                    ? $"{factura.Alumno.Nombre} {factura.Alumno.Apellido}" 
                    : null,
                NumeroFactura = factura.NumeroFactura,
                Concepto = factura.Concepto,
                Monto = factura.Monto,
                Saldo = factura.Monto - totalPagado,
                TotalPagado = totalPagado,
                FechaEmision = factura.FechaEmision,
                FechaVencimiento = factura.FechaVencimiento,
                Estado = factura.Estado.ToString(),
                IssuedAt = factura.IssuedAt,
                CanceledAt = factura.CanceledAt,
                CancelReason = factura.CancelReason,
                Pagos = factura.Pagos?.Select(ToPagoDto).ToList() ?? new List<PagoDto>()
            };
        }

        public static EstadoCuentaDto ToEstadoCuentaDto(Alumno alumno)
        {
            var facturasPagadas = alumno.Facturas.Where(f => f.Estado == EstadoFactura.Pagada).ToList();
            var facturasPendientes = alumno.Facturas.Where(f => f.Estado != EstadoFactura.Pagada).ToList();

            var totalFacturado = alumno.Facturas.Sum(f => f.Monto);
            var totalPagado = alumno.Facturas.SelectMany(f => f.Pagos ?? Enumerable.Empty<Pago>()).Sum(p => p.Monto);

            return new EstadoCuentaDto
            {
                AlumnoId = alumno.Id,
                NombreCompleto = $"{alumno.Nombre} {alumno.Apellido}",
                TotalFacturado = totalFacturado,
                TotalPagado = totalPagado,
                SaldoPendiente = totalFacturado - totalPagado,
                FacturasPagadas = facturasPagadas.Select(f => ToFacturaDto(f)).ToList(),
                FacturasPendientes = facturasPendientes.Select(f => ToFacturaDto(f)).ToList()
            };
        }
    }
}
