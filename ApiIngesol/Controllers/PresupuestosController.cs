using ApiIngesol.Controllers.Base;
using ApiIngesol.Models;
using ApiIngesol.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PresupuestosController(IService<Presupuesto> service, IMapper mapper)
    : GenericController<Presupuesto, PresupuestoDto, PresupuestoReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Incluimos las relaciones necesarias
        var includeProps = "Orden,Orden.Proyecto,Orden.Proyecto.Area,Orden.Proyecto.Area.Planta,Items";

        // 2️⃣ Obtenemos las entidades incluyendo las relaciones
        var entities = await _service.GetAllAsync(includeProps);

        // 3️⃣ Mapeamos y filtramos en una sola línea
        var dtos = await MapperHelper.MapToDtoListAsync<Presupuesto, PresupuestoReadDto>(_mapper, entities, filter);

        // 4️⃣ Retornamos el resultado final
        return Ok(dtos);
    }

    protected override Guid GetEntityId(Presupuesto entity) => entity.Id;
}
