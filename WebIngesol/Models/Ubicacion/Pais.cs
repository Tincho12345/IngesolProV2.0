using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models.Ubicacion;

public class Pais : AuditableEntity
{
    [Required]
    [MaxLength(3)]
    public string? Codigo { get; set; }
    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;
}

public class PaisDtos : IdentityAuditable
{
    [Required]
    public string? Codigo { get; set; }
    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;
}