namespace WebIngesol.Models;

public class Planta : AuditableEntity
{
    public string Direccion { get; set; } = string.Empty;
}
public class PlantaDto : IdentityAuditable
{
    public string Direccion { get; set; } = string.Empty;
}
