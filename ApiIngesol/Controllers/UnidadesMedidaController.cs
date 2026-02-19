using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models.Materiales;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UnidadesMedidaController(IService<UnidadMedida> service, IMapper mapper)
    : GenericController<UnidadMedida, UnidadMedidaDto, UnidadMedidaDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        // Traemos todas las unidades de medida
        var entities = await _service.GetAllAsync();

        // Mapeamos a DTOs
        var dtos = _mapper.Map<IEnumerable<UnidadMedidaDto>>(entities);

        // Opcional: aplicar filtro si se pasa
        if (!string.IsNullOrWhiteSpace(filter))
        {
            dtos = dtos.Where(d => d.Nombre.Contains(filter, StringComparison.OrdinalIgnoreCase));
        }

        return Ok(dtos);
    }

    protected override Guid GetEntityId(UnidadMedida entity) => entity.Id;
}
