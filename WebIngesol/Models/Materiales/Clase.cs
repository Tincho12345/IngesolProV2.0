using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models.Materiales;

public class Clase : AuditableEntity
{
    [Required]
    public int Codigo { get; set; } 
    [Required]
    public Guid FamiliaId { get; set; }
    public Familia Familia { get; set; } = null!;
    public string FamiliaNombre { get; set; } = null!;
}

public class ClaseDto : IdentityAuditable
{
    [Required]
    public int Codigo { get; set; }
    [Required]
    public Guid FamiliaId { get; set; }
    public string? FamiliaNombre { get; private set; } = null!;
}
