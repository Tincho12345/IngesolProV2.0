using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models.Ubicacion;

public class Provincia : AuditableEntity
{
    [Required]
    [Column(TypeName = "nvarchar(10)")]
    public string Codigo { get; set; } = null!;

    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;

    [ForeignKey("Pais")]
    public Guid PaisId { get; set; }
    public Pais Pais { get; set; } = null!;

    public ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
}

public class ProvinciaDto : IdentityAuditable
{
    [Required]
    public string Codigo { get; set; } = string.Empty!;

    [Required]
    public Guid PaisId { get; set; }
    public ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
}

public class ProvinciaReadDto : AuditableEntity
{
    [Required]
    public string Codigo { get; set; } = string.Empty!;
    public Guid PaisId { get; set; }
    public string PaisNombre { get; set; } = null!;
    public ICollection<Ciudad> Ciudades { get; set; } = new List<Ciudad>();
}
