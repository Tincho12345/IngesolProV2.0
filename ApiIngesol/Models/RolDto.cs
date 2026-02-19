using Microsoft.AspNetCore.Identity;

namespace ApiIngesol.Models;

public class ApplicationRole : IdentityRole
{
    public bool IsActive { get; set; } = true;
}

public class RolDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}
public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
