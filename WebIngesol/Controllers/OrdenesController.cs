using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin, 👤 Empleados, ✏️ Data Entry")]
public class OrdenesController(
    IRepository<OrdenReadDto> repository,
    IRepository<Usuario> usuarioRepository,
    IRepository<Proyecto> proyectoRepository,
    IMapper mapper
    ) : BaseController<OrdenReadDto, OrdenDto>(repository, mapper, CT.Ordenes)
{
    private readonly IRepository<Usuario> _usuarioRepository = usuarioRepository ?? throw new ArgumentNullException(nameof(usuarioRepository));
    private readonly IRepository<Proyecto> _proyectoRepository = proyectoRepository ?? throw new ArgumentNullException(nameof(proyectoRepository));

    /// 👤 Endpoint para obtener todos los Usuarios en formato: APELLIDO, Nombre (capitalizado)
    [HttpGet]
    public async Task<IActionResult> ObtenerResponsable()
    {
        var usuarios = await _usuarioRepository.GetAllAsync(CT.User);
        var ti = CultureInfo.InvariantCulture.TextInfo;

        var result = usuarios
            .Where(u => !string.IsNullOrWhiteSpace(u.FirstName) && !string.IsNullOrWhiteSpace(u.LastName))
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new
            {
                id = u.Id,
                nombre = $"{u.LastName.ToUpperInvariant()}, {ti.ToTitleCase(u.FirstName.ToLowerInvariant())}"
            });

        return Json(result);
    }


    /// 🗂️ Endpoint para obtener todos los Proyectos con su información completa
    [HttpGet]
    public async Task<IActionResult> ObtenerProyecto()
    {
        var proyectos = await _proyectoRepository.GetAllAsync(CT.Proyectos);

        var result = proyectos
           .Where(p => !string.IsNullOrWhiteSpace(p.Descripcion))
           .OrderBy(p => p.PlantaNombre)
           .ThenBy(p => p.AreaNombre)
           .Select(p => new
           {
               id = p.Id,
               nombre = $"{p.PlantaNombre ?? "Sin Planta"} - {p.AreaNombre ?? "Sin Área"} - {p.Descripcion.ToUpperInvariant()}"
           });

        return Json(result);
    }
}
