using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;

namespace ApiIngesol.Models;

// 🏭 Entidad Planta
public class Planta : AuditableEntity
{
    public string? Direccion { get; set; }

    // Relación inversa - navegación
    public ICollection<Area> Areas { get; set; } = [];
}

public class PlantaDto : IdentityAuditable
{
    public string Direccion { get; set; } = string.Empty;
}