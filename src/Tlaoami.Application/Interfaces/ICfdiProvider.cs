using System;
using System.Threading.Tasks;
using Tlaoami.Application.Dtos;

namespace Tlaoami.Application.Interfaces
{
    public interface ICfdiProvider
    {
        /// <summary>
        /// Timbra un CFDI y retorna los datos de timbrado
        /// </summary>
        Task<CfdiResult> TimbrarAsync(CfdiRequest request);
    }

    public class CfdiRequest
    {
        public Guid FacturaId { get; set; }
        public decimal Monto { get; set; }
        public string? Concepto { get; set; }
        
        // Datos del receptor (tutor/padre)
        public required string ReceptorRfc { get; set; }
        public required string ReceptorNombre { get; set; }
        public required string ReceptorCodigoPostal { get; set; }
        public required string ReceptorRegimen { get; set; }
        
        // Metadata CFDI
        public string? UsoCfdi { get; set; }
        public string? MetodoPago { get; set; }
        public string? FormaPago { get; set; }
        
        // Datos de la escuela (emisor)
        public required string EmisorRfc { get; set; }
        public required string EmisorNombre { get; set; }
        public required string EmisorRegimen { get; set; }
    }

    public class CfdiResult
    {
        public bool Exitoso { get; set; }
        public string? CfdiUuid { get; set; }
        public string? CfdiXmlBase64 { get; set; }
        public string? CfdiPdfBase64 { get; set; }
        public string? ErrorMensaje { get; set; }
        public DateTime TimbradoAtUtc { get; set; }
    }
}
