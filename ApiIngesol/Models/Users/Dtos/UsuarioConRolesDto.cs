namespace ApiIngesol.Models.Users.Dtos;

public class UsuarioConRolesDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    //public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }

    public List<string> Roles { get; set; } = [];

    // 👇 Estas son nuevas
    public string? RolId { get; set; }         // Para el dropdown
    public string RolNombre { get; set; } = ""; // Para mostrar en tabla
    public string ImagePath { get; set; } = ""; // Para mostrar en tabla
}
