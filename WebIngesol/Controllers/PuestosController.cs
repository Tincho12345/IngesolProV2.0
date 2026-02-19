using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class PuestosController(IRepository<Puesto> repository,
                            IMapper mapper) : BaseController<Puesto, 
                            PuestoDto>(repository, mapper, CT.Puestos)
{
}