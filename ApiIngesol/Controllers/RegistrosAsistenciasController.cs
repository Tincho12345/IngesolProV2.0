using ApiIngesol.Controllers.Base;
using ApiIngesol.Models;
using ApiIngesol.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RegistrosAsistencias(
    IService<RegistroAsistencia> service,
    IService<Presupuesto> presupuestoService,
    IMapper mapper
) : GenericController<RegistroAsistencia, RegistroAsistenciaCreateDto, RegistroAsistenciaReadDto>(service, mapper)
{
    private readonly IService<Presupuesto> _presupuestoService = presupuestoService;

    // GET ALL
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        var entities = await _service.GetAllAsync("Empleado,Presupuesto");

        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (userRole == "👤 Empleados" && userId != null)
            entities = entities.Where(x => x.EmpleadoId == userId).ToList();

        var dtos = await MapperHelper.MapToDtoListAsync<RegistroAsistencia, RegistroAsistenciaReadDto>(
            _mapper,
            entities,
            filter
        );

        return Ok(dtos);
    }

    // CREATE
    [HttpPost]
    public override async Task<IActionResult> Create([FromForm] RegistroAsistenciaCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var registro = _mapper.Map<RegistroAsistencia>(dto);

        // Validar Presupuesto
        var presupuesto = await _presupuestoService.GetByIdAsync(dto.PresupuestoId);
        if (presupuesto == null)
            return BadRequest($"No existe un Presupuesto con Id = {dto.PresupuestoId}");

        registro.Presupuesto = presupuesto;

        var result = await _service.CreateAsync(registro);
        if (!result)
            return StatusCode(500, "Error al crear el registro.");

        return CreatedAtAction(nameof(GetById), new { id = registro.Id }, registro);
    }

    // UPDATE
    [HttpPut("{id}")]
    public override async Task<IActionResult> Update(Guid id, [FromForm] RegistroAsistenciaCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        _mapper.Map(dto, existing);

        // Validar Presupuesto
        var presupuesto = await _presupuestoService.GetByIdAsync(dto.PresupuestoId);
        if (presupuesto == null)
            return BadRequest($"No existe un Presupuesto con Id = {dto.PresupuestoId}");

        existing.Presupuesto = presupuesto;

        var result = await _service.UpdateAsync(existing);
        if (!result)
            return StatusCode(500, "Error al actualizar.");

        return NoContent();
    }

    protected override Guid GetEntityId(RegistroAsistencia entity) => entity.Id;
}
