using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ApiIngesol.Models.Auditorias;

namespace ApiIngesol.Models.Materiales;

public class Clase : AuditableEntity
{
    [Required]
    public int Codigo { get; set; }

    [Required]
    public Guid FamiliaId { get; set; }

    [ForeignKey(nameof(FamiliaId))]
    public Familia Familia { get; set; } = null!;

    public ICollection<Tipo> Tipos { get; set; } = new List<Tipo>();
}
public class ClaseDto : IdentityAuditable
{
    [Required]
    public int Codigo { get; set; }

    [Required]
    public Guid FamiliaId { get; set; }
}

public class ClaseReadDto : AuditableEntity
{
    public int Codigo { get; set; }
    public Guid FamiliaId { get; set; }
    public string FamiliaNombre { get; set; } = null!;
}
