using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models.Viatico;

    // Modelo completo para uso interno / mapeo (backend)
    public class RegistroViatico : AuditableEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string EmpleadoNombre { get; set; } = string.Empty;
        public string ValorViaticoId { get; set; } = string.Empty;
        public string TipoViaticoNombre { get; set; } = string.Empty;
        public decimal Valor { get; set; }

        public string PresupuestoId { get; set; } = string.Empty;
        public string PresupuestoNombre { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }
    }

    // DTO para creación desde front (UserId se asigna en backend)
    public class RegistroViaticoCreateDto
    {
        // Opcional si el backend asigna UserId automáticamente
        public string UserId { get; set; } = string.Empty;

        [Required]
        public Guid ValorViaticoId { get; set; }

        [Required]
        public Guid PresupuestoId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO para lectura / consumo en front
    public class RegistroViaticoReadDto : AuditableEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string EmpleadoNombre { get; set; } = string.Empty;

        [Required]
        public Guid ValorViaticoId { get; set; }
        public string TipoViaticoNombre { get; set; } = string.Empty;
        public decimal Valor { get; set; }

        [Required]
        public Guid PresupuestoId { get; set; }
        public string PresupuestoNombre { get; set; } = string.Empty;

        [Required]
        public DateTime Fecha { get; set; }
    }

