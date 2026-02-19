using System.ComponentModel.DataAnnotations;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Models;

/// <summary>
/// Contiene Id e IsActive. Todas las entidades deben tenerlo.
/// </summary>
public class IdentityAuditable : IEntidadConNombre
{
    [Key]
    public Guid Id { get; set; }          // 👈 NO nullable
    public string? Nombre { get; set; }
    public bool IsActive { get; set; }
    Guid? IEntidadConNombre.Id { get => Id; set => throw new NotImplementedException(); }
}


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
