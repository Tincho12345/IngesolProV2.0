namespace ApiIngesol.Models.Users.Dtos;

public class LoginResponseDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public List<string> Roles { get; set; } = [];
    public string Token { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string? ImagePath { get; set; }

    // 🔥 NUEVO
    public Guid? CompanyId { get; set; }
}
