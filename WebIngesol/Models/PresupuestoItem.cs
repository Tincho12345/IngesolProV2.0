using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using WebIngesol.Models.Materiales;

namespace WebIngesol.Models;

public class PresupuestoItem : AuditableEntity
{
    [Required]
    public Guid PresupuestoId { get; set; }   // Cambiado a Guid
    public required virtual Presupuesto Presupuesto { get; set; } // Navegación al padre
    [Required]
    public Guid MaterialId { get; set; }       // FK a Material
    [Required]
    public required virtual Material Material { get; set; } // Navegación a Material
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }
    public decimal Cantidad { get; set; }
}
public class PresupuestoItemDto : IdentityAuditable
{
    public Guid MaterialId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public decimal Cantidad { get; set; }
}

public class PresupuestoItemReadDto : AuditableEntity
{
    public Guid PresupuestoId { get; set; }
    public Guid MaterialId { get; set; }
    public string MaterialNombre { get; set; } = null!;
    public string Descripcion { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public decimal Cantidad { get; set; }
    public decimal Total { get; set; }
}