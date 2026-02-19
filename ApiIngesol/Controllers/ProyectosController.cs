using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProyectosController(IService<Proyecto> service, IMapper mapper)
    : GenericController<Proyecto, CreateProyectoDto, ProyectoReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // 1️⃣ Cargamos jerarquía completa
        var entities = await _service.GetAllAsync("Area.Planta,Ordenes.Presupuestos");

        // 2️⃣ Mapeamos sin filtrar aún
        var dtos = (await MapperHelper
            .MapToDtoProyectosListAsync<Proyecto, ProyectoReadDto>(_mapper, entities))
            .ToList();

        // 3️⃣ Enriquecemos los DTOs
        foreach (var dto in dtos)
        {
            var proyecto = entities.First(p => p.Id == dto.Id);

            dto.TotalPresupuestos = proyecto.Ordenes
                .SelectMany(o => o.Presupuestos)
                .Sum(p => p.Total);

            var ultimaOrden = proyecto.Ordenes
                .OrderByDescending(o => o.NumeroOrden)
                .FirstOrDefault();

            dto.UltimoNumeroOrden = ultimaOrden?.NumeroOrden;
            dto.UltimaDescripcionOrden = ultimaOrden?.DescripcionOrden ?? string.Empty;
        }

        // 4️⃣ Si el usuario filtró, lo aplicamos sobre el DTO final
        var result = string.IsNullOrWhiteSpace(filter)
            ? dtos
            : MapperHelper.FilterList(dtos, filter);

        return Ok(result);
    }

    protected override Guid GetEntityId(Proyecto entity) => entity.Id;
}
