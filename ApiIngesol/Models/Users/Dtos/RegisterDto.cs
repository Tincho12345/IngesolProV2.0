using System.ComponentModel.DataAnnotations;

namespace ApiIngesol.Models.Users.Dtos;

public class RegisterDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [Compare("Password")]
    public string ConfirmPassword { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    public Guid RolId { get; set; }

    // 🔥 ESTE CAMPO NUEVO
    [Required]
    public Guid CompanyId { get; set; }

    public IFormFile? UserPic { get; set; }
}
