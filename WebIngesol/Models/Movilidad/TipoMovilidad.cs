namespace WebIngesol.Models.Movilidad;

public class TipoMovilidad : AuditableEntity
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public ICollection<ValorMovilidad> Valores { get; set; } = [];
}

// DTO de lectura
public class TipoMovilidadDto : IdentityAuditable
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public ICollection<ValorMovilidadReadDto> Valores { get; set; } = [];
}

// DTO de Create
public class TipoMovilidadCreateDto : IdentityAuditable
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;
    public ICollection<ValorMovilidadCreateDto>? Valores { get; set; }
}
