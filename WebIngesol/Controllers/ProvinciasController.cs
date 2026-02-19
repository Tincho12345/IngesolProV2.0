using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.Materiales;
using WebIngesol.Models.Ubicacion;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]

public class ProvinciasController : BaseController<Provincia, ProvinciaDto>
{
    private readonly IRepository<Pais> _paisRepository;

    public ProvinciasController(
        IRepository<Provincia> repository,
        IRepository<Pais> paisRepository,
        IMapper mapper
    ) : base(repository, mapper, CT.Provincias)
    {
        _paisRepository = paisRepository;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerPais()
    {
        var paises = await _paisRepository.GetAllAsync(CT.Paises);
        var result = paises
            .OrderBy(f => f.Nombre)
            .Select(f => new { id = f.Id, nombre = f.Nombre });

        return Json(result);
    }
}
