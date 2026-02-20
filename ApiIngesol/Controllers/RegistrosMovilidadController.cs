using ApiIngesol.Controllers.Base;
using ApiIngesol.Models.Movilidad;
using ApiIngesol.Repository;
using ApiIngesol.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RegistrosMovilidadController(
    IService<RegistroMovilidad> service,
    IService<ValorMovilidad> valorService,
    IMapper mapper
) : GenericController<RegistroMovilidad, RegistroMovilidadCreateDto, RegistroMovilidadReadDto>(service, mapper)
{
    private readonly IService<ValorMovilidad> _valorService = valorService;

    // GET ALL
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        var entities = await _service.GetAllAsync("ValorMovilidad.TipoMovilidad,,Presupuesto,User");

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userRole == "👤 Empleados" && userId != null)
            entities = entities.Where(x => x.UserId.ToString() == userId).ToList();

        var dtos = await MapperHelper.MapToDtoListAsync<RegistroMovilidad, RegistroMovilidadReadDto>(
            _mapper,
            entities,
            filter
        );

        return Ok(dtos);
    }

    // CREATE
    [HttpPost]
    public override async Task<IActionResult> Create([FromForm] RegistroMovilidadCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var registro = _mapper.Map<RegistroMovilidad>(dto);

        var valor = await _valorService.GetByIdAsync(dto.ValorMovilidadId);
        if (valor == null)
            return BadRequest($"No existe un ValorMovilidad con Id = {dto.ValorMovilidadId}");

        registro.Valor = valor.Valor;

        var result = await _service.CreateAsync(registro);
        if (!result)
            return StatusCode(500, "Error al crear el registro.");

        return CreatedAtAction(nameof(GetById), new { id = registro.Id }, registro);
    }

    // UPDATE
    [HttpPut("{id}")]
    public override async Task<IActionResult> Update(Guid id, [FromForm] RegistroMovilidadCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        _mapper.Map(dto, existing);

        var valor = await _valorService.GetByIdAsync(dto.ValorMovilidadId);
        if (valor == null)
            return BadRequest($"No existe un ValorMovilidad con Id = {dto.ValorMovilidadId}");

        existing.Valor = valor.Valor;

        var result = await _service.UpdateAsync(existing);
        if (!result)
            return StatusCode(500, "Error al actualizar.");

        return NoContent();
    }

    protected override Guid GetEntityId(RegistroMovilidad entity) => entity.Id;
}
