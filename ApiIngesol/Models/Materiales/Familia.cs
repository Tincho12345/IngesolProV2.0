using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;

namespace ApiIngesol.Models.Materiales;

public class Familia : AuditableEntity
{
    [Required]
    public int Codigo { get; set; }
    public ICollection<Clase> Clases { get; set; } = new List<Clase>();
}
public class FamiliaDto : IdentityAuditable
{
    [Required]
    public int Codigo { get; set; }
}

public class FamiliaReadDto : AuditableEntity
{
    [Required]
    public int Codigo { get; set; }
}
