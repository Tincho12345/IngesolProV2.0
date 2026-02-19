using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.Movilidad;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;

[Authorize(Roles = "🛡️ Admin")]

public class TiposMovilidadController(IRepository<TipoMovilidad> repository,
                            IMapper mapper) : BaseController<TipoMovilidad,
                            TipoMovilidadDto>(repository, mapper, CT.TiposMovilidad)
{ }
