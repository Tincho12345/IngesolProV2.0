using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ApiIngesol.Models.Materiales;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FamiliasController(IService<Familia> service, IMapper mapper)
    : GenericController<Familia, FamiliaDto, FamiliaDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // Traemos todas las familias
        var entities = await _service.GetAllAsync();

        // Mapear y aplicar filtro
        var dtos = await MapperHelper.MapToDtoListAsync<Familia, FamiliaReadDto>(_mapper, entities, filter);

        return Ok(dtos);
    }

    protected override Guid GetEntityId(Familia entity) => entity.Id;
}
