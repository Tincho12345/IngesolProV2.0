    using System.ComponentModel.DataAnnotations;

    namespace WebIngesol.Models.Movilidad;

    public class RegistroMovilidad : AuditableEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string userNombre { get; set; } = string.Empty;

        public Guid ValorMovilidadId { get; set; }
        public string TipoMovilidadNombre { get; set; } = string.Empty;

        public decimal Valor { get; set; }
        public Guid PresupuestoId { get; set; }
        public string PresupuestoNombre { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }
    }

    public class RegistroMovilidadReadDto : AuditableEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string userNombre { get; set; } = string.Empty;

        public Guid ValorMovilidadId { get; set; }
        public string TipoMovilidadNombre { get; set; } = string.Empty;

        public decimal Valor { get; set; }

        public Guid PresupuestoId { get; set; } = Guid.Empty; // ⚡ CORREGIDO
        public string PresupuestoNombre { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }
    }

    // DTO para creación desde front (UserId se asigna en backend)

public class RegistroMovilidadCreateDto
{
    // Opcional si el backend asigna UserId automáticamente
    public string UserId { get; set; } = string.Empty;

    [Required]
    public Guid ValorMovilidadId { get; set; }

    [Required]
    public Guid PresupuestoId { get; set; }

    [Required]
    public DateTime Fecha { get; set; }
    public bool IsActive { get; set; }
}
