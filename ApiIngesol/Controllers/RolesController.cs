using ApiIngesol.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
//[Authorize(Roles = "🛡️ Admin")]
[ApiController]
public class RolesController(RoleManager<ApplicationRole> roleManager) : ControllerBase
{
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;

    [HttpGet("{id}")]
    public async Task<IActionResult> GetRoleById(Guid id)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
            return NotFound("Rol no encontrado.");

        return Ok(new
        {
            role.Id,
            role.Name,
            role.IsActive
        });
    }

    [HttpPost]
    public async Task<IActionResult> CreateRole([FromForm] CreateRoleDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre del rol es obligatorio.");

        var exists = await _roleManager.RoleExistsAsync(dto.Name);
        if (exists)
            return BadRequest("El rol ya existe.");

        var role = new ApplicationRole
        {
            Name = dto.Name,
            IsActive = dto.IsActive  // Asegúrate de agregar IsActive en CreateRoleDto
        };

        var result = await _roleManager.CreateAsync(role);
        if (!result.Succeeded)
            return StatusCode(500, result.Errors);

        return Ok("Rol creado correctamente.");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateRole(Guid id, [FromForm] CreateRoleDto dto)
    {
        var role = await _roleManager.FindByIdAsync(id.ToString());
        if (role == null)
            return NotFound("Rol no encontrado.");

        // Validaciones opcionales
        if (string.IsNullOrWhiteSpace(dto.Name))
            return BadRequest("El nombre es obligatorio.");

        // Actualizar propiedades
        role.Name = dto.Name;
        role.IsActive = dto.IsActive;

        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded)
            return StatusCode(500, result.Errors);

        return Ok("Rol actualizado correctamente.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteRole(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
            return NotFound("Rol no encontrado.");

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
            return StatusCode(500, result.Errors);

        return Ok("Rol eliminado correctamente.");
    }

    [HttpGet]
    public IActionResult GetRoles()
    {
        var roles = _roleManager.Roles
            .Select(r => new { r.Id, r.Name, r.IsActive })
            .ToList();

        return Ok(roles);
    }
}
