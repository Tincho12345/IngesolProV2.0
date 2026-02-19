using ApiIngesol.Models.Materiales;

namespace ApiIngesol.Repository.IRepository;

public interface IMaterialService
{
    // ================================
    // 🔹 QUERIES OPTIMIZADAS (LECTURA)
    // ================================

    IQueryable<MaterialReadDto> QueryReadDto();

    Task<Material?> GetByIdAsync(Guid id);

    // ================================
    // 🔹 COMANDOS (WRITE)
    // ================================

    Task<bool> CreateAsync(MaterialCreateDto dto);

    Task<bool> UpdateAsync(Guid id, MaterialCreateDto dto);

    Task<bool> DeleteAsync(Guid id);

    // ================================
    // 🔹 VALIDACIONES
    // ================================

    Task<bool> ExistsAsync(string nombre, string descripcion);
}