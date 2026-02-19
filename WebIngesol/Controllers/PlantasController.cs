using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class PlantasController : BaseController<Planta, PlantaDto>
{
    public PlantasController(IRepository<Planta> repository, IMapper mapper)
        : base(repository, mapper, CT.Plantas) { }
}