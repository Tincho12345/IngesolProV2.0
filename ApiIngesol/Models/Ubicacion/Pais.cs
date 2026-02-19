using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;

namespace ApiIngesol.Models.Ubicacion;

public class Pais : AuditableEntity
{
    [Required]
    [MaxLength(5)]
    public string Codigo { get; set; } = null!; // Ej: "ARG"

    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;

    public ICollection<Provincia> Provincias { get; set; } = new List<Provincia>();
}

public class PaisDto : IdentityAuditable
{
    [Required]
    [MaxLength(5)]
    public string Codigo { get; set; } = null!; // Ej: "ARG"

    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;
}