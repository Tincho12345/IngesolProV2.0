using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AreasController(IService<Area> service, IMapper mapper)
    : GenericController<Area, AreaDto, AreaReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Traemos las áreas con su Planta relacionada
        var entities = await _service.GetAllAsync("Planta");

        // 2️⃣ Mapeamos y filtramos en una sola línea (ya incluye el filtro si existe)
        var dtos = await MapperHelper.MapToDtoListAsync<Area, AreaReadDto>(_mapper, entities, filter);

        // 3️⃣ Devolvemos la lista final
        return Ok(dtos);
    }
    protected override Guid GetEntityId(Area entity) => entity.Id;
}
