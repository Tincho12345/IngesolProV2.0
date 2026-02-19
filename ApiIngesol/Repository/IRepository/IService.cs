using System.Linq.Expressions;

namespace ApiIngesol.Repository.IRepository;

public interface IService<T> where T : class
{
    // Método para obtener todos, con posibilidad de incluir propiedades relacionadas
    Task<IEnumerable<T>> GetAllAsync(string includeProperties = "", string filter = "");
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<bool> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> DeleteRangeAsync(IEnumerable<T> entities);
}
