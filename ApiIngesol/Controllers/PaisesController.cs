using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using ApiIngesol.Models.Ubicacion;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PaisesController(IService<Pais> service, IMapper mapper)
    : GenericController<Pais, PaisDto, PaisDto>(service, mapper)
{
    protected override Guid GetEntityId(Pais entity) => entity.Id;
}
