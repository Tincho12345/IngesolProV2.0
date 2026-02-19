using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.Ubicacion;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class PaisesController : BaseController<Pais, PaisDtos>
{
    public PaisesController(IRepository<Pais> repository, IMapper mapper)
        : base(repository, mapper, CT.Paises) {}
}