using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SolicitudesProyectoController(
    IService<SolicitudProyecto> service,
    IMapper mapper)
    : GenericController<SolicitudProyecto, CreateSolicitudProyectoDto, SolicitudProyectoReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Traemos todas las solicitudes (no hay navegación que incluir)
        var entities = await _service.GetAllAsync();

        // 2️⃣ Mapeo + filtro (idéntico a Areas)
        var dtos = await MapperHelper
            .MapToDtoListAsync<SolicitudProyecto, SolicitudProyectoReadDto>(
                _mapper,
                entities,
                filter
            );

        // 3️⃣ Respuesta
        return Ok(dtos);
    }

    protected override Guid GetEntityId(SolicitudProyecto entity) => entity.Id;
}
