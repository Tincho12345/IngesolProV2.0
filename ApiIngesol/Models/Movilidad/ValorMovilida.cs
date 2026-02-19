using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models.Movilidad;
public class ValorMovilidad : AuditableEntity
{
    [Required]
    public DateTime FechaDesde { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Valor { get; set; }

    // FK a TipoMovilidad usando Guid
    public Guid TipoMovilidadId { get; set; }
    public required TipoMovilidad TipoMovilidad { get; set; }
}

// DTOs
public class ValorMovilidadReadDto : AuditableEntity
{
    public DateTime FechaDesde { get; set; }
    public decimal Valor { get; set; }
    public Guid TipoMovilidadId { get; set; }
    public string TipoMovilidadNombre { get; set; } = string.Empty;
}

public class ValorMovilidadCreateDto : IdentityAuditable
{
    public DateTime FechaDesde { get; set; }
    public decimal Valor { get; set; }
    public Guid TipoMovilidadId { get; set; }
}
