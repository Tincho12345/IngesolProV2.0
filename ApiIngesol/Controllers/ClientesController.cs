using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using ApiIngesol.Models;
using AutoMapper;
using ApiIngesol.Repository;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientesController(IService<Cliente> service, IMapper mapper)
    : GenericController<Cliente, ClienteDto, ClienteReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Obtenemos todos los clientes
        var entities = await _service.GetAllAsync();

        // 2️⃣ Mapeamos a DTO y aplicamos filtro si corresponde
        var dtos = await MapperHelper.MapToDtoListAsync<Cliente, ClienteReadDto>(_mapper, entities, filter);

        // 3️⃣ Devolvemos la lista resultante
        return Ok(dtos);
    }

    protected override Guid GetEntityId(Cliente entity) => entity.Id;
}
