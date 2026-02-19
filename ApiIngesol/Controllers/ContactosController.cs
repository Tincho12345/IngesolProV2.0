using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using AutoMapper;
using ApiIngesol.Models;
using Microsoft.AspNetCore.Mvc;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ContactosController(IService<Contacto> service, IMapper mapper)
    : GenericController<Contacto, ContactoDto, ContactoReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Obtenemos todos los contactos incluyendo la relación con Cliente
        var entities = await _service.GetAllAsync("Cliente");

        // 2️⃣ Mapeamos a DTO y aplicamos filtro si corresponde
        var dtos = await MapperHelper.MapToDtoListAsync<Contacto, ContactoReadDto>(_mapper, entities, filter);

        // 3️⃣ Devolvemos la lista final
        return Ok(dtos);
    }

    protected override Guid GetEntityId(Contacto entity) => entity.Id;
}
