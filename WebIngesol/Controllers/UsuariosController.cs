using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]

public class UsuariosController(
    IRepository<Usuario> repository,
    IRepository<RoleDto> rolRepository,
    IMapper mapper
    ) : BaseController<Usuario, UsuarioDto>(repository, mapper, CT.User)
{
    private readonly IRepository<RoleDto> _rolRepository = rolRepository;

    [HttpGet]
    public async Task<IActionResult> ObtenerRol()
    {
        var rol = await _rolRepository.GetAllAsync(CT.Roles);
        var result = rol
            .OrderBy(f => f.Name)
            .Select(f => new { id = f.Id, nombre = f.Name });
        return Json(result);
    }
}
