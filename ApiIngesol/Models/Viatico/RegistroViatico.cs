using ApiIngesol.Models.Auditorias;
using ApiIngesol.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models.Viatico;

public class RegistroViatico : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [Required]
    public Guid ValorViaticoId { get; set; }

    [ForeignKey(nameof(ValorViaticoId))]
    public virtual ValorViatico ValorViatico { get; set; } = null!;

    [Required]
    public DateTime Fecha { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    // ⭐ NUEVO: relación directa al presupuesto
    [Required]
    public Guid PresupuestoId { get; set; }

    [ForeignKey(nameof(PresupuestoId))]
    public required Presupuesto Presupuesto { get; set; }
}

public class RegistroViaticoCreateDto
{
    public string UserId { get; set; } = string.Empty;
    public Guid ValorViaticoId { get; set; }
    public Guid PresupuestoId { get; set; }   // ⭐ agregado
    public DateTime Fecha { get; set; }
    public bool IsActive { get; set; }
}

public class RegistroViaticoReadDto : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string EmpleadoNombre { get; set; } = string.Empty;
    public Guid ValorViaticoId { get; set; }
    public string TipoViaticoNombre { get; set; } = string.Empty;
    public decimal Valor { get; set; }
    public Guid PresupuestoId { get; set; }
    public string PresupuestoNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
}
