using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin, ✏️ Data Entry")]
public class PresupuestosController(
    IRepository<PresupuestoReadDto> repository,
    IRepository<Orden> ordenRepository,
    IMapper mapper,
    IHttpClientFactory httpClientFactory
) : BaseController<PresupuestoReadDto, PresupuestoDto>(repository, mapper, CT.Presupuestos)
{
    private readonly IRepository<Orden> _ordenRepository = ordenRepository;
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;

    [HttpGet]
    public async Task<IActionResult> ObtenerOrden()
    {
        var ordenes = await _ordenRepository.GetAllAsync(CT.Ordenes);

        var result = ordenes
            .OrderBy(o => o.NumeroOrden)
            .Select(o => new
            {
                id = o.Id,
                nombre = $"{o.NumeroOrden} - {o.DescripcionOrden ?? "Sin descripción"} - {o.Nombre ?? "Sin nombre"}"
            });

        return Json(result);
    }
}
