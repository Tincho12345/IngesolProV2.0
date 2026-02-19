using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models;

public class Puesto : AuditableEntity
{
    [Required]
    [MaxLength(10)]
    public string Codigo { get; set; } = null!;
    public string? Tareas { get; set; }

    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;

    public ICollection<ItemPresupuesto> ItemPresupuestos { get; set; } = new List<ItemPresupuesto>();
}

public class PuestoDto : IdentityAuditable
{
    [Required]
    [MaxLength(10)]
    public string Codigo { get; set; } = null!;
    public string? Tareas { get; set; }

    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;
    public ICollection<ItemPresupuesto> ItemPresupuestos { get; set; } = new List<ItemPresupuesto>();
}
