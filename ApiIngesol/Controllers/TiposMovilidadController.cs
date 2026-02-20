using ApiIngesol.Controllers.Base;
using ApiIngesol.Models.Movilidad;
using ApiIngesol.Repository;
using ApiIngesol.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TiposMovilidadController(IService<TipoMovilidad> service, IMapper mapper)
    : GenericController<TipoMovilidad, TipoMovilidadCreateDto, TipoMovilidadDto>(service, mapper)
{
    // GET api/tiposmovilidad
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        var entities = await _service.GetAllAsync();
        var dtos = await MapperHelper.MapToDtoListAsync<TipoMovilidad, TipoMovilidadDto>(_mapper, entities, filter);
        return Ok(dtos);
    }

    protected override Guid GetEntityId(TipoMovilidad entity) => entity.Id;
}
