using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models.Movilidad;
using ApiIngesol.Repository;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ValoresMovilidadController(IService<ValorMovilidad> service, IMapper mapper)
    : GenericController<ValorMovilidad, ValorMovilidadCreateDto, ValorMovilidadReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Traemos todos los valores de movilidad con su TipoMovilidad relacionado
        var entities = await _service.GetAllAsync("TipoMovilidad");

        // 2️⃣ Mapeamos y filtramos en DTOs
        var dtos = await MapperHelper.MapToDtoListAsync<ValorMovilidad, ValorMovilidadReadDto>(_mapper, entities, filter);

        // 3️⃣ Devolvemos la lista final
        return Ok(dtos);
    }

    protected override Guid GetEntityId(ValorMovilidad entity) => entity.Id;
}
