using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models
{
    public enum TipoPersona
    {
        Cliente,
        Proveedor,
        Ambos
    }
    public class Cliente : AuditableEntity
    {
        [Required]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NombreFantasia { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Domicilio { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [RegularExpression(@"^\d{2}-\d{8}-\d{1}$", ErrorMessage = "Formato de CUIT inválido (ej: 20-12345678-3).")]
        public string? CUIT { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de persona es obligatorio.")]
        public TipoPersona Tipo { get; set; }

        [StringLength(22, MinimumLength = 22, ErrorMessage = "El CBU debe tener exactamente 22 dígitos.")]
        [RegularExpression(@"^\d{22}$", ErrorMessage = "El CBU debe contener exactamente 22 dígitos numéricos.")]
        public string? Cbu { get; set; } = string.Empty;

        public ICollection<Contacto> GetContactos { get; set; } = new List<Contacto>();
    }

    public class ClienteDto : IdentityAuditable
    {
        [Required]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NombreFantasia { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Domicilio { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(100)]
        public string? Email { get; set; }

        [RegularExpression(@"^\d{2}-\d{8}-\d{1}$", ErrorMessage = "Formato de CUIT inválido.")]
        public string? CUIT { get; set; } = string.Empty;

        [Required(ErrorMessage = "El tipo de persona es obligatorio.")]
        public TipoPersona Tipo { get; set; }

        [StringLength(22, MinimumLength = 22, ErrorMessage = "El CBU debe tener exactamente 22 dígitos.")]
        [RegularExpression(@"^\d{22}$", ErrorMessage = "El CBU debe contener exactamente 22 dígitos numéricos.")]
        public string? Cbu { get; set; } = string.Empty;
    }

    public class ClienteReadDto : IdentityAuditable
    {
        [Required]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string NombreFantasia { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Domicilio { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
        [StringLength(100)]
        public string? Email { get; set; } = string.Empty;

        [RegularExpression(@"^\d{2}-\d{8}-\d{1}$", ErrorMessage = "Formato de CUIT inválido.")]
        public string? CUIT { get; set; } = string.Empty;

        public TipoPersona Tipo { get; set; }

        public string Cbu { get; set; } = string.Empty;
    }
}
