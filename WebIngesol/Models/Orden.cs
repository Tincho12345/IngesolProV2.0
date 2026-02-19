using System.ComponentModel.DataAnnotations;

namespace WebIngesol.Models;

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

public class Orden : AuditableEntity
{
    public int NumeroOrden { get; set; }
    [MaxLength(250)]
    public string? DescripcionOrden { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public EstadoOrden Estado { get; set; }
    // Relación con Proyecto
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

public class OrdenDto : IdentityAuditable
{
    public int NumeroOrden { get; set; }
    public string? DescripcionOrden { get; set; }
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    public EstadoOrden Estado { get; set; }
    public Guid ProyectoId { get; set; }
    public Guid ResponsableId { get; set; }
}