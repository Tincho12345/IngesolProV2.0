using Microsoft.AspNetCore.Authorization;
using ApiIngesol.Repository.IRepository;
using ApiIngesol.Models.Users.Dtos;
using Microsoft.AspNetCore.Mvc;
using ApiIngesol.Models.Users;
using AutoMapper;

namespace ApiIngesol.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService, IMapper mapper) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var users = await userService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Edit(string id)
    {
        var user = await userService.GetByIdWithRolesAsync(id);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromForm] RegisterDto registerDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = mapper.Map<ApplicationUser>(registerDto);

        // 🔥 ASIGNACIÓN FIJA DE EMPRESA
        user.CompanyId = Guid.Parse("EC2DBFC2-0CF1-4FDE-9613-8EC9A3BBEEA6");

        var result = await userService.CreateUserAsync(
            user,
            registerDto.Password,
            registerDto.RolId,
            registerDto.UserPic
        );

        if (!result)
            return BadRequest("No se pudo registrar el usuario");

        return Ok("Usuario registrado correctamente.");
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await userService.AuthenticateAsync(loginDto);
        if (response == null) return Unauthorized("Credenciales inválidas");

        return Ok(response);
    }

    [Authorize(Roles = "🛡️ Admin")]
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateUserDto updateDto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        user.FirstName = updateDto.FirstName;
        user.LastName = updateDto.LastName;
        user.Email = updateDto.Email;
        user.IsActive = updateDto.IsActive;

        var result = await userService.UpdateUserAsync(user, updateDto.UserPic);
        if (!result)
            return Ok(new { success = false, message = "Error al actualizar el usuario." });

        var rolCambiado = await userService.ActualizarRolAsync(user, updateDto.RolId);
        if (!rolCambiado)
            return Ok(new { success = false, message = "Error al actualizar el rol." });

        return Ok(new { success = true, message = "Usuario actualizado correctamente." });
    }

    [Authorize(Roles = "🛡️ Admin")]
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var user = await userService.GetByIdAsync(id);
        if (user == null) return NotFound();

        var result = await userService.DeleteUserAsync(id);
        if (!result) return BadRequest("Error al eliminar el usuario.");

        return Ok("Usuario eliminado correctamente.");
    }

    [Authorize]
    [HttpGet("ByUserName/{userName}")]
    public async Task<IActionResult> GetByUserName(string userName)
    {
        var user = await userService.GetByUserNameAsync(userName);
        if (user == null) return NotFound();

        return Ok(user);
    }

    //[Authorize(Roles = "🛡️ Admin")]
    [HttpGet("ObtenerRoles")]
    public async Task<IActionResult> ObtenerRoles()
    {
        var roles = await userService.ObtenerTodosLosRolesAsync();
        return Ok(roles);
    }
}
