using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models.Viatico;
using ApiIngesol.Repository;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ValoresViaticosController(IService<ValorViatico> service, IMapper mapper)
    : GenericController<ValorViatico, ValorViaticoCreateDto, ValorViaticoReadDto>(service, mapper)
{
    // 🔎 GET personalizado
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Obtener entidades incluyendo TipoViatico
        var entities = await _service.GetAllAsync("TipoViatico");


        // 2️⃣ Mapear a DTOs con filtro
        var dtos = await MapperHelper.MapToDtoListAsync<ValorViatico, ValorViaticoReadDto>(
            _mapper, entities, filter
        );

        // 3️⃣ Completar el nombre del TipoViatico sin hacer búsquedas repetidas
        foreach (var dto in dtos)
        {
            var entity = entities.First(e => e.Id == dto.Id);
            dto.TipoViaticoNombre = entity.TipoViatico?.Nombre ?? string.Empty;
            dto.Nombre =  entity.TipoViatico?.Nombre ?? string.Empty;
        }

        // 4️⃣ Ordenar → Activos primero, luego por nombre del TipoViatico
        var ordered = dtos
            .OrderByDescending(v => v.IsActive)
            .ThenBy(v => v.TipoViaticoNombre)
            .ToList();

        // 5️⃣ Retornar lista ordenada
        return Ok(ordered);
    }

    // ➕ CREATE con lógica de negocio
    [HttpPost]
    public override async Task<IActionResult> Create([FromForm] ValorViaticoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<ValorViatico>(dto);

        // Siempre activar el nuevo registro
        entity.IsActive = true;

        // Desactivar todos los activos del mismo TipoViatico
        var otrosActivos = await _service.FindAsync(v =>
            v.TipoViaticoId == entity.TipoViaticoId &&
            v.IsActive
        );

        foreach (var act in otrosActivos)
        {
            act.IsActive = false;
            await _service.UpdateAsync(act);
        }

        var created = await _service.CreateAsync(entity);
        if (!created)
            return StatusCode(500, "Error al crear el valor de viático.");

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, entity);
    }


    [HttpPut("{id}")]
    public override async Task<IActionResult> Update(Guid id, [FromForm] ValorViaticoCreateDto dto)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound();

        // Guardar estado anterior
        bool oldIsActive = existing.IsActive;
        Guid oldTipo = existing.TipoViaticoId;

        // Mapear cambios del DTO → entidad existente
        _mapper.Map(dto, existing);

        // 💡 Detectar el CAMBIO de estado a Activo
        bool cambioAActivo = (!oldIsActive && existing.IsActive);

        // 💡 Detectar si se cambió el TipoViatico
        bool cambioTipo = existing.TipoViaticoId != oldTipo;

        // Si el usuario activó este registro O cambió el tipo y mantiene activo
        if (cambioAActivo || (cambioTipo && existing.IsActive))
        {
            // Desactivar otros activos del mismo tipo
            var otrosActivos = await _service.FindAsync(v =>
                v.TipoViaticoId == existing.TipoViaticoId &&
                v.IsActive &&
                v.Id != existing.Id
            );

            foreach (var act in otrosActivos)
            {
                act.IsActive = false;
                await _service.UpdateAsync(act);
            }
        }

        var updated = await _service.UpdateAsync(existing);
        if (!updated)
            return StatusCode(500, "Error al actualizar el valor de viático.");

        return NoContent();
    }

    protected override Guid GetEntityId(ValorViatico entity) => entity.Id;
}
