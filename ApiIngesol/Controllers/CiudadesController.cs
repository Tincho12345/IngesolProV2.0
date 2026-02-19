using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models.Ubicacion;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CiudadesController(IService<Ciudad> service, IMapper mapper)
    : GenericController<Ciudad, CiudadDto, CiudadReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Traemos las ciudades incluyendo su Provincia
        var entities = await _service.GetAllAsync("Provincia");

        // 2️⃣ Mapeamos a DTOs aplicando el filtro (si existe)
        var dtos = await MapperHelper.MapToDtoListAsync<Ciudad, CiudadReadDto>(_mapper, entities, filter);

        // 3️⃣ Devolvemos la lista resultante
        return Ok(dtos);
    }

    protected override Guid GetEntityId(Ciudad entity) => entity.Id;
}
