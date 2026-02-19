using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models;

public class Empleado : AuditableEntity
{
    [Required]
    [MaxLength(100)]
    public string Apellido { get; set; } = null!;
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = null!;
    [MaxLength(20)]
    public string? Telefono { get; set; }
    public DateTime FechaIngreso { get; set; }
    public DateTime FechaBaja { get; set; }
    public Guid PuestoId { get; set; }
    public Puesto Puesto { get; set; } = null!;
}

public class EmpleadoDto : IdentityAuditable
{
    [Required]
    [MaxLength(100)]
    public string Apellido { get; set; } = null!;
    [Required]
    [MaxLength(100)]
    public string Email { get; set; } = null!;
    [MaxLength(20)]
    public string? Telefono { get; set; }
    public DateTime FechaIngreso { get; set; }
    public DateTime FechaBaja { get; set; }
    public Guid PuestoId { get; set; }
    public PuestoDto? Puesto { get; set; }
    public IFormFile? UserPic { get; set; }
}
