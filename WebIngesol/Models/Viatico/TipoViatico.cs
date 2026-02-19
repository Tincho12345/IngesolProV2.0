namespace WebIngesol.Models.Viaticos;

public class TipoViatico : AuditableEntity
{
    public string? Observaciones { get; set; }
}

public class TipoViaticoDto : IdentityAuditable
{
    public string? Observaciones { get; set; }
}
