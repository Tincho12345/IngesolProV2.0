using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.Materiales;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class ClasesController : BaseController<Clase, ClaseDto>
{
    private readonly IRepository<Familia> _familiaRepository;

    public ClasesController(
        IRepository<Clase> repository,
        IRepository<Familia> familiaRepository,
        IMapper mapper
    ) : base(repository, mapper, CT.Clases)
    {
        _familiaRepository = familiaRepository;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerFamilia()
    {
        var familias = await _familiaRepository.GetAllAsync(CT.Familias);
        var result = familias
            .OrderBy(f => f.Nombre)
            .Select(f => new { id = f.Id, nombre = f.Nombre });

        return Json(result);
    }
}
