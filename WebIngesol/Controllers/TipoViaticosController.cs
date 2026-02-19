using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.Viaticos;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;

[Authorize(Roles = "🛡️ Admin")]

public class TipoViaticosController(IRepository<TipoViatico> repository,
                            IMapper mapper) : BaseController<TipoViatico,
                            TipoViaticoDto>(repository, mapper, CT.TipoViaticos) { }
