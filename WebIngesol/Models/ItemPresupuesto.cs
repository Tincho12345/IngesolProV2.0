using WebIngesol.Models.Materiales;

namespace WebIngesol.Models;


public class ItemPresupuesto
{
    public Guid Id { get; set; }
    public Guid PresupuestoId { get; set; }
    public Guid MaterialId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PesoUnitario { get; set; }
    public decimal Total => Cantidad * PrecioUnitario;
    public string? UnidadMedidaNombre { get; set; } = string.Empty!;
}

public class ItemPresupuestoDto : IdentityAuditable
{
    public Guid PresupuestoId { get; set; }
    public Guid MaterialId { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PesoUnitario { get; set; }
    public decimal Total => Cantidad * PrecioUnitario;
    public string? UnidadMedidaNombre { get; set; } = string.Empty!;
}

public class ItemPresupuestoReadDto
{
    public Guid Id { get; set; }
    public Guid PresupuestoId { get; set; }
    public Guid MaterialId { get; set; }
    public string MaterialNombre { get; set; } = null!;
    public string UnidadMedidaNombre { get; set; } = string.Empty!;
    public string CodigoBarra { get; set; } = string.Empty!;   // 👈 AGREGAR ESTO
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal PesoUnitario { get; set; }
    public decimal Total { get; set; }
    // 👇👇 NUEVO
    public string? ImagePath { get; set; }
}
