using ApiIngesol.Models.Auditorias;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiIngesol.Models;

// ================================
// 📌 ENTIDAD
// ================================
public class BackgroundImage : AuditableEntity
{
    [MaxLength(300)]
    public string? ImageHash { get; set; }

    [Required]
    [MaxLength(300)]
    public string ImagePath { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? LocalImagePath { get; set; }

    public bool IsActiveBackground { get; set; }
}

// ================================
// 📌 CREATE DTO
// ================================
public class BackgroundImageCreateDto : IdentityAuditable
{
    [NotMapped]
    public IFormFile? Image { get; set; }
}

// ================================
// 📌 READ DTO
// ================================
public class BackgroundImageReadDto
{
    public Guid Id { get; set; }

    public string? ImagePath { get; set; }
    public string? LocalImagePath { get; set; }

    public bool IsActiveBackground { get; set; }
}
