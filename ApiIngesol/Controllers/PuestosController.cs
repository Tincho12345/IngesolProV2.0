using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PuestosController(IService<Puesto> service, IMapper mapper)
    : GenericController<Puesto, PuestoDto, PuestoDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // Traemos todos los puestos
        var entities = await _service.GetAllAsync();

        // Mapeamos a DTOs
        var dtos = _mapper.Map<IEnumerable<PuestoDto>>(entities);

        // Aplicamos filtro si se pasa
        if (!string.IsNullOrWhiteSpace(filter))
        {
            dtos = dtos.Where(d => d.Nombre.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(dtos);
    }

    protected override Guid GetEntityId(Puesto entity) => entity.Id;
}
