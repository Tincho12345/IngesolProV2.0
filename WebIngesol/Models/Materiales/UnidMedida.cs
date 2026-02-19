using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models.Materiales;

public class UnidadMedida : AuditableEntity
{
    [Required(ErrorMessage = "El Campo del Area Técnica es obligatorio.")]
    [StringLength(3, ErrorMessage = "El nombre del puesto no puede superar los 3 caracteres.")]
    public string Codigo { get; set; } = null!; // Ej: "UN"

    [Required(ErrorMessage = "Agregue una Descripción.")]
    [StringLength(250, ErrorMessage = "El campo Descripción no puede superar los 250 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;
}
public class UnidadMedidaDto : IdentityAuditable
{
    [Required(ErrorMessage = "El Campo del Area Técnica es obligatorio.")]
    [StringLength(3, ErrorMessage = "El nombre del puesto no puede superar los 3 caracteres.")]
    public string Codigo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Agregue una Descripción.")]
    [StringLength(250, ErrorMessage = "El campo Descripción no puede superar los 250 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;
}
