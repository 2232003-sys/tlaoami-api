using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tlaoami.API.Dtos;
using Tlaoami.Domain.Entities;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlumnosController : ControllerBase
    {
        private readonly TlaoamiDbContext _context;

        public AlumnosController(TlaoamiDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AlumnoDto>>> GetAlumnos()
        {
            var alumnos = await _context.Alumnos
                .Include(a => a.Facturas)
                    .ThenInclude(f => f.Pagos)
                .ToListAsync();

            var alumnosDto = alumnos.Select(a => new AlumnoDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Apellido = a.Apellido,
                Email = a.Email,
                Facturas = a.Facturas.Select(f => new FacturaDto
                {
                    Id = f.Id,
                    NumeroFactura = f.NumeroFactura,
                    Monto = f.Monto,
                    FechaEmision = f.FechaEmision,
                    FechaVencimiento = f.FechaVencimiento,
                    Estado = f.Estado.ToString(),
                    Pagos = f.Pagos.Select(p => new PagoDto
                    {
                        Id = p.Id,
                        Monto = p.Monto,
                        FechaPago = p.FechaPago,
                        Metodo = p.Metodo.ToString()
                    }).ToList()
                }).ToList()
            }).ToList();

            return Ok(alumnosDto);
        }
    }
}
