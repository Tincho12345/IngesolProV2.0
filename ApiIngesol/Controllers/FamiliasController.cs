using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using ApiIngesol.Models.Materiales;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FamiliasController(IService<Familia> service, IMapper mapper)
    : GenericController<Familia, FamiliaDto, FamiliaReadDto>(service, mapper)
{
    protected override Guid GetEntityId(Familia entity) => entity.Id;
}
