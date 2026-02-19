using ApiIngesol.Data;
using ApiIngesol.Models;
using ApiIngesol.Models.Users;
using ApiIngesol.Models.Users.Dtos;
using ApiIngesol.Repository.IRepository;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ApiIngesol.Repository;

public class UserService(
    IRepository<ApplicationUser> repository,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IConfiguration configuration,
    AppDbContext context,
    IWebHostEnvironment env)
    : Service<ApplicationUser>(repository), IUserService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;
    private readonly IConfiguration _configuration = configuration;
    private readonly AppDbContext _context = context;
    private readonly RoleManager<ApplicationRole> _roleManager = roleManager;
    private readonly IWebHostEnvironment _env = env;

    public async Task<bool> CreateUserAsync(ApplicationUser user, string password, Guid rolId, IFormFile? imageFile)
    {
        ApplicationRole? rol; 

        if (rolId == Guid.Empty)
        {
            rol = await _roleManager.FindByNameAsync("👁️ Users");
            if (rol == null) return false;
        }
        else
        {
            rol = await _roleManager.Roles.FirstOrDefaultAsync(r => r.Id == rolId.ToString());
            if (rol == null) return false;
        }

        await HandleImageUploadForUser(imageFile, user, false);

        var result = await _userManager.CreateAsync(user, password);
        if (!result.Succeeded) return false;

        var roleAssignment = await _userManager.AddToRoleAsync(user, rol.Name!);
        return roleAssignment.Succeeded;
    }

    public async Task<bool> UpdateUserAsync(ApplicationUser user, IFormFile? imageFile)
    {
        var existingUser = await _userManager.FindByIdAsync(user.Id);
        if (existingUser == null) return false;

        existingUser.FirstName = user.FirstName;
        existingUser.LastName = user.LastName;
        existingUser.Email = user.Email;
        existingUser.UserName = user.UserName;

        await HandleImageUploadForUser(imageFile, existingUser, true);

        var result = await _userManager.UpdateAsync(existingUser);
        return result.Succeeded;
    }

    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return false;

        var result = await _userManager.DeleteAsync(user);

        if (result.Succeeded)
        {
            await DeleteImageIfUnusedAsync(user.ImageHash, user.LocalImagePath);
        }

        return result.Succeeded;
    }

    public async Task<ApplicationUser?> GetByEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<UsuarioConRolesDto?> GetByIdWithRolesAsync(string id)
    {
        return await _context.Users
            .Where(u => u.Id == id)
            .Select(user => new UsuarioConRolesDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                IsActive = user.IsActive,
                ImagePath = user.ImagePath ?? "/Images/Users/SinFoto.png",


                Roles = (from ur in _context.UserRoles
                         join r in _context.Roles on ur.RoleId equals r.Id
                         where ur.UserId == user.Id
                         select r.Name!).ToList(),

                RolId = (from ur in _context.UserRoles
                         where ur.UserId == user.Id
                         select ur.RoleId).FirstOrDefault(),

                RolNombre = (from ur in _context.UserRoles
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where ur.UserId == user.Id
                             select r.Name!).FirstOrDefault() ?? ""
            })
            .FirstOrDefaultAsync();
    }

    public async Task<List<UsuarioConRolesDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .Select(user => new UsuarioConRolesDto
            {
                Id = user.Id,
                UserName = user.UserName!,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email!,
                IsActive = user.IsActive,
                ImagePath = user.ImagePath ?? "/Images/Users/SinFoto.png",

                Roles = (from ur in _context.UserRoles
                         join r in _context.Roles on ur.RoleId equals r.Id
                         where ur.UserId == user.Id
                         select r.Name!).ToList(),

                RolId = (from ur in _context.UserRoles
                         where ur.UserId == user.Id
                         select ur.RoleId).FirstOrDefault(),

                RolNombre = (from ur in _context.UserRoles
                             join r in _context.Roles on ur.RoleId equals r.Id
                             where ur.UserId == user.Id
                             select r.Name!).FirstOrDefault() ?? ""
            })
            .ToListAsync();
    }

    public async Task<bool> ExistsWithEmailAsync(string email, Guid? excludeId = null)
    {
        return await Task.FromResult(_userManager.Users.Any(u => u.Email == email && (!excludeId.HasValue || u.Id != excludeId.ToString())));
    }

    public async Task<LoginResponseDto?> AuthenticateAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.NombreUsuario) ??
                   await _userManager.FindByEmailAsync(loginDto.NombreUsuario);

        if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
            return null;

        var roles = await _userManager.GetRolesAsync(user);

        var tokenHandler = new JwtSecurityTokenHandler();
        var secret = _configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(secret))
            throw new InvalidOperationException("JWT Key is not configured.");

        var key = Encoding.UTF8.GetBytes(secret);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),       
            // 🔥 CLAIM DE COMPANY
            new("CompanyId", user.CompanyId!.Value.ToString())
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);

        return new LoginResponseDto
        {
            Id = user.Id,
            UserName = user.UserName!,
            Email = user.Email!,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Roles = [.. roles],
            Token = tokenHandler.WriteToken(token),
            Expiration = token.ValidTo,
            ImagePath = user.ImagePath,
            // 🔥 AQUÍ
            CompanyId = user.CompanyId
        };
    }

    public async Task<List<RolDto>> ObtenerTodosLosRolesAsync()
    {
        return await _context.Roles
            .Select(r => new RolDto
            {
                Id = r.Id,
                Name = r.Name ?? string.Empty
            })
            .ToListAsync();
    }

    public async Task<bool> ActualizarRolAsync(ApplicationUser user, Guid nuevoRolId)
    {
        var rolesActuales = await _userManager.GetRolesAsync(user);
        var quitarRoles = await _userManager.RemoveFromRolesAsync(user, rolesActuales);
        if (!quitarRoles.Succeeded) return false;

        var nuevoRol = await _roleManager.FindByIdAsync(nuevoRolId.ToString());
        if (nuevoRol == null) return false;

        var agregarRol = await _userManager.AddToRoleAsync(user, nuevoRol.Name!);
        return agregarRol.Succeeded;
    }

    // ===============================
    // MÉTODOS PRIVADOS PARA IMÁGENES
    // ===============================

    private async Task HandleImageUploadForUser(IFormFile? file, ApplicationUser user, bool isUpdate)
    {
        if (file != null && file.Length > 0)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

            if (!allowed.Contains(ext)) throw new ArgumentException("Formato de imagen no permitido");

            var sizeMB = file.Length / 1024.0 / 1024.0;
            if (sizeMB > 2) throw new ArgumentException("La imagen no puede superar los 2MB.");

            var newHash = await ComputeFileHashAsync(file);
            string? oldHash = isUpdate ? user.ImageHash : null;
            string? oldPath = isUpdate ? user.LocalImagePath : null;

            var existing = await _context.Users.FirstOrDefaultAsync(u =>
                u.ImageHash == newHash &&
                !string.IsNullOrEmpty(u.ImagePath) &&
                !string.IsNullOrEmpty(u.LocalImagePath));

            if (existing != null)
            {
                SetUserImage(user, newHash, Path.GetFileName(existing.LocalImagePath!));
                if (isUpdate && oldHash != newHash)
                    await DeleteImageIfUnusedAsync(oldHash, oldPath);
                return;
            }

            var folder = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "Images", "Users");
            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{ext}";
            var fullPath = Path.Combine(folder, fileName);
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            SetUserImage(user, newHash, fileName);

            if (isUpdate && oldHash != null && oldHash != newHash)
                await DeleteImageIfUnusedAsync(oldHash, oldPath);
        }
        else if (!isUpdate)
        {
            user.ImagePath = "/Images/Users/SinFoto.png";
            user.LocalImagePath = "Images/Users/SinFoto.png";
            user.ImageHash = "hash-sin-foto";
        }
    }

    private static void SetUserImage(ApplicationUser user, string hash, string fileName)
    {
        user.ImageHash = hash;
        user.LocalImagePath = Path.Combine("Images", "Users", fileName);
        user.ImagePath = $"/Images/Users/{fileName}";
    }

    private static async Task<string> ComputeFileHashAsync(IFormFile file)
    {
        using var sha256 = SHA256.Create();
        using var stream = file.OpenReadStream();
        var hash = await sha256.ComputeHashAsync(stream);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private async Task DeleteImageIfUnusedAsync(string? hash, string? localPath)
    {
        if (string.IsNullOrWhiteSpace(hash) || string.IsNullOrWhiteSpace(localPath))
            return;

        var isUsed = await _context.Users.AnyAsync(u =>
            u.ImageHash == hash && u.LocalImagePath == localPath);

        if (!isUsed)
        {
            var fullPath = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), localPath);
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }
    }

    public async Task<ApplicationUser?> GetByUserNameAsync(string userName)
    {
        if (string.IsNullOrEmpty(userName))
            return null;

        return await _context.Users
            .FirstOrDefaultAsync(u => u.UserName == userName);
    }
}
