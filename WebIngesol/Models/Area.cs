using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models;

public class Area : AuditableEntity
{
    public Guid PlantaId { get; set; }
    public string PlantaNombre { get; set; } = null!;
}

public class AreaDto : IdentityAuditable
{
    public Guid PlantaId { get; set; }
    public string? PlantaNombre { get; private set; } = string.Empty;
}