using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AreasTecnicasController(IService<AreaTecnica> service, IMapper mapper)
    : GenericController<AreaTecnica, AreaTecnicaDto, AreaTecnicaReadDto>(service, mapper)
{
    // 🔹 Si necesitas Includes, declaralos acá.
    //protected override string? Includes => null;
    protected override Guid GetEntityId(AreaTecnica entity) => entity.Id;
}
