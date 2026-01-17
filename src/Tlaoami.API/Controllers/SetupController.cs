using Microsoft.AspNetCore.Mvc;
using Tlaoami.Domain.Entities;
using Tlaoami.Infrastructure;
using System;
using System.Threading.Tasks;

namespace Tlaoami.API.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class SetupController : ControllerBase
    {
        private readonly TlaoamiDbContext _context;

        public SetupController(TlaoamiDbContext context)
        {
            _context = context;
        }

        [HttpPost("test-data")]
        public async Task<IActionResult> CreateTestData()
        {
            var alumno = new Alumno { Id = Guid.NewGuid(), Nombre = "Juan", Apellido = "Perez" };
            _context.Alumnos.Add(alumno);
            await _context.SaveChangesAsync();

            var factura = new Factura { AlumnoId = alumno.Id, Monto = 1000, Estado = EstadoFactura.Pendiente };
            _context.Facturas.Add(factura);
            await _context.SaveChangesAsync();

            return Ok(new { facturaId = factura.Id, alumnoId = alumno.Id });
        }
    }
}
