using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Tlaoami.Domain.Enums;

namespace Tlaoami.Domain.Entities
{
    public class Factura
    {
        public Guid Id { get; set; }
        public Guid AlumnoId { get; set; }
        public Alumno? Alumno { get; set; }

        [Required]
        public string? NumeroFactura { get; set; }

        [Required]
        [StringLength(500)]
        public string Concepto { get; set; } = string.Empty;

        [StringLength(7)]
        public string? Periodo { get; set; }

        public Guid? ConceptoCobroId { get; set; }
        public ConceptoCobro? ConceptoCobro { get; set; }

        public TipoDocumento TipoDocumento { get; set; } = TipoDocumento.Factura;

        public string? ReciboFolio { get; set; }
        public DateTime? ReciboEmitidoAtUtc { get; set; }
        
        public decimal Monto { get; set; }
        public DateTime FechaEmision { get; set; }
        public DateTime FechaVencimiento { get; set; }

        // Estado de la factura (flujo de emisión/pago/cancelación)
        [Required]
        public EstadoFactura Estado { get; set; } = EstadoFactura.Borrador;

        // Timestamps de acciones de negocio
        public DateTime? IssuedAt { get; set; }
        public DateTime? CanceledAt { get; set; }
        public string? CancelReason { get; set; }
        public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
        public ICollection<FacturaLinea> Lineas { get; set; } = new List<FacturaLinea>();

        /// <summary>
        /// Recalcula totales y estado a partir de líneas (opcionales) y pagos confirmados.
        /// Total = Subtotal - Descuentos + Impuestos. Si no hay líneas, se usa Monto actual como Total.
        /// PaidAmount = suma de pagos válidos/confirmados (modelo actual: todos los Pagos son confirmados).
        /// Balance = Total - PaidAmount con tolerancia 0.01m.
        /// Reglas de estado (mapea Issued->Pendiente, PartiallyPaid->ParcialmentePagada, Paid->Pagada):
        /// - Si Cancelada, permanece Cancelada.
        /// - Si PaidAmount <= 0 y la factura está emitida => Pendiente (Issued).
        /// - Si 0 < PaidAmount < Total => ParcialmentePagada.
        /// - Si PaidAmount >= Total => Pagada.
        /// Además, si está sin pagar y vencida, queda Vencida.
        /// </summary>
        public void RecalculateFrom(IEnumerable<FacturaRecalcLine>? lines, IEnumerable<Pago> payments, decimal tolerance = 0.01m)
        {
            var recalcLines = (lines ?? Lineas?.Select(l => new FacturaRecalcLine(l.Subtotal, l.Descuento, l.Impuesto)) ?? Enumerable.Empty<FacturaRecalcLine>()).ToList();
            decimal subtotal = 0m, descuentos = 0m, impuestos = 0m;
            if (recalcLines.Any())
            {
                foreach (var l in recalcLines)
                {
                    subtotal += l.Subtotal;
                    descuentos += l.Descuento;
                    impuestos += l.Impuesto;
                }
            }

            var total = recalcLines.Any()
                ? subtotal - descuentos + impuestos
                : Monto; // respetar modelo actual si no hay líneas

            if (total < 0) total = 0; // robustez

            // En el modelo actual, todos los pagos persistidos son confirmados/válidos
            var paidAmount = payments?.Where(p => p != null && p.Monto > 0m).Sum(p => p.Monto) ?? 0m;

            // Tolerancia
            var balanced = Math.Abs(total - paidAmount) <= tolerance;

            // Si definimos líneas, reflejar el total en Monto (total de la factura)
            if (recalcLines.Any())
            {
                Monto = total;
            }

            // Estados
            if (Estado == EstadoFactura.Cancelada)
            {
                return; // inmutable si cancelada
            }

            // Determinar emitida: IssuedAt o estado distinto a Borrador
            var emitida = IssuedAt.HasValue || (Estado != EstadoFactura.Borrador);

            if (balanced || paidAmount >= total)
            {
                Estado = EstadoFactura.Pagada;
                return;
            }

            if (paidAmount > 0m && paidAmount < total)
            {
                Estado = EstadoFactura.ParcialmentePagada;
                return;
            }

            // Sin pagos
            if (emitida)
            {
                // Vencida si pasó fecha de vencimiento
                if (DateTime.UtcNow.Date > FechaVencimiento.Date)
                {
                    Estado = EstadoFactura.Vencida;
                }
                else
                {
                    Estado = EstadoFactura.Pendiente; // Issued
                }
            }
            // Si no emitida, permanece tal cual (Borrador)
        }
    }

    public enum EstadoFactura
    {
        // Mantener valores originales para compatibilidad con datos existentes
        Pendiente = 0,
        ParcialmentePagada = 1,
        Pagada = 2,
        Vencida = 3,
        Cancelada = 4,
        Borrador = 5
    }

    public enum TipoDocumento
    {
        Factura = 0,
        Recibo = 1
    }

    /// <summary>
    /// Línea simple para recálculo; no está mapeada a BD.
    /// </summary>
    public readonly record struct FacturaRecalcLine(decimal Subtotal, decimal Descuento, decimal Impuesto);
}
