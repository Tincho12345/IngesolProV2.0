using ApiIngesol.Models.Auditorias;
using ApiIngesol.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models;

public class RegistroAsistencia : AuditableEntity
{
    // 🔹 Relación con el empleado
    [Required]
    public string EmpleadoId { get; set; } = string.Empty;

    [Required]
    public virtual ApplicationUser Empleado { get; set; } = null!;

    // 🔹 Fecha y hora de entrada
    [Required]
    public DateTime HoraEntrada { get; set; }

    // 🔹 Fecha y hora de salida (null si aún no salió)
    public DateTime? HoraSalida { get; set; }

    // 🔹 Relación con el presupuesto / obra
    [Required]
    public Guid PresupuestoId { get; set; }

    [Required]
    public virtual Presupuesto Presupuesto { get; set; } = null!;

    // 🔹 Observaciones opcionales
    [MaxLength(500)]
    public string? Observaciones { get; set; } = string.Empty;

    // 🔹 Propiedad calculada para horas trabajadas
    [NotMapped]
    public TimeSpan? HorasTrabajadas => HoraSalida.HasValue ? HoraSalida - HoraEntrada : null;
}

public class RegistroAsistenciaCreateDto
{
    [Required]
    public Guid EmpleadoId { get; set; }
    [Required]
    public Guid PresupuestoId { get; set; }
    [Required]
    public DateTime HoraEntrada { get; set; }  // Cambiado
    public DateTime? HoraSalida { get; set; }  // Cambiado
    public bool IsActive { get; set; } = true;
    public string? Observaciones { get; set; } = string.Empty;
}

public class RegistroAsistenciaReadDto : IdentityAuditable
{
    public Guid EmpleadoId { get; set; }
    public string EmpleadoNombre { get; set; } = string.Empty;
    public Guid PresupuestoId { get; set; }
    public string PresupuestoNombre { get; set; } = string.Empty;
    public DateTime HoraEntrada { get; set; }  // Cambiado
    public DateTime? HoraSalida { get; set; }  // Cambiado
    public bool Activo { get; set; } = true;
    public DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime ModifiedDate { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public string? HorasTrabajadas { get; set; }   // 👈 agregado
}
