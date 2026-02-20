using ApiIngesol.Controllers.Base;
using ApiIngesol.Models.Viatico;
using ApiIngesol.Repository;
using ApiIngesol.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RegistrosViaticosController(
    IService<RegistroViatico> service,
    IService<ValorViatico> valorService,
    IMapper mapper
) : GenericController<RegistroViatico, RegistroViaticoCreateDto, RegistroViaticoReadDto>(service, mapper)
{
    private readonly IService<ValorViatico> _valorService = valorService;

    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        var entities = await _service.GetAllAsync("ValorViatico.TipoViatico,Presupuesto,User");

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userRole == "👤 Empleados" && userId != null)
            entities = entities
                .Where(x => x.UserId.ToString() == userId);

        var dtos = await MapperHelper.MapToDtoListAsync<RegistroViatico, RegistroViaticoReadDto>(
            _mapper,
            entities,
            filter
        );

        return Ok(dtos);
    }

    [HttpPost]
    public override async Task<IActionResult> Create([FromForm] RegistroViaticoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var registro = _mapper.Map<RegistroViatico>(dto);

        var valorViatico = await _valorService.GetByIdAsync(dto.ValorViaticoId);
        if (valorViatico == null)
            return BadRequest($"No existe un ValorViatico con Id = {dto.ValorViaticoId}");

        registro.Valor = valorViatico.Valor;

        var result = await _service.CreateAsync(registro);
        if (!result)
            return StatusCode(500, "Error al crear el registro.");

        return CreatedAtAction(nameof(GetById), new { id = registro.Id }, registro);
    }

    [HttpPut("{id}")]
    public override async Task<IActionResult> Update(Guid id, [FromForm] RegistroViaticoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        _mapper.Map(dto, existing);

        var valorViatico = await _valorService.GetByIdAsync(dto.ValorViaticoId);
        if (valorViatico == null)
            return BadRequest($"No existe un ValorViatico con Id = {dto.ValorViaticoId}");

        existing.Valor = valorViatico.Valor;

        var result = await _service.UpdateAsync(existing);
        if (!result)
            return StatusCode(500, "Error al actualizar.");

        return NoContent();
    }

    protected override Guid GetEntityId(RegistroViatico entity) => entity.Id;
}
