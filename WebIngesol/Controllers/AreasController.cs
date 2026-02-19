using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class AreasController(
    IRepository<Area> repository,
    IRepository<Planta> plantaRepository,
    IMapper mapper
    ) : BaseController<Area, AreaDto>(repository, mapper, CT.Areas)
{
    private readonly IRepository<Planta> _plantaRepository = plantaRepository;

    [HttpGet]
    public async Task<IActionResult> ObtenerPlanta()
    {
        var planta = await _plantaRepository.GetAllAsync(CT.Plantas);
        var result = planta
            .OrderBy(f => f.Nombre)
            .Select(f => new { id = f.Id, nombre = f.Nombre });

        return Json(result);
    }
}
