using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Configuration;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;
using Microsoft.Extensions.Options;

namespace Tlaoami.Application.Services
{
    public interface IFacturaFiscalService
    {
        Task<FacturaFiscalDto?> GetByFacturaIdAsync(Guid facturaId);
        Task<FacturaFiscalDto> TimbrarAsync(Guid facturaId, TimbrarCfdiRequest? request = null);
        Task<(string xml, string pdf)> DescargarAsync(Guid facturaId);
    }

    public class FacturaFiscalService : IFacturaFiscalService
    {
        private readonly TlaoamiDbContext _context;
        private readonly ICfdiProvider _cfdiProvider;
        private readonly IReceptorFiscalService _receptorFiscalService;
        private readonly EmisorFiscalOptions _emisorOptions;

        public FacturaFiscalService(
            TlaoamiDbContext context,
            ICfdiProvider cfdiProvider,
            IReceptorFiscalService receptorFiscalService,
            IOptions<EmisorFiscalOptions> emisorOptions)
        {
            _context = context;
            _cfdiProvider = cfdiProvider;
            _receptorFiscalService = receptorFiscalService;
            _emisorOptions = emisorOptions.Value;
        }

        public async Task<FacturaFiscalDto?> GetByFacturaIdAsync(Guid facturaId)
        {
            var fiscal = await _context.FacturasFiscales
                .FirstOrDefaultAsync(ff => ff.FacturaId == facturaId);

            if (fiscal == null)
                return null;

            return MapToDto(fiscal);
        }

