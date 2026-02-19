using ApiIngesol.Models.Auditorias;

namespace ApiIngesol.Models;

public class Contacto : IdentityAuditable
{
    // Clave foránea
    public Guid ClienteId { get; set; }

    // Propiedad de navegación
    public Cliente Cliente { get; set; } = null!;
    public string NumeroTelefono { get; set; } = string.Empty;
}
public class ContactoDto : IdentityAuditable
{
    // Clave foránea
    public Guid ClienteId { get; set; }
    public string NumeroTelefono { get; set; } = string.Empty;
}
public class ContactoReadDto : AuditableEntity
{
    
}