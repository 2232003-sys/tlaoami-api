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
            var config = new CsvConfiguration(cultura) 
            { 
                HasHeaderRecord = true,
                IgnoreBlankLines = true
            };

            List<MovimientoCsvRow> movimientosParseados;
            using (var reader = new StreamReader(archivoCsv.OpenReadStream()))
            using (var csv = new CsvReader(reader, config))
            {
                // Lee encabezados y normaliza (remueve acentos)
                csv.Read();
                csv.ReadHeader();
                var headersRaw = csv.HeaderRecord;
                var headersNormalized = headersRaw?.Select(h => RemoveAccents(h ?? "").Trim()).ToArray() ?? Array.Empty<string>();
                
                var registrosCrudos = new List<MovimientoCsvRawRow>();
                while (csv.Read())
                {
                    registrosCrudos.Add(new MovimientoCsvRawRow
                    {
                        Fecha = GetFieldNormalized(csv, headersNormalized, "Fecha"),
                        Descripcion = GetFieldNormalized(csv, headersNormalized, "Descripcion"),
                        Deposito = GetFieldNormalized(csv, headersNormalized, "Depositos"),
                        Retiro = GetFieldNormalized(csv, headersNormalized, "Retiros"),
                        Saldo = GetFieldNormalized(csv, headersNormalized, "Saldo")
                    });
                }

                movimientosParseados = registrosCrudos.Select(row => new MovimientoCsvRow
                {
                    Fecha = DateTime.Parse(row.Fecha, cultura),
                    Descripcion = row.Descripcion?.Trim() ?? string.Empty,
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
                    Saldo = movimientoCsv.Saldo,
                    Tipo = esDeposito ? TipoMovimiento.Deposito : TipoMovimiento.Retiro,
                    Estado = esDeposito ? EstadoConciliacion.NoConciliado : EstadoConciliacion.Ignorado,
                    HashMovimiento = hash
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
            DateTime? hasta,
            int page = 1,
            int pageSize = 50)
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
                .OrderByDescending(m => m.Fecha)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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

        private static string RemoveAccents(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;
            var nfdForm = text.Normalize(NormalizationForm.FormD);
            var result = new StringBuilder();
            foreach (var c in nfdForm)
            {
                if (char.GetUnicodeCategory(c) != System.Globalization.UnicodeCategory.NonSpacingMark)
                    result.Append(c);
            }
            return result.ToString();
        }

        private static string GetFieldNormalized(CsvReader csv, string[] normalizedHeaders, string fieldName)
        {
            var normalizedField = RemoveAccents(fieldName).Trim();
            var idx = Array.FindIndex(normalizedHeaders, h => h.Equals(normalizedField, StringComparison.OrdinalIgnoreCase));
            return idx >= 0 ? (csv[idx] ?? string.Empty) : string.Empty;
        }

        private static decimal? ParseCurrency(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            // Limpiar: remover $, espacios, comas, paréntesis, etc.
            var cleaned = value.Trim();
            cleaned = Regex.Replace(cleaned, @"[$,\s]", "");
            
            // Detectar negativos por paréntesis
            var isNegative = cleaned.StartsWith("(") && cleaned.EndsWith(")");
            if (isNegative)
            {
                cleaned = cleaned.Substring(1, cleaned.Length - 2);
            }

            if (decimal.TryParse(cleaned, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var result))
            {
                return isNegative ? -result : result;
            }

            return null;
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
            public string Fecha { get; set; } = string.Empty;
            public string Descripcion { get; set; } = string.Empty;
            public string Deposito { get; set; } = string.Empty;
            public string Retiro { get; set; } = string.Empty;
            public string Saldo { get; set; } = string.Empty;
        }

        private class MovimientoCsvRow
        {
            public DateTime Fecha { get; set; }
            public string Descripcion { get; set; } = string.Empty;
            public decimal? Deposito { get; set; }
            public decimal? Retiro { get; set; }
            public decimal Saldo { get; set; }
        }
    }
}
