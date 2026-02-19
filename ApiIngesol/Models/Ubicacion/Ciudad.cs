using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models.Ubicacion;

public class Ciudad : AuditableEntity
{
    [Required]
    public int Codigo { get; set; }

    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;

    [ForeignKey("Provincia")]
    public Guid ProvinciaId { get; set; }
    public Provincia Provincia { get; set; } = null!;
    public ICollection<Cliente> Clientes { get; set; } = new List<Cliente>();

}

public class CiudadDto : IdentityAuditable
{
    [Required]
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public Guid ProvinciaId { get; set; }
}

public class CiudadReadDto : AuditableEntity
{
    public int Codigo { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public Guid ProvinciaId { get; set; }
    public string? ProvinciaNombre { get; set; } = null!;
}