        public async Task<FacturaFiscalDto> TimbrarAsync(Guid facturaId, TimbrarCfdiRequest? request = null)
        {
            request ??= new TimbrarCfdiRequest();

            // Obtener factura
            var factura = await _context.Facturas
                .Include(f => f.Alumno)
                .FirstOrDefaultAsync(f => f.Id == facturaId);

            if (factura == null)
                throw new NotFoundException("Factura no encontrada", code: "FACTURA_NO_ENCONTRADA");

            // Validar que alumno existe
            if (factura.Alumno == null)
                throw new NotFoundException("Alumno de factura no encontrado", code: "ALUMNO_NO_ENCONTRADO");

            // Verificar si ya existe FacturaFiscal timbrada (idempotencia)
            var existente = await _context.FacturasFiscales
                .FirstOrDefaultAsync(ff => ff.FacturaId == facturaId);

            if (existente != null && existente.EstadoTimbrado == "Timbrado")
            {
                return MapToDto(existente);
            }

            // Obtener receptor fiscal del alumno
            var receptor = await _receptorFiscalService.GetByAlumnoIdAsync(factura.AlumnoId);

            if (receptor == null)
            {
                throw new BusinessException(
                    "Receptor fiscal del alumno no configurado",
                    code: "RECEPTOR_FISCAL_FALTANTE");
            }

            // Resolver valores CFDI
            var usoCfdi = request.UsoCfdi ?? receptor.UsoCfdiDefault ?? "P0000000";
            var metodoPago = request.MetodoPago ?? "PUE";
            var formaPago = request.FormaPago ?? "01";
            var proveedor = request.Proveedor ?? "Dummy";

            // Preparar request para provider
            var cfdiRequest = new CfdiRequest
            {
                FacturaId = facturaId,
                Monto = factura.Monto,
                Concepto = factura.Concepto,
                ReceptorRfc = receptor.Rfc,
                ReceptorNombre = receptor.NombreFiscal,
                ReceptorCodigoPostal = receptor.CodigoPostalFiscal,
                ReceptorRegimen = receptor.RegimenFiscal,
                UsoCfdi = usoCfdi,
                MetodoPago = metodoPago,
                FormaPago = formaPago,
                EmisorRfc = _emisorOptions.Rfc,
                EmisorNombre = _emisorOptions.Nombre,
                EmisorRegimen = _emisorOptions.Regimen
            };

            // Llamar provider
            var cfdiResult = await _cfdiProvider.TimbrarAsync(cfdiRequest);

            // Crear o actualizar FacturaFiscal
            if (existente == null)
            {
                existente = new FacturaFiscal
                {
                    FacturaId = facturaId,
                    Proveedor = proveedor,
                    EstadoTimbrado = cfdiResult.Exitoso ? "Timbrado" : "Error",
                    CfdiUuid = cfdiResult.CfdiUuid,
                    CfdiXmlBase64 = cfdiResult.CfdiXmlBase64,
                    CfdiPdfBase64 = cfdiResult.CfdiPdfBase64,
                    TimbradoAtUtc = cfdiResult.TimbradoAtUtc,
                    ErrorTimbrado = cfdiResult.ErrorMensaje,
                    UsoCfdi = usoCfdi,
                    MetodoPago = metodoPago,
                    FormaPago = formaPago,
                    ReceptorRfcSnapshot = receptor.Rfc,
                    ReceptorNombreSnapshot = receptor.NombreFiscal,
                    ReceptorCodigoPostalSnapshot = receptor.CodigoPostalFiscal,
                    ReceptorRegimenSnapshot = receptor.RegimenFiscal
                };
                _context.FacturasFiscales.Add(existente);
            }
            else
            {
                existente.EstadoTimbrado = cfdiResult.Exitoso ? "Timbrado" : "Error";
                existente.CfdiUuid = cfdiResult.CfdiUuid;
                existente.CfdiXmlBase64 = cfdiResult.CfdiXmlBase64;
                existente.CfdiPdfBase64 = cfdiResult.CfdiPdfBase64;
                existente.TimbradoAtUtc = cfdiResult.TimbradoAtUtc;
                existente.ErrorTimbrado = cfdiResult.ErrorMensaje;
                existente.UpdatedAtUtc = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            if (!cfdiResult.Exitoso)
            {
                throw new BusinessException(
                    cfdiResult.ErrorMensaje ?? "Error al timbrar CFDI",
                    code: "TIMBRADO_ERROR");
            }

            return MapToDto(existente);
        }

        public async Task<(string xml, string pdf)> DescargarAsync(Guid facturaId)
        {
            var fiscal = await _context.FacturasFiscales
                .FirstOrDefaultAsync(ff => ff.FacturaId == facturaId);

            if (fiscal == null)
                throw new NotFoundException("Factura fiscal no encontrada", code: "FACTURA_FISCAL_NO_ENCONTRADA");

            if (fiscal.EstadoTimbrado != "Timbrado")
                throw new BusinessException("La factura aún no ha sido timbrada", code: "FACTURA_NO_TIMBRADA");

            if (string.IsNullOrEmpty(fiscal.CfdiXmlBase64) || string.IsNullOrEmpty(fiscal.CfdiPdfBase64))
                throw new BusinessException("Archivos CFDI no disponibles", code: "ARCHIVOS_NO_DISPONIBLES");

            return (fiscal.CfdiXmlBase64, fiscal.CfdiPdfBase64);
        }

        private FacturaFiscalDto MapToDto(FacturaFiscal fiscal)
        {
            return new FacturaFiscalDto
            {
                FacturaId = fiscal.FacturaId,
                Proveedor = fiscal.Proveedor,
                EstadoTimbrado = fiscal.EstadoTimbrado,
                CfdiUuid = fiscal.CfdiUuid,
                CfdiXmlBase64 = fiscal.CfdiXmlBase64,
                CfdiPdfBase64 = fiscal.CfdiPdfBase64,
                TimbradoAtUtc = fiscal.TimbradoAtUtc,
                ErrorTimbrado = fiscal.ErrorTimbrado,
                UsoCfdi = fiscal.UsoCfdi,
                MetodoPago = fiscal.MetodoPago,
                FormaPago = fiscal.FormaPago,
                ReceptorRfcSnapshot = fiscal.ReceptorRfcSnapshot,
                ReceptorNombreSnapshot = fiscal.ReceptorNombreSnapshot,
                ReceptorCodigoPostalSnapshot = fiscal.ReceptorCodigoPostalSnapshot,
                ReceptorRegimenSnapshot = fiscal.ReceptorRegimenSnapshot,
                CreatedAtUtc = fiscal.CreatedAtUtc,
                UpdatedAtUtc = fiscal.UpdatedAtUtc
            };
        }
    }
}
