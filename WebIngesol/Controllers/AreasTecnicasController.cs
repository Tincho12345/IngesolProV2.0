using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.AreaTecnica;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class AreasTecnicasController(IRepository<AreaTecnica> repository,
                                    IMapper mapper) : BaseController<AreaTecnica, 
                                    AreaTecnicaDto>(repository, mapper, CT.AreasTecnicas)
{
}