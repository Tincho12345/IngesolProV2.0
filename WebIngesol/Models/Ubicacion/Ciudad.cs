using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models.Materiales;

public class Ciudad : AuditableEntity
{
    public int Codigo { get; set; }
    [Required]
    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;
    [Required]
    public Guid ProvinciaId { get; set; }
    public Provincia Provincia { get; set; } = null!;
    public string ProvinciaNombre { get; set; } = null!;
}

public class CiudadDto : AuditableEntity
{
    public int Codigo { get; set; }
    [Required]
    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;
    public Guid ProvinciaId { get; set; }
    public string? ProvinciaNombre { get; set; } = null!;
}
