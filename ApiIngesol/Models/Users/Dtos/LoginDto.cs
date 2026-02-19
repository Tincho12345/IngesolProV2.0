using System.ComponentModel.DataAnnotations;

namespace ApiIngesol.Models.Users.Dtos;
public class LoginDto
{
    [Required(ErrorMessage = "El nombre de usuario o correo electrónico es obligatorio.")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}
