using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.Materiales;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class CiudadesController : BaseController<Ciudad, CiudadDto>
{
    private readonly IRepository<Provincia> _provinciaRepository;

    public CiudadesController(
        IRepository<Ciudad> repository,
        IRepository<Provincia> provinciaRepository,
        IMapper mapper
    ) : base(repository, mapper, CT.Ciudades)
    {
        _provinciaRepository = provinciaRepository;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerProvincia()
    {
        var provincia = await _provinciaRepository.GetAllAsync(CT.Provincias);
        var result = provincia
            .OrderBy(f => f.Nombre)
            .Select(f => new { id = f.Id, nombre = f.Nombre });
        return Json(result);
    }
}
