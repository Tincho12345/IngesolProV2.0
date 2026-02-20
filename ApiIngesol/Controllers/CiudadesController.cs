using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models.Ubicacion;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]

public class CiudadesController(IService<Ciudad> service, IMapper mapper)
    : GenericController<Ciudad, CiudadDto, CiudadReadDto>(service, mapper)
{
    protected override string? Includes => "Provincia";
    protected override Guid GetEntityId(Ciudad entity) => entity.Id;
}
