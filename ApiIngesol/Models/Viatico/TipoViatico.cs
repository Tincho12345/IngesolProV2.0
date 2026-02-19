using ApiIngesol.Models.Auditorias;

namespace ApiIngesol.Models.Viatico;

// 🏭 Entidad Planta
public class TipoViatico : AuditableEntity
{
    public string? Observaciones { get; set; }

    // Relación inversa - navegación
    public ICollection<ValorViatico> ValorViaticos { get; set; } = [];
}

public class TipoViaticoDto : IdentityAuditable
{
    public string Observaciones { get; set; } = string.Empty;
}