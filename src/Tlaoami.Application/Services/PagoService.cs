using Tlaoami.Application.Dtos;
using Tlaoami.Application.Interfaces;
using Tlaoami.Application.Mappers;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Tlaoami.Application.Services;

public class PagoService : IPagoService
{
    private readonly TlaoamiDbContext _context;

    public PagoService(TlaoamiDbContext context)
    {
        _context = context;
    }

    public async Task<(PagoDto pago, bool created)> RegistrarPagoAsync(PagoCreateDto pagoCreateDto)
    {
        if (string.IsNullOrWhiteSpace(pagoCreateDto.IdempotencyKey))
            throw new ArgumentException("IdempotencyKey es requerido");
        if (pagoCreateDto.IdempotencyKey.Length > 128)
            throw new ArgumentException("IdempotencyKey supera la longitud máxima de 128 caracteres");
        if (pagoCreateDto.IdempotencyKey.Length < 8)
            throw new ArgumentException("IdempotencyKey debe tener al menos 8 caracteres");

        var existing = await _context.Pagos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdempotencyKey == pagoCreateDto.IdempotencyKey);
        if (existing != null)
        {
            return (MappingFunctions.ToPagoDto(existing), false);
        }

        var factura = await _context.Facturas
            .Include(f => f.Pagos)
            .FirstOrDefaultAsync(f => f.Id == pagoCreateDto.FacturaId);
        if (factura == null)
        {
            throw new Tlaoami.Application.Exceptions.NotFoundException("Factura no encontrada", code: "FACTURA_NO_ENCONTRADA");
        }

        if (factura.Estado == EstadoFactura.Pagada)
        {
            throw new Tlaoami.Application.Exceptions.ValidationException("La factura ya ha sido pagada.", code: "FACTURA_YA_PAGADA");
        }

        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            FacturaId = pagoCreateDto.FacturaId,
            IdempotencyKey = pagoCreateDto.IdempotencyKey,
            Monto = pagoCreateDto.Monto,
            FechaPago = pagoCreateDto.FechaPago.Kind == DateTimeKind.Unspecified 
                ? DateTime.SpecifyKind(pagoCreateDto.FechaPago, DateTimeKind.Utc) 
                : pagoCreateDto.FechaPago.ToUniversalTime(),
            Metodo = (MetodoPago)Enum.Parse(typeof(MetodoPago), pagoCreateDto.Metodo, true)
        };

        _context.Pagos.Add(pago);
        factura.Pagos.Add(pago);
        factura.RecalculateFrom(null, factura.Pagos);
        try
        {
            await _context.SaveChangesAsync();
            return (MappingFunctions.ToPagoDto(pago), true);
        }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex))
        {
            // Concurrencia: otro proceso grabó el mismo (FacturaId, IdempotencyKey); recupera el existente y responde 200.
            _context.Entry(pago).State = EntityState.Detached;
            var concurrent = await _context.Pagos.AsNoTracking()
                .FirstOrDefaultAsync(p => p.IdempotencyKey == pagoCreateDto.IdempotencyKey);
            if (concurrent != null)
                return (MappingFunctions.ToPagoDto(concurrent), false);
            throw;
        }
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
    {
        var inner = ex.InnerException;
        if (inner == null) return false;

        var typeName = inner.GetType().Name;
        
        // SQLite via reflection
        if (typeName == "SqliteException")
        {
            var errorCodeProp = inner.GetType().GetProperty("SqliteErrorCode");
            var errorCode = errorCodeProp?.GetValue(inner);
            if (errorCode != null && (int)errorCode == 19 && inner.Message.Contains("UNIQUE"))
                return true;
        }

        // PostgreSQL via reflection
        if (typeName == "PostgresException")
        {
            var sqlStateProp = inner.GetType().GetProperty("SqlState");
            var sqlState = sqlStateProp?.GetValue(inner) as string;
            if (sqlState == "23505") return true;
        }
        
        return false;
    }

    public async Task<IEnumerable<PagoDto>> GetPagosByFacturaIdAsync(Guid facturaId)
    {
        var pagos = await _context.Pagos
            .Where(p => p.FacturaId == facturaId)
            .OrderByDescending(p => p.FechaPago)
            .ToListAsync();

        return pagos.Select(MappingFunctions.ToPagoDto);
    }

    public async Task<PagoDto?> GetPagoByIdAsync(Guid id)
    {
        var pago = await _context.Pagos.FindAsync(id);
        
        if (pago == null)
        {
            return null;
        }

        return MappingFunctions.ToPagoDto(pago);
    }
}
