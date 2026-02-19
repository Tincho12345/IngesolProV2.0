namespace WebIngesol.Models;

public class RoleDto : IdentityAuditable
{
    public string? Name { get; set; }
}

public class Rol : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
}
