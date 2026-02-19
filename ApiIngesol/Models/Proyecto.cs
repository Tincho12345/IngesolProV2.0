using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models;

/// <summary>
/// Entidad principal para Proyectos
/// </summary>
public class Proyecto : AuditableEntity
{
    [Required]
    public string Descripcion { get; set; } = string.Empty;

    [ForeignKey(nameof(AreaId))]
    public Area Area { get; set; } = null!;

    [Required]
    public Guid AreaId { get; set; }

    public ICollection<Orden> Ordenes { get; set; } = [];
}

/// <summary>
/// DTO para creación de Proyectos
/// </summary>
public class CreateProyectoDto : IdentityAuditable
{
    [Required]
    public Guid AreaId { get; set; }

    [Required]
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>
/// DTO para lectura de Proyectos
/// </summary>
public class ProyectoReadDto : AuditableEntity
{
    public Guid AreaId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string AreaNombre { get; set; } = string.Empty;
    public string PlantaNombre { get; set; } = string.Empty;
    public decimal TotalPresupuestos { get; set; }
    // 🔹 Nuevo campo: último número de orden
    public int? UltimoNumeroOrden { get; set; }
    public string UltimaDescripcionOrden { get; set; } = string.Empty;

    // 👇 NUEVO: lista opcional de órdenes (no rompe el front actual)
    public List<OrdenResumenDto> Ordenes { get; set; } = [];
}

public class OrdenResumenDto
{
    public int NumeroOrden { get; set; }
    public string? DescripcionOrden { get; set; }
    public decimal Subtotal { get; set; }
}
