using ApiIngesol.Models.Auditorias;
using ApiIngesol.Models.Materiales;

namespace ApiIngesol.Models;

public class ItemPresupuesto
{
    public Guid Id { get; set; }

    public Guid PresupuestoId { get; set; }
    public Presupuesto Presupuesto { get; set; } = null!;

    public Guid MaterialId { get; set; }
    public Material Material { get; set; } = null!;

    public decimal Cantidad { get; set; }

    // Precio unitario editable (puede venir por defecto desde Material)
    public decimal PrecioUnitario { get; set; }

    // Peso unitario editable (puede venir por defecto desde Material)
    public decimal PesoUnitario { get; set; }

    // Campo calculado, no mapeado a base de datos
    public decimal Total => Cantidad * PrecioUnitario;
}

public class ItemPresupuestoDto : IdentityAuditable
{
    public Guid PresupuestoId { get; set; }
    
    public Guid MaterialId { get; set; }
   
    public decimal Cantidad { get; set; }

    public decimal PrecioUnitario { get; set; }

    public decimal PesoUnitario { get; set; }

    public decimal Total => Cantidad * PrecioUnitario;
    public string UnidadMedidaNombre { get; set; } = string.Empty!;
}

public class ItemPresupuestoReadDto
{
    public Guid Id { get; set; }
    public Guid PresupuestoId { get; set; }
    public Guid MaterialId { get; set; }

    public string MaterialNombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public string CodigoBarra { get; set; } = string.Empty;

    // 👇👇 NUEVO
    public string? ImagePath { get; set; }

    public string UnidadMedidaNombre { get; set; } = string.Empty!;

    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PesoUnitario { get; set; }

    public decimal Total => Cantidad * PrecioUnitario;
}
