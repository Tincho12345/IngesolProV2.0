namespace WebIngesol.Models
{
    // ================================
    //  Modelo principal (para tablas)
    // ================================
    public class RegistroAsistencia : AuditableEntity
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string EmpleadoNombre { get; set; } = string.Empty;

        public Guid PresupuestoId { get; set; }
        public string PresupuestoNombre { get; set; } = string.Empty;

        public DateTime HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }

        public string Observaciones { get; set; } = string.Empty;

        public TimeSpan? HorasTrabajadas =>
            HoraSalida.HasValue ? HoraSalida - HoraEntrada : null;
    }

    // ================================
    //  DTO para LECTURA (GET)
    // ================================
    public class RegistroAsistenciaReadDto : IdentityAuditable
    {
        public string EmpleadoId { get; set; } = string.Empty;
        public string EmpleadoNombre { get; set; } = string.Empty;

        public Guid PresupuestoId { get; set; }
        public string PresupuestoNombre { get; set; } = string.Empty;

        public DateTime HoraEntrada { get; set; }
        public DateTime? HoraSalida { get; set; }

        public DateTime CreatedDate { get; set; }
        public string CreatedBy { get; set; } = string.Empty;

        public DateTime ModifiedDate { get; set; }
        public string ModifiedBy { get; set; } = string.Empty;
        public string? HorasTrabajadas { get; set; }   // 👈 agregado
    }

    // ================================
    //  DTO para CREAR / EDITAR (POST/PUT)
    // ================================
    public class RegistroAsistenciaCreateDto
    {
        public string EmpleadoId { get; set; } = string.Empty;

        public Guid PresupuestoId { get; set; }

        public DateTime HoraEntrada { get; set; }

        public DateTime? HoraSalida { get; set; }

        public bool IsActive { get; set; } = true;

        public string? Observaciones { get; set; } = string.Empty;
    }
}
