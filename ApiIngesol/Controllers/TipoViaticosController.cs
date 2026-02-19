using ApiIngesol.Controllers.Base;
using ApiIngesol.Models.Viatico;
using ApiIngesol.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TipoViaticosController(IService<TipoViatico> service, IMapper mapper)
    : GenericController<TipoViatico, TipoViaticoDto, TipoViaticoDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        var entities = await _service.GetAllAsync();
        var dtos = await MapperHelper.MapToDtoListAsync<TipoViatico, TipoViaticoDto>(_mapper, entities, filter);
        return Ok(dtos);
    }

    protected override Guid GetEntityId(TipoViatico entity) => entity.Id;
}
