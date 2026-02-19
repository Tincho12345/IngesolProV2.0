using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace WebIngesol.Models;

// 🧑‍💼 Clase principal de usuario (entidad de dominio)
public class Usuario: IdentityAuditable
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public Guid RolId { get; set; }

    // 👇 Contraseña por defecto
    [Required]
    [MinLength(6, ErrorMessage = "Debe tener al menos 9 caracteres.")]
    public string Password { get; set; } = "123456789?";

    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = "123456789?";
    public string? ImagePath { get; set; }
    public string? RolNombre { get; set; }
}


// 🧾 DTO principal para administración de usuarios
public class UsuarioDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    public Guid RolId { get; set; }
    public string RolNombre { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

// 🔐 DTO de solicitud de login
public class LoginRequest
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio")]
    public string NombreUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria")]
    public string Password { get; set; } = string.Empty;
}

// 🔐 DTO de respuesta de login (web client)
public class LoginResponse
{
    [JsonProperty("id")]
    public string Id { get; set; } = string.Empty;
    [JsonProperty("userName")]
    public string UserName { get; set; } = string.Empty;
    [JsonProperty("email")]
    public string Email { get; set; } = string.Empty;
    [JsonProperty("firstName")]
    public string FirstName { get; set; } = string.Empty;
    [JsonProperty("lastName")]
    public string LastName { get; set; } = string.Empty;
    [JsonProperty("fullName")]
    public string FullName { get; set; } = string.Empty;
    [JsonProperty("roles")]
    public List<string> Roles { get; set; } = new();
    [JsonProperty("token")]
    public string Token { get; set; } = string.Empty;
    [JsonProperty("expiration")]
    public DateTime Expiration { get; set; }
    [JsonProperty("imagePath")]
    public string? ImagePath { get; set; } = string.Empty;
    // 🔥 AGREGAR SOLO ESTO
    [JsonProperty("companyId")]
    public Guid? CompanyId { get; set; }
    public string Error { get; set; } = string.Empty;
}

// 🔐 DTO reducido para login (web client)
public class UsuarioLoginDto
{
    public string Id { get; set; } = string.Empty;

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;

    public List<string> Roles { get; set; } = [];
    // 👉 Propiedad calculada
    public string FullName => $"{FirstName} {LastName}";
}

// 📦 DTO para API genérica de autenticación
public class UsuarioAuthRespuesta
{
    public int StatusCode { get; set; }

    public bool IsSuccess { get; set; }

    public List<string> ErrorMessages { get; set; } = [];

    public ResultadoAuth Result { get; set; } = new();
}

public class ResultadoAuth
{
    public Usuario Usuario { get; set; } = new();

    public string Role { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

//Clase usada para que un usuario se registre al sistema
public class RegisterDto
{
    [Required]
    public string UserName { get; set; } = string.Empty;
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
    [Required]
    [PasswordCompleja]
    public string Password { get; set; } = string.Empty;
    [Required]
    [Compare("Password", ErrorMessage = "Las contraseñas no coinciden.")]
    public string ConfirmPassword { get; set; } = string.Empty;
    [Required]
    public string FirstName { get; set; } = string.Empty;
    [Required]
    public string LastName { get; set; } = string.Empty;
    [Required]
    public Guid RolId { get; set; }
    public IFormFile? UserPic { get; set; }
}

//Validaciones del PASSWORD
public class PasswordComplejaAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var password = value as string;

        if (string.IsNullOrWhiteSpace(password))
            return new ValidationResult("La contraseña es obligatoria.");

        if (password.Length < 9)
            return new ValidationResult("La contraseña debe tener al menos 9 caracteres.");

        if (!Regex.IsMatch(password, @"[A-Z]"))
            return new ValidationResult("La contraseña debe contener al menos una letra mayúscula.");

        if (!Regex.IsMatch(password, @"\d"))
            return new ValidationResult("La contraseña debe contener al menos un número.");

        return ValidationResult.Success;
    }
}

public class EditarPerfilDto : RegisterDto
{
   public string? ImagePath { get; set; }
}

