namespace WebIngesol.Repository.IRepository;

public interface IRepository<T> where T : class
{
    // ————————————————————————————————————————————————————————
    // 🔍 Búsqueda por nombre o descripción (texto parcial o completo)
    // ————————————————————————————————————————————————————————
    Task<IEnumerable<T>> GetAllAsync(string url, string? filter = null);

    // ————————————————————————————————————————————————————————
    // 🆔 Obtener un único registro por ID
    // ————————————————————————————————————————————————————————
    Task<T?> GetByIdAsync(string url, Guid id);

    // ————————————————————————————————————————————————————————
    // ➕ Crear un nuevo registro (POST)
    // ————————————————————————————————————————————————————————
    Task<bool> CreateAsync(string url, T objToCreate);

    // ————————————————————————————————————————————————————————
    // ♻️ Actualizar un registro existente (PUT)
    // ————————————————————————————————————————————————————————
    Task<bool> UpdateAsync(string url, Guid id, T objToUpdate);

    // ————————————————————————————————————————————————————————
    // ❌ Eliminar un registro por ID (DELETE)
    // ————————————————————————————————————————————————————————
    Task<bool> DeleteAsync(string url, Guid id);

    // ————————————————————————————————————————————————————————
    // 🧲 Obtener todos los registros filtrando por propiedad Guid
    // ————————————————————————————————————————————————————————
    Task<IEnumerable<T>> GetAllByPropertyGuidAsync(string url, string propertyName, Guid value);

}
