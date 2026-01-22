using System;

namespace Tlaoami.Application.Dtos
{
    public class FacturaFiscalDto
    {
        public Guid FacturaId { get; set; }
        public required string Proveedor { get; set; } // "Dummy", "Facturama"
        public required string EstadoTimbrado { get; set; } // "Pendiente", "Timbrado", "Error", "Cancelado"
        
        // CFDI Data
        public string? CfdiUuid { get; set; }
        public string? CfdiXmlBase64 { get; set; }
        public string? CfdiPdfBase64 { get; set; }
        public DateTime? TimbradoAtUtc { get; set; }
        public string? ErrorTimbrado { get; set; }
        
        // Metadata
        public string? UsoCfdi { get; set; }
        public string? MetodoPago { get; set; }
        public string? FormaPago { get; set; }
        
        // Snapshot del receptor
        public string? ReceptorRfcSnapshot { get; set; }
        public string? ReceptorNombreSnapshot { get; set; }
        public string? ReceptorCodigoPostalSnapshot { get; set; }
        public string? ReceptorRegimenSnapshot { get; set; }
        
        public DateTime CreatedAtUtc { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
    }

    // Request para timbrar
    public class TimbrarCfdiRequest
    {
        public string? UsoCfdi { get; set; } // Override si existe ReceptorFiscal
        public string? MetodoPago { get; set; } // "PUE" (default), "PPD"
        public string? FormaPago { get; set; } // "01", "02", "03", etc.
        public string? Proveedor { get; set; } // "Dummy" (default), "Facturama"
    }

    // Respuesta de faltantes fiscales
    public class FaltantesFiscalesResponse
    {
        public required string[] Faltantes { get; set; }
        public string? Mensaje { get; set; }
    }
}
