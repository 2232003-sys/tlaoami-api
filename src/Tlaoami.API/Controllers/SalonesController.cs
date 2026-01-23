using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Tlaoami.Application.Dtos;
using Tlaoami.Application.Exceptions;
using Tlaoami.Application.Interfaces;

namespace Tlaoami.API.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/v1/[controller]")]
public class SalonesController : ControllerBase
{
    private readonly ISalonService _salonService;

    public SalonesController(ISalonService salonService)
    {
        _salonService = salonService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<SalonDto>>> GetAll([FromQuery] bool soloActivos = false)
    {
        var activo = soloActivos ? true : (bool?)null;
        var data = await _salonService.GetAllAsync(activo);
        return Ok(data);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SalonDto>> GetById(Guid id)
    {
        var salon = await _salonService.GetByIdAsync(id);
        return salon is null ? NotFound() : Ok(salon);
    }

    [HttpPost]
    public async Task<ActionResult<SalonDto>> Create([FromBody] SalonCreateDto input)
    {
        try
        {
            var created = await _salonService.CreateAsync(input);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (BusinessException ex) when (ex.Code == "SALON_CODIGO_DUPLICADO")
        {
            return Conflict(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SalonDto>> Update(Guid id, [FromBody] SalonUpdateDto input)
    {
        try
        {
            var updated = await _salonService.UpdateAsync(id, input);
            return Ok(updated);
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BusinessException ex) when (ex.Code == "SALON_CODIGO_DUPLICADO")
        {
            return Conflict(ex.Message);
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id)
    {
        try
        {
            await _salonService.DeleteAsync(id);
            return NoContent();
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
        catch (BusinessException ex) when (ex.Code == "SALON_EN_USO")
        {
            return Conflict(ex.Message);
        }
    }
}
