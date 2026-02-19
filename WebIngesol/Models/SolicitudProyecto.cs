using System.ComponentModel.DataAnnotations;
namespace WebIngesol.Models;

public enum EstadoSolicitudProyecto
{
    [Display(Name = "🆕 Nuevo")]
    Nuevo = 1,

    [Display(Name = "📞 Contactado")]
    Contactado = 2,

    [Display(Name = "📄 Propuesta enviada")]
    PropuestaEnviada = 3,

    [Display(Name = "✅ Cerrado")]
    Cerrado = 4,

    [Display(Name = "❌ Descartado")]
    Descartado = 5
}

public class SolicitudProyecto : AuditableEntity
{
    [EmailAddress]
    [MaxLength(150)]
    public string Email { get; set; } = string.Empty;

    [MaxLength(50)]
    public string? Telefono { get; set; }

    [MaxLength(100)]
    public string? TipoProyecto { get; set; }

    [MaxLength(150)]
    public string? Ubicacion { get; set; }

    [MaxLength(100)]
    public string? EtapaProyecto { get; set; }

    public string Mensaje { get; set; } = string.Empty;

    public EstadoSolicitudProyecto Estado { get; set; }
}

public class SolicitudProyectoDto
{
    [Required]
    public string Nombre { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public string? TipoProyecto { get; set; }

    public string? Ubicacion { get; set; }

    public string? EtapaProyecto { get; set; }

    [Required]
    public string Mensaje { get; set; } = string.Empty;

    // ✅ CLAVE
    public EstadoSolicitudProyecto Estado { get; set; }
}

public class SolicitudProyectoReadDto : AuditableEntity
{
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }

    public string? TipoProyecto { get; set; }
    public string? Ubicacion { get; set; }
    public string? EtapaProyecto { get; set; }
    public string Mensaje { get; set; } = string.Empty;
    public EstadoSolicitudProyecto Estado { get; set; }
}