using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Tlaoami.Application.Contracts;
using Tlaoami.Application.Interfaces;
using Tlaoami.Domain.Entities;
using Tlaoami.Domain.Enumerations;
using Tlaoami.Infrastructure;

namespace Tlaoami.Application.Services
{
    public class ImportacionEstadoCuentaService : IImportacionEstadoCuentaService
    {
        private readonly TlaoamiDbContext _context;

        public ImportacionEstadoCuentaService(TlaoamiDbContext context)
        {
            _context = context;
        }

        public async Task<ImportacionResultadoDto> ImportarAsync(IFormFile archivoCsv)
        {
            var resultado = new ImportacionResultadoDto();
            var cultura = new CultureInfo("es-MX");
            var config = new CsvConfiguration(cultura) { HasHeaderRecord = true };

            List<MovimientoCsvRow> movimientosParseados;
            using (var reader = new StreamReader(archivoCsv.OpenReadStream()))
            using (var csv = new CsvReader(reader, config))
            {
                var registrosCrudos = csv.GetRecords<MovimientoCsvRawRow>().ToList();
                movimientosParseados = registrosCrudos.Select(row => new MovimientoCsvRow
                {
                    Fecha = DateTime.Parse(row.Fecha, cultura),
                    Descripcion = row.Descripcion?.Trim(),
                    Deposito = ParseCurrency(row.Deposito),
                    Retiro = ParseCurrency(row.Retiro),
                    Saldo = ParseCurrency(row.Saldo) ?? 0m
                }).ToList();
            }

            var todosLosHashes = await _context.MovimientosBancarios.Select(m => m.HashMovimiento).ToListAsync();
            var hashesEnDb = new HashSet<string>(todosLosHashes);

            foreach (var movimientoCsv in movimientosParseados)
            {
                var monto = movimientoCsv.Deposito ?? movimientoCsv.Retiro;
                if (!monto.HasValue || string.IsNullOrWhiteSpace(movimientoCsv.Descripcion))
                {
                    resultado.Omitidos++;
                    continue;
                }

                var hash = GenerarHash(movimientoCsv.Fecha, monto.Value, movimientoCsv.Descripcion);

                if (hashesEnDb.Contains(hash))
                {
                    resultado.Omitidos++;
                    continue;
                }

                var esDeposito = movimientoCsv.Deposito.HasValue;
                var nuevoMovimiento = new MovimientoBancario
                {
                    Fecha = movimientoCsv.Fecha,
                    Descripcion = movimientoCsv.Descripcion,
                    Monto = monto.Value,
                    SaldoAlMomento = movimientoCsv.Saldo,
                    Tipo = esDeposito ? TipoMovimiento.Deposito : TipoMovimiento.Retiro,
                    Estado = esDeposito ? EstadoConciliacion.NoConciliado : EstadoConciliacion.Ignorado,
                    HashMovimiento = hash,
                    FechaCreacion = DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow
                };

                await _context.MovimientosBancarios.AddAsync(nuevoMovimiento);
                hashesEnDb.Add(hash); 

                resultado.MovimientosImportados++;
                if (esDeposito) resultado.Depositos++;
                else resultado.Retiros++;
            }

            await _context.SaveChangesAsync();

            return resultado;
        }

        public async Task<IEnumerable<MovimientoBancarioDto>> GetMovimientosBancariosAsync(
            EstadoConciliacion? estado,
            TipoMovimiento? tipo,
            DateTime? desde,
            DateTime? hasta)
        {
            var query = _context.MovimientosBancarios.AsQueryable();

            if (estado.HasValue)
            {
                query = query.Where(m => m.Estado == estado.Value);
            }

            if (tipo.HasValue)
            {
                query = query.Where(m => m.Tipo == tipo.Value);
            }

            if (desde.HasValue)
            {
                query = query.Where(m => m.Fecha >= desde.Value);
            }

            if (hasta.HasValue)
            {
                query = query.Where(m => m.Fecha <= hasta.Value);
            }

            return await query
                .Select(m => new MovimientoBancarioDto
                {
                    Id = m.Id,
                    Fecha = m.Fecha,
                    Descripcion = m.Descripcion,
                    Monto = m.Monto,
                    Tipo = m.Tipo,
                    Estado = m.Estado
                })
                .ToListAsync();
        }

        private decimal? ParseCurrency(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;
            return decimal.Parse(value, NumberStyles.Currency, new CultureInfo("es-MX"));
        }

        private string GenerarHash(DateTime fecha, decimal monto, string descripcion)
        {
            var descripcionNormalizada = Regex.Replace(descripcion.ToUpper().Replace("  ", " "), "[^A-Z0-9 ]", "");
            var input = $"{fecha:yyyyMMdd}{monto:0.00}{descripcionNormalizada}";
            
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                var builder = new StringBuilder();
                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private class MovimientoCsvRawRow
        {
            public string Fecha { get; set; }
            public string Descripcion { get; set; }
            public string Deposito { get; set; }
            public string Retiro { get; set; }
            public string Saldo { get; set; }
        }

        private class MovimientoCsvRow
        {
            public DateTime Fecha { get; set; }
            public string Descripcion { get; set; }
            public decimal? Deposito { get; set; }
            public decimal? Retiro { get; set; }
            public decimal Saldo { get; set; }
        }
    }
}
