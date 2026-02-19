using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Models.Movilidad;
using WebIngesol.Repository.IRepository;

[Authorize(Roles = "🛡️ Admin, 👤 Empleados, ✏️ Data Entry, 👁️ Users")]
public class RegistrosMovilidadController : BaseController<RegistroMovilidad, RegistroMovilidadCreateDto>
{
    private readonly IRepository<ValorMovilidad> _valorMovilidadRepository;
    private readonly IRepository<Presupuesto> _presupuestoRepository;

    public RegistrosMovilidadController(
        IRepository<RegistroMovilidad> registroRepository,
        IRepository<ValorMovilidad> valorMovilidadRepository,
        IRepository<Presupuesto> presupuestoRepository,
        IMapper mapper
    ) : base(registroRepository, mapper, CT.RegistrosMovilidad)
    {
        _valorMovilidadRepository = valorMovilidadRepository;
        _presupuestoRepository = presupuestoRepository;
    }

    // ----------------------------------------------------------------------
    // 📌 Valores de Movilidad (SOLO ACTIVOS)
    // ----------------------------------------------------------------------
    [HttpGet]
    public async Task<IActionResult> ObtenerValorMovilidad()
    {
        var valores = await _valorMovilidadRepository.GetAllAsync(CT.ValoresMovilidad);
        var culture = new CultureInfo("es-AR"); // Español de Argentina

        var result = valores
            .Where(v => v.IsActive)
            .OrderBy(v => v.TipoMovilidadNombre)
            .Select(v => new
            {
                id = v.Id,
                nombre = $"{v.TipoMovilidadNombre} - {v.Valor.ToString("C2", culture)}"
            });

        return Json(result);
    }

    // ----------------------------------------------------------------------
    // 📌 Presupuestos
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
