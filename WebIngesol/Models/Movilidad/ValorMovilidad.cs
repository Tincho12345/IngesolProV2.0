namespace WebIngesol.Models.Movilidad;

public class ValorMovilidad : AuditableEntity
{
    public DateTime FechaDesde { get; set; }
    public decimal Valor { get; set; }

    public Guid TipoMovilidadId { get; set; }
    public TipoMovilidad TipoMovilidad { get; set; } = null!;
    public string TipoMovilidadNombre { get; set; } = string.Empty;
}

public class ValorMovilidadReadDto : AuditableEntity
{
    public DateTime FechaDesde { get; set; }
    public decimal Valor { get; set; }

    public Guid TipoMovilidadId { get; set; }
    public string tipoMovilidadNombre { get; set; } = string.Empty;
}

public class ValorMovilidadCreateDto
{
    public DateTime FechaDesde { get; set; }
    public decimal Valor { get; set; }
    public Guid TipoMovilidadId { get; set; }
}
