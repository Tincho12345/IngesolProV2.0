using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models.Materiales;

public class Familia : AuditableEntity
{
    [Required]
    public int Codigo { get; set; } 
}

public class FamiliaDtos : IdentityAuditable
{
    [Required]
    public int Codigo { get; set; } 
}