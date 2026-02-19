using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.Materiales;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class TiposController : BaseController<Tipo, TipoDto>
{
    private readonly IRepository<Clase> _claseRepository;

    public TiposController(
        IRepository<Tipo> repository,
        IRepository<Clase> claseRepository,
        IMapper mapper
    ) : base(repository, mapper, CT.Tipos)
    {
        _claseRepository = claseRepository;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerClase()
    {
        var clases = await _claseRepository.GetAllAsync(CT.Clases);
        var result = clases
            .OrderBy(f => f.Nombre)
            .Select(f => new { id = f.Id, nombre = f.Nombre });

        return Json(result);
    }
}
