using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models.Materiales;

public class Material : AuditableEntity
{
    [Required]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public Guid TipoId { get; set; }

    [ForeignKey(nameof(TipoId))]
    public virtual Tipo? Tipo { get; set; }

    [Required]
    public Guid UnidadMedidaId { get; set; }

    [ForeignKey(nameof(UnidadMedidaId))]
    public virtual UnidadMedida? UnidadMedida { get; set; }

    [MaxLength(100)]
    public string? ImageHash { get; set; }

    [MaxLength(300)]
    public string? ImagePath { get; set; }

    [MaxLength(300)]
    public string? LocalImagePath { get; set; }

    // 🔵 NUEVOS CAMPOS
    public int? CodigoBarra { get; set; }
    public decimal? PesoUnitario { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public bool IsFavorite { get; set; }
}

// ================================
// 📌 CREATE DTO
// ================================
public class MaterialCreateDto : IdentityAuditable
{
    [Required]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public Guid TipoId { get; set; }

    [Required]
    public Guid UnidadMedidaId { get; set; }

    // Nuevos campos
    public int? CodigoBarra { get; set; }
    public decimal PesoUnitario { get; set; }
    public decimal PrecioUnitario { get; set; }

    [NotMapped]
    public IFormFile? Image { get; set; }
    public bool IsFavorite { get; set; }
}


// ================================
// 📌 READ DTO
// ================================
public class MaterialReadDto : AuditableEntity
{
    public string Descripcion { get; set; } = string.Empty;

    public string? ImagePath { get; set; }
    public string? LocalImagePath { get; set; }

    public Guid TipoId { get; set; }
    public string? TipoNombre { get; set; }

    public Guid UnidadMedidaId { get; set; }
    public string? UnidadNombre { get; set; }

    // 🟦 Nuevos campos agregados
    public int? CodigoBarra { get; set; }
    public decimal? PesoUnitario { get; set; }
    public decimal? PrecioUnitario { get; set; }

    // 🔥 NUEVOS CAMPOS PARA EL REPORTE
    public string? ClaseNombre { get; set; }
    public string? FamiliaNombre { get; set; }
    public bool IsFavorite { get; set; }
}
