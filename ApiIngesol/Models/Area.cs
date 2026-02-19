using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using ApiIngesol.Models.Auditorias;

namespace ApiIngesol.Models;

// 🏗️ Entidad Area
public class Area : AuditableEntity
{
    [Required]
    public Guid PlantaId { get; set; }

    [ForeignKey(nameof(PlantaId))]
    public Planta Planta { get; set; } = null!;
    public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
}

// Para Crear/Actualizar Area
public class AreaDto : IdentityAuditable
{
    [Required]
    public Guid PlantaId { get; set; }
    public ICollection<Proyecto> Proyectos { get; set; } = new List<Proyecto>();
}


// Para mostrar (lectura)
public class AreaReadDto : AuditableEntity
{
    public Guid PlantaId { get; set; }

    public string PlantaNombre { get; set; } = string.Empty;
    public ICollection<Proyecto> Proyectos { get; set; } =[];
}
