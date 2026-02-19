using ApiIngesol.Repository.IRepository;

namespace ApiIngesol.Models.Auditorias;

/// <summary>
/// Extiende a IdentityAuditable con campos de auditoría.
/// </summary>
public class AuditableEntity : IdentityAuditable
{
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime ModifiedDate { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
}

/// <summary>
/// Contiene Id e IsActive. Todas las entidades deben tenerlo.
/// </summary>
public abstract class IdentityAuditable : IAuditableEntity
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; } = true;
    public string Nombre { get; set; } = string.Empty;
}
