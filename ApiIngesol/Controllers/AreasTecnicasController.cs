using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models;
using System;
using System.Threading.Tasks;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AreasTecnicasController(IService<AreaTecnica> service, IMapper mapper)
    : GenericController<AreaTecnica, AreaTecnicaDto, AreaTecnicaReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Traemos las áreas técnicas
        var entities = await _service.GetAllAsync();

        // 2️⃣ Mapeamos y filtramos
        var dtos = await MapperHelper.MapToDtoListAsync<AreaTecnica, AreaTecnicaReadDto>(_mapper, entities, filter);

        // 3️⃣ Devolvemos la lista
        return Ok(dtos);
    }

    protected override Guid GetEntityId(AreaTecnica entity) => entity.Id;
}
