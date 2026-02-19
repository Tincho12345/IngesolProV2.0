using ApiIngesol.Models.Auditorias;
using ApiIngesol.Models.Users;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models
{

    public class Orden : AuditableEntity
    {
        [Required]
        public int NumeroOrden { get; set; }

        [MaxLength(250)]
        public string? DescripcionOrden { get; set; }

        [Required]
        public DateTime FechaInicio { get; set; }

        [Required]
        public DateTime FechaFin { get; set; }

        [Required]
        public EstadoOrden Estado { get; set; }

        // 🔹 Proyecto
        [Required]
        public Guid ProyectoId { get; set; }

        [ForeignKey(nameof(ProyectoId))]
        public virtual Proyecto Proyecto { get; set; } = null!;

        // 🔹 Responsable (USUARIO)
        [Required]
        public string ResponsableId { get; set; } = null!;

        [ForeignKey(nameof(ResponsableId))]
        public virtual ApplicationUser Responsable { get; set; } = null!;

        // 🔹 Presupuestos
        public virtual ICollection<Presupuesto> Presupuestos { get; set; } = [];
    }

    public class OrdenDto : IdentityAuditable
    {
        public int NumeroOrden { get; set; }
        public string? DescripcionOrden { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public EstadoOrden Estado { get; set; }

        // 🔹 Solo los IDs de relaciones
        public Guid ProyectoId { get; set; }
        public Guid ResponsableId { get; set; }
    }

    public class OrdenReadDto : AuditableEntity
    {
        public int NumeroOrden { get; set; }
        public string? DescripcionOrden { get; set; }
        public DateTime FechaInicio { get; set; }
        public DateTime FechaFin { get; set; }
        public EstadoOrden Estado { get; set; }
        public Guid ProyectoId { get; set; }
        public string ProyectoNombre { get; set; } = null!;
        public string AreaNombre { get; set; } = null!;      // 👈 nuevo
        public string PlantaNombre { get; set; } = null!;    // 👈 nuevo
        public Guid ResponsableId { get; set; }
        public string ResponsableNombre { get; set; } = null!;
    }

    public enum EstadoOrden
    {
        [Display(Name = "📂 Abierta")]
        Abierta,
        [Display(Name = "⏳ En Curso")]
        EnCurso,
        [Display(Name = "🔒 Cerrada")]
        Cerrada,
        [Display(Name = "✅ Finalizada")]
        Finalizada,
        [Display(Name = "❌ Anulada")]
        Anulada
    }
}
