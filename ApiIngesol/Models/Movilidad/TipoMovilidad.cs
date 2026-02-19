using ApiIngesol.Models.Auditorias;

namespace ApiIngesol.Models.Movilidad;

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

public class TipoMovilidadCreateDto : IdentityAuditable
{
    public string Desde { get; set; } = string.Empty;
    public string Hasta { get; set; } = string.Empty;
    public string Observaciones { get; set; } = string.Empty;

    // Si querés permitir creación de valores en la misma llamada,
    // dejás esta propiedad. Si no, ELIMINALA.
    public ICollection<ValorMovilidadCreateDto>? Valores { get; set; }
}
