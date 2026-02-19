using ApiIngesol.Models;
using ApiIngesol.Models.Users;
using ApiIngesol.Models.Users.Dtos;

namespace ApiIngesol.Repository.IRepository;

public interface IUserService : IService<ApplicationUser>
{
    Task<LoginResponseDto?> AuthenticateAsync(LoginDto loginDto);
    Task<List<RolDto>> ObtenerTodosLosRolesAsync();

    // Ahora incluyen IFormFile para la imagen del usuario
    Task<bool> CreateUserAsync(ApplicationUser user, string password, Guid rolId, IFormFile? imageFile);
    Task<bool> UpdateUserAsync(ApplicationUser user, IFormFile? imageFile);

    Task<ApplicationUser?> GetByEmailAsync(string email);
    Task<bool> ExistsWithEmailAsync(string email, Guid? excludeId = null);
    Task<List<UsuarioConRolesDto>> GetAllUsersAsync();
    Task<bool> ActualizarRolAsync(ApplicationUser user, Guid nuevoRolId);
    Task<UsuarioConRolesDto?> GetByIdWithRolesAsync(string id);
    Task<bool> DeleteUserAsync(Guid id);
    Task<ApplicationUser?> GetByUserNameAsync(string userName);
}
