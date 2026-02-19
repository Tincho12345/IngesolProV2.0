using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;

namespace ApiIngesol.Models;

public enum EstadoSolicitudProyecto
{
    Nuevo = 1,
    Contactado = 2,
    PropuestaEnviada = 3,
    Cerrado = 4,
    Descartado = 5
}

public class SolicitudProyecto : AuditableEntity
{
    [Required]
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

    [Required]
    public string Mensaje { get; set; } = string.Empty;

    [Required]
    public EstadoSolicitudProyecto Estado { get; set; } = EstadoSolicitudProyecto.Nuevo;
}

public class CreateSolicitudProyectoDto
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

    // ✅ AGREGADO
    public EstadoSolicitudProyecto Estado { get; set; } = EstadoSolicitudProyecto.Nuevo;
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
