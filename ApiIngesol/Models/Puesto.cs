using ApiIngesol.Models.Auditorias;
using ApiIngesol.Models.Users;
using System.ComponentModel.DataAnnotations;

namespace ApiIngesol.Models;

public class Puesto : AuditableEntity
{
    [Required]
    [MaxLength(10)]
    public string Codigo { get; set; } = null!;
    public string? Tareas { get; set; }

    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;

    // Relación uno a muchos con Empleados
    public ICollection<ApplicationUser> Empleados { get; set; } =[];
}

public class PuestoDto: IdentityAuditable
{
    [Required]
    [MaxLength(10)]
    public string Codigo { get; set; } = null!;
    public string? Tareas { get; set; }

    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;
}