using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models.Materiales;

public class Material : AuditableEntity
{
    public string Descripcion { get; set; } = string.Empty;
    [Required]
    public Guid TipoId { get; set; }
    public Guid UnidadMedidaId { get; set; } // 🧩 Relación con Tipo (foreign key)
    public virtual Tipo? Tipo { get; set; } // Propiedad de navegación
    public virtual UnidadMedida? UnidadMedida { get; set; } // Propiedad de navegación
    [MaxLength(300)]
    public string? ImagePath { get; set; }

    [MaxLength(300)]
    public string? LocalImagePath { get; set; }
    public IFormFile? Image { get; set; }
    public bool IsFavorite { get; set; }
}

public class CreateMaterialDto : IdentityAuditable
{
    public string? Descripcion { get; set; } = string.Empty;

    [Required]
    public Guid? TipoId { get; set; } // FK Tipo

    [Required]
    public Guid? UnidadMedidaId { get; set; } // FK UnidadMedida

    // 🆕 Nuevos campos que sí vienen desde el front
    public int? CodigoBarra { get; set; }
    public decimal PesoUnitario { get; set; }
    public decimal PrecioUnitario { get; set; }

    /// <summary>
    /// 🖼️ Imagen cargada por el usuario (no se mapea a BD)
    /// </summary>
    [NotMapped]
    public IFormFile? Image { get; set; }
    public bool IsFavorite { get; set; }
}

public class ReadMaterialDto : AuditableEntity
{
    public string Descripcion { get; set; } = string.Empty;

    public string? ImagePath { get; set; }
    public string? LocalImagePath { get; set; }

    public Guid TipoId { get; set; }
    public string? TipoNombre { get; set; }

    public Guid UnidadMedidaId { get; set; }
    public string? UnidadNombre { get; set; }

    // 🟦 Nuevos campos
    public int? CodigoBarra { get; set; }
    public decimal PesoUnitario { get; set; }
    public decimal PrecioUnitario { get; set; }

    // 🔥 Campos de jerarquía para reporte
    public string? ClaseNombre { get; set; }
    public string? FamiliaNombre { get; set; }
    [NotMapped]
    public IFormFile? Image { get; set; }
    public bool IsFavorite { get; set; }
}
