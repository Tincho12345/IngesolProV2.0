using ApiIngesol.Controllers.Base;
using ApiIngesol.Models;
using ApiIngesol.Repository;
using ApiIngesol.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PlantasController(IService<Planta> service, IMapper mapper)
    : GenericController<Planta, PlantaDto, PlantaDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Obtenemos todas las entidades (sin filtro en base de datos)
        var entities = await _service.GetAllAsync();

        // 2️⃣ Mapeamos y filtramos en una sola línea (gracias a MapperHelper)
        var dtos = await MapperHelper.MapToDtoListAsync<Planta, PlantaDto>(_mapper, entities, filter);

        // 3️⃣ Devolvemos la lista final
        return Ok(dtos);
    }

    protected override Guid GetEntityId(Planta entity) => entity.Id;
}
