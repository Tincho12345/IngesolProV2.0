using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ApiIngesol.Models.Auditorias;

namespace ApiIngesol.Models.Materiales;

public class Tipo : AuditableEntity
{
    [Required]
    public int Codigo { get; set; }

    [Required]
    public Guid ClaseId { get; set; }

    [ForeignKey(nameof(ClaseId))]
    public Clase Clase { get; set; } = null!;

    public ICollection<Material> Materiales { get; set; } = new List<Material>();
}

public class TipoDto : IdentityAuditable
{
    [Required]
    public int Codigo { get; set; }

    [Required]
    public Guid ClaseId { get; set; }
}

public class TipoReadDto : AuditableEntity
{
    public int Codigo { get; set; }
    public Guid ClaseId { get; set; }
    public string ClaseNombre { get; set; } = null!;
}