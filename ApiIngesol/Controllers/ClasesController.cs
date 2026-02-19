using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using ApiIngesol.Models.Materiales;
using AutoMapper;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClasesController(IService<Clase> service, IMapper mapper)
    : GenericController<Clase, ClaseDto, ClaseReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Traemos las clases con su relación a Familia
        var entities = await _service.GetAllAsync("Familia");

        // 2️⃣ Mapeamos y aplicamos el filtro (si se envía)
        var dtos = await MapperHelper.MapToDtoListAsync<Clase, ClaseReadDto>(_mapper, entities, filter);

        // 3️⃣ Devolvemos el resultado final
        return Ok(dtos);
    }

    protected override Guid GetEntityId(Clase entity) => entity.Id;
}
