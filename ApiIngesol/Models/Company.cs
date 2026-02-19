using ApiIngesol.Models.Auditorias;

namespace ApiIngesol.Models;

public class Company : IdentityAuditable
{
    public string? LogoUrl { get; set; }
}
