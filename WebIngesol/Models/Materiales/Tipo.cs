using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models.Materiales;

public class Tipo : AuditableEntity
{
    [Required]
    public int Codigo { get; set; }
    [Required]
    public Guid ClaseId { get; set; }
    public Provincia Clase { get; set; } = null!;
    public string ClaseNombre { get; set; } = null!;
}

public class TipoDto : IdentityAuditable
{
    [Required]
    public int Codigo { get; set; }
    [Required]
    public Guid ClaseId { get; set; }
    public string? ClaseNombre { get; private set; } = null!;
}
