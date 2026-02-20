using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models.Ubicacion;
using ApiIngesol.Repository;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProvinciasController(IService<Provincia> service, IMapper mapper)
    : GenericController<Provincia, ProvinciaDto, ProvinciaReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Traemos todas las provincias con su país relacionado
        var entities = await _service.GetAllAsync("Pais");

        // 2️⃣ Mapeamos y filtramos en DTOs
        var dtos = await MapperHelper.MapToDtoListAsync<Provincia, ProvinciaReadDto>(_mapper, entities, filter);

        // 3️⃣ Devolvemos la lista final
        return Ok(dtos);
    }

    protected override Guid GetEntityId(Provincia entity) => entity.Id;
}
