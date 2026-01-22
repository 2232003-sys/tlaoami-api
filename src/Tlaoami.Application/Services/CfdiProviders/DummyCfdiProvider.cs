using System;
using System.Text;
using System.Threading.Tasks;
using Tlaoami.Application.Interfaces;

namespace Tlaoami.Application.Services.CfdiProviders
{
    public class DummyCfdiProvider : ICfdiProvider
    {
        public async Task<CfdiResult> TimbrarAsync(CfdiRequest request)
        {
            // Simular delay de red
            await Task.Delay(100);

            // Generar UUID dummy (v4 format)
            var dummyUuid = Guid.NewGuid().ToString();

            // Generar XML placeholder
            var xmlContent = GenerarXmlDummy(request, dummyUuid);
            var xmlBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(xmlContent));

            // Generar PDF placeholder (simple texto en base64)
            var pdfContent = GenerarPdfDummy(request);
            var pdfBase64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(pdfContent));

            return new CfdiResult
            {
                Exitoso = true,
                CfdiUuid = dummyUuid,
                CfdiXmlBase64 = xmlBase64,
                CfdiPdfBase64 = pdfBase64,
                TimbradoAtUtc = DateTime.UtcNow,
                ErrorMensaje = null
            };
        }

        private string GenerarXmlDummy(CfdiRequest request, string uuid)
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
<!-- CFDI DUMMY - No validado -->
<Comprobante xmlns=""http://www.sat.gob.mx/cfd/4"" 
             xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance""
             Version=""4.0"" 
             UUID=""{uuid}""
             Folio=""{request.FacturaId}""
             Fecha=""{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}""
             Serie=""FAC"">
  
  <Emisor Rfc=""{request.EmisorRfc}"" 
          Nombre=""{request.EmisorNombre}""
          RegimenFiscal=""{request.EmisorRegimen}"" />
  
  <Receptor Rfc=""{request.ReceptorRfc}""
            Nombre=""{request.ReceptorNombre}""
            ResidenciaFiscal="""" 
            UsoCFDI=""{request.UsoCfdi ?? "P0000000"}"" />
  
  <Conceptos>
    <Concepto Clave=""84111506""
              Cantidad=""1""
              ClaveUnidad=""H87""
              Descripcion=""{request.Concepto ?? "Concepto de cobro"}""
              ValorUnitario=""{request.Monto}""
              Importe=""{request.Monto}"" />
  </Conceptos>
  
  <Impuestos TotalImpuestosRetenidos=""0""
             TotalImpuestosTrasladados=""0""
             Monto=""{request.Monto}"" />
  
  <!-- Timbrado Dummy: Este archivo NO es válido ante SAT -->
</Comprobante>";
        }

        private string GenerarPdfDummy(CfdiRequest request)
        {
            return $@"FACTURA TIMBRADA - DUMMY PROVIDER
=====================================
Folio: {request.FacturaId}
Fecha: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}

EMISOR:
{request.EmisorNombre}
RFC: {request.EmisorRfc}

RECEPTOR (TUTOR/PADRE):
{request.ReceptorNombre}
RFC: {request.ReceptorRfc}
CP: {request.ReceptorCodigoPostal}

CONCEPTO:
{request.Concepto ?? "Concepto de cobro"}
Monto: ${request.Monto:N2}

USO CFDI: {request.UsoCfdi ?? "P0000000"}
Método Pago: {request.MetodoPago ?? "PUE"}
Forma Pago: {request.FormaPago ?? "01"}

NOTA: Este es un CFDI DUMMY generado para desarrollo.
No es válido ante el SAT. Use Facturama para producción.
";
        }
    }
}
