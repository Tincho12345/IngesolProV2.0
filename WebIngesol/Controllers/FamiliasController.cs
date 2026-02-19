using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.Materiales;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class FamiliasController : BaseController<Familia, FamiliaDtos>
{
    public FamiliasController(IRepository<Familia> repository, IMapper mapper)
        : base(repository, mapper, CT.Familias) {}
}