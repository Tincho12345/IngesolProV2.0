using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class ProyectosController(
    IRepository<Proyecto> repository,
    IRepository<Area> areaRepository,
    IMapper mapper
    ) : BaseController<Proyecto, CreateProyectoDto>(repository, mapper, CT.Proyectos)
{
    private readonly IRepository<Area> _areaRepository = areaRepository;

    [HttpGet]
    public async Task<IActionResult> ObtenerArea()
    {
        var usuario = User.Identity?.Name ?? "No autenticado";

        var roles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        var usuarioRol =  new JsonResult(new
        {
            Usuario = usuario,
            Roles = roles
        });

        var areas = await _areaRepository.GetAllAsync(CT.Areas);

        var result = areas
            .OrderBy(f => f.PlantaNombre)
            .ThenBy(f => f.Nombre) // opcional: orden secundario por nombre
            .Select(f => new
            {
                id = f.Id,
                nombre = $"{f.PlantaNombre} / {f.Nombre}"
            });
        return Json(result);
    }

    [HttpGet("VerUsuarioYRol")]
    public IActionResult VerUsuarioYRol()
    {
        var usuario = User.Identity?.Name ?? "No autenticado";

        var roles = User.Claims
            .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
            .Select(c => c.Value)
            .ToList();

        return new JsonResult(new
        {
            Usuario = usuario,
            Roles = roles
        });
    }
}
