using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using ApiIngesol.Models.Ubicacion;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaisesController(IService<Pais> service, IMapper mapper)
    : GenericController<Pais, PaisDto, PaisDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        var entities = await _service.GetAllAsync();
        var dtos = await MapperHelper.MapToDtoListAsync<Pais, PaisDto>(_mapper, entities, filter);
        return Ok(dtos);
    }

    protected override Guid GetEntityId(Pais entity) => entity.Id;
}
