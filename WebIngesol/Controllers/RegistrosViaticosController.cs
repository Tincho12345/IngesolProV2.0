using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Models.Viatico;
using WebIngesol.Repository.IRepository;

[Authorize(Roles = "🛡️ Admin, 👤 Empleados, ✏️ Data Entry, 👁️ Users")]
public class RegistrosViaticosController(
    IRepository<RegistroViatico> registroRepository,
    IRepository<ValorViatico> valorViaticoRepository,
    IRepository<Presupuesto> presupuestoRepository,
    IMapper mapper
) : BaseController<RegistroViatico, RegistroViaticoCreateDto>(
        registroRepository, mapper, CT.RegistrosViaticos)
{
    private readonly IRepository<ValorViatico> _valorViaticoRepository = valorViaticoRepository;
    private readonly IRepository<Presupuesto> _presupuestoRepository = presupuestoRepository;

    // ----------------------------------------------------------------------
    // 📌 Valores de Viáticos (SOLO ACTIVOS)  --- usado por search-youtube
    // ----------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> ObtenerValorViatico()
    {
        var valores = await _valorViaticoRepository.GetAllAsync(CT.ValoresViaticos);

        var culture = new CultureInfo("es-AR"); // Español de Argentina

        var result = valores
            .Where(v => v.IsActive)
            .OrderBy(v => v.TipoViaticoNombre)
            .Select(v => new
            {
                id = v.Id,
                nombre = $"{v.TipoViaticoNombre} - {v.Valor.ToString("C2", culture)}"
            });

        return Json(result);
    }

    // ----------------------------------------------------------------------
    // 📌 Presupuestos   --- usado por search-youtube
    // ----------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> ObtenerPresupuesto()
    {
        var presupuestos = await _presupuestoRepository.GetAllAsync(CT.Presupuestos);

        var result = presupuestos
            .OrderBy(p => p.NumeroOrden)
            .Select(p => new
            {
                id = p.Id,
                nombre = $"Orden {p.NumeroOrden} - {p.Descripcion}"
            });

        return Json(result);
    }
}
