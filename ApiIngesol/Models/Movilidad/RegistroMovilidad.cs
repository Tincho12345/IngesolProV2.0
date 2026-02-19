using ApiIngesol.Models.Auditorias;
using ApiIngesol.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models.Movilidad;

public class RegistroMovilidad : AuditableEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    [Required]
    public Guid ValorMovilidadId { get; set; }
    public required ValorMovilidad ValorMovilidad { get; set; }

    [Required]
    public DateTime Fecha { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    // ❗ Solo agrega si Movilidad también usa Presupuesto
    [Required]
    public Guid PresupuestoId { get; set; }

    [ForeignKey(nameof(PresupuestoId))]
    public Presupuesto Presupuesto { get; set; } = null!;
}

// DTO para creación
public class RegistroMovilidadCreateDto
{
    public string UserId { get; set; } = string.Empty;
    public Guid ValorMovilidadId { get; set; }
    public Guid PresupuestoId { get; set; }
    public DateTime Fecha { get; set; }
    public bool IsActive { get; set; }
}

public class RegistroMovilidadReadDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserNombre { get; set; } = string.Empty;

    public Guid ValorMovilidadId { get; set; }
    public string TipoMovilidadNombre { get; set; } = string.Empty;

    public decimal Valor { get; set; }

    public Guid PresupuestoId { get; set; }
    public string PresupuestoNombre { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    // Campos de auditoría (copiados manualmente)
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime ModifiedDate { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public Guid Id { get; set; }
}
