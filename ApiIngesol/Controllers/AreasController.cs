using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models;

namespace ApiIngesol.Controllers;


[Route("api/[controller]")]
[ApiController]
public class AreasController(IService<Area> service, IMapper mapper)
    : GenericController<Area, AreaDto, AreaReadDto>(service, mapper)
{
    protected override string? Includes => "Planta";
    protected override Guid GetEntityId(Area entity) => entity.Id;
}
