using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models.Ubicacion;

[Owned] // EF Core necesita esto para tratarlo como Value Object
public class Direccion
{
    [MaxLength(250)]
    public string NombreCalle { get; set; } = null!;

    public int Numero { get; set; }

    [MaxLength(250)]
    public string Descripcion { get; set; } = string.Empty;

    [Required]
    public Guid CiudadId { get; set; }
}