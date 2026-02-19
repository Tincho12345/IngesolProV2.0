using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;

namespace ApiIngesol.Models.Materiales;

public class UnidadMedida : AuditableEntity
{
    [Required]
    public string Codigo { get; set; } = null!; // Ej: "UN"
    [Required]
    public string Descripcion { get; set; } = null!;
    public ICollection<Material> Materiales { get; set; } = new List<Material>();
}
public class UnidadMedidaDto : IdentityAuditable
{
    [Required]
    public string Codigo { get; set; } = null!;

    [Required]
    public string Descripcion { get; set; } = null!;
}