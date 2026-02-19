using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.Viatico;
using WebIngesol.Models.Viaticos;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;

[Authorize(Roles = "🛡️ Admin")]
public class ValoresViaticosController : BaseController<ValorViatico, ValorViaticoDto>
{
    private readonly IRepository<TipoViatico> _tipoViaticoRepository;

    public ValoresViaticosController(
        IRepository<ValorViatico> repository,
        IRepository<TipoViatico> tipoViaticoRepository,
        IMapper mapper
    ) : base(repository, mapper, CT.ValoresViaticos)
    {
        _tipoViaticoRepository = tipoViaticoRepository;
    }

    // ⭐ Igual que ProvinciasController → ObtenerPais()
    [HttpGet]
    public async Task<IActionResult> ObtenerTipoViatico()
    {
        var tipos = await _tipoViaticoRepository.GetAllAsync(CT.TipoViaticos);

        var result = tipos
            .OrderBy(t => t.Nombre)
            .Select(t => new
            {
                id = t.Id,
                nombre = t.Nombre
            });

        return Json(result);
    }
}
