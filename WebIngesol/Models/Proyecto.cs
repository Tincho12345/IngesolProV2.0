using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models
{
    /// <summary>
    /// Modelo para visualizar un Proyecto en el Frontend
    /// </summary>
    public class Proyecto : AuditableEntity
    {
        [Required]
        public string Descripcion { get; set; } = string.Empty;

        [Required]
        public Guid AreaId { get; set; }

        public string? AreaNombre { get; set; }
        public string PlantaNombre { get; set; } = string.Empty;

        // 🔹 Total general de presupuestos por proyecto
        public decimal TotalPresupuestos { get; set; }

        // 🔹 Último número de orden (nuevo campo)
        public int? UltimoNumeroOrden { get; set; }
        public string UltimaDescripcionOrden { get; set; } = string.Empty;

        // 🔹 Un Proyecto tiene muchas Órdenes (resumen)
        public ICollection<OrdenResumen> Ordenes { get; set; } = [];
    }

    public class OrdenResumen
    {
        public int NumeroOrden { get; set; }
        public string DescripcionOrden { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
    }

    public class CreateProyectoDto : IdentityAuditable
    {
        [Required]
        public Guid AreaId { get; set; }

        [Required]
        public string Descripcion { get; set; } = string.Empty;
    }
}
