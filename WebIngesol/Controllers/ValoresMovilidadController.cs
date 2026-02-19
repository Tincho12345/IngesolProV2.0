using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.Movilidad;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;

[Authorize(Roles = "🛡️ Admin")]
public class ValoresMovilidadController : BaseController<ValorMovilidad, ValorMovilidadReadDto>
{
    private readonly IRepository<TipoMovilidad> _tipoMovilidadRepository;

    public ValoresMovilidadController(
        IRepository<ValorMovilidad> repository,
        IRepository<TipoMovilidad> tipoMovilidadRepository,
        IMapper mapper
    ) : base(repository, mapper, CT.ValoresMovilidad)
    {
        _tipoMovilidadRepository = tipoMovilidadRepository;
    }

    // Igual al método ObtenerTipoViatico pero para Movilidad
    [HttpGet]
    public async Task<IActionResult> ObtenerTipoMovilidad()
    {
        var tipos = await _tipoMovilidadRepository.GetAllAsync(CT.TiposMovilidad);

        var result = tipos
            .Where(t => t.IsActive)          // ← SOLO ACTIVOS
            .OrderBy(t => t.Nombre)
            .Select(t => new
            {
                id = t.Id,
                nombre = $"DESDE: {t.Desde} | HASTA: {t.Hasta} | OBS: {t.Observaciones}"
            });

        return Json(result);
    }
}
