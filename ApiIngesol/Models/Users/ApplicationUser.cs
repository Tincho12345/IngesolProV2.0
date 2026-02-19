using ApiIngesol.Models.Ubicacion;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace ApiIngesol.Models.Users;

public class ApplicationUser : IdentityUser
{
    // ========================
    // Datos personales
    // ========================
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    public string FullName => $"{FirstName} {LastName}".Trim();

    // ========================
    // Auditoría básica
    // ========================
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedDate { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    // ========================
    // Imagen de perfil
    // ========================
    [MaxLength(100)]
    public string? ImageHash { get; set; }

    [MaxLength(300)]
    public string? ImagePath { get; set; }

    [MaxLength(300)]
    public string? LocalImagePath { get; set; }

    // ========================
    // Rol / puesto
    // ========================
    public Guid? PuestoId { get; set; }
    public Puesto? Puesto { get; set; }

    public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = [];

    // ========================
    // 🔥 MULTI-TENANT (CLAVE)
    // ========================
    public Guid? CompanyId { get; set; }     // FK
    public Company? Company { get; set; }    // navegación
}
