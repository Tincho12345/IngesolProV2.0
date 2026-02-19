using System.ComponentModel.DataAnnotations;
using WebIngesol.Models.Ubicacion;

namespace WebIngesol.Models.Materiales;

public class Provincia : AuditableEntity
{
    public string Codigo { get; set; } = string.Empty;
    [Required]
    public Guid PaisId { get; set; }
    public Pais Pais { get; set; } = null!;
    public string PaisNombre { get; set; } = null!;
}

public class ProvinciaDto : AuditableEntity
{
    public string Codigo { get; set; } = string.Empty;
    [Required]
    public Guid PaisId { get; set; }
    public string? PaisNombre { get; private set; } = null!;
}
