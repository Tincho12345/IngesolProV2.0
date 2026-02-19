using System.ComponentModel.DataAnnotations;

namespace ApiIngesol.Models.Users.Dtos;

public class UpdateUserDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public Guid RolId { get; set; }

    public string? Password { get; set; }

    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string? ConfirmPassword { get; set; }

    // Opcional: solo si lo necesitás mostrar o usar en lógica
    public string? RolNombre { get; set; }
    public IFormFile? UserPic { get; set; }
}

