using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models;

public class Presupuesto : AuditableEntity
{
    [Required]
    public Guid OrdenId { get; set; }

    [Required]
    public required Orden Orden { get; set; }

    [MaxLength(500)]
    public string Descripcion { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    //Relación uno a muchos con Items(opcional: si querés dejarlo descomentado)
    public virtual ICollection<ItemPresupuesto> Items { get; set; } = [];
}

public class PresupuestoDto : IdentityAuditable
{
    public Guid OrdenId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    //public decimal Total { get; set; }
    public List<ItemPresupuesto> Items { get; set; } = [];
}

public class PresupuestoReadDto : AuditableEntity
{
    public Guid OrdenId { get; set; }
    public int NumeroOrden { get; set; }
    //public string ClienteNombre { get; set; } = null!;
    public string Descripcion { get; set; } = string.Empty;
    public string PlantaNombre { get; set; } = string.Empty;
    public string AreaNombre { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<ItemPresupuesto> Items { get; set; } = [];
}
