using System;

namespace Tlaoami.Domain.Entities
{
    public class FacturaFiscal
    {
        public Guid FacturaId { get; set; } // PK/FK único a Factura
        public required string Proveedor { get; set; } // "Dummy" o "Facturama"
        public required string EstadoTimbrado { get; set; } // "Pendiente", "Timbrado", "Error", "Cancelado"
        
        // CFDI Data
        public string? CfdiUuid { get; set; } // UUID del timbrado (null si Pendiente/Error)
        public string? CfdiXmlBase64 { get; set; } // XML codificado en base64
        public string? CfdiPdfBase64 { get; set; } // PDF codificado en base64
        public DateTime? TimbradoAtUtc { get; set; }
        public string? ErrorTimbrado { get; set; } // Mensaje de error si falló
        
        // Metadata CFDI
        public string? UsoCfdi { get; set; } // "P0000000", "G01000000", etc.
        public string? MetodoPago { get; set; } // "PUE" (Pago en una exhibición), "PPD" (Pago en parcialidades diferidas)
        public string? FormaPago { get; set; } // "01" (Efectivo), "02" (Cheque), "03" (Transferencia), etc.
        
        // Snapshot del receptor al momento del timbrado
        public string? ReceptorRfcSnapshot { get; set; }
        public string? ReceptorNombreSnapshot { get; set; }
        public string? ReceptorCodigoPostalSnapshot { get; set; }
        public string? ReceptorRegimenSnapshot { get; set; }
        
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAtUtc { get; set; }

        // Relationships
        public Factura? Factura { get; set; }
    }
}
