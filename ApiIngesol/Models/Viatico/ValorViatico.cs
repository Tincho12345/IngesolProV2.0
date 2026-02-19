using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models.Viatico;

public class ValorViatico : AuditableEntity
{
    [Required]
    public DateTime FechaDesde { get; set; } // Fecha de vigencia

    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    [Required]
    public Guid TipoViaticoId { get; set; }

    [ForeignKey(nameof(TipoViaticoId))]
    public TipoViatico TipoViatico { get; set; } = null!;
}

// DTOs
public class ValorViaticoReadDto : AuditableEntity
{
    public DateTime FechaDesde { get; set; }
    public decimal Valor { get; set; }
    public Guid TipoViaticoId { get; set; }

    public string TipoViaticoNombre { get; set; } = string.Empty;
    public string? TipoViaticoObservaciones { get; set; } // 👈 NUEVO
}

public class ValorViaticoCreateDto : IdentityAuditable
{
    public DateTime FechaDesde { get; set; }
    public decimal Valor { get; set; }
    public Guid TipoViaticoId { get; set; }
}
