using System.Linq.Expressions;

namespace ApiIngesol.Repository.IRepository;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllWithIncludesAsync(params string[] includeProperties);
    Task<IEnumerable<T>> GetAllAsync(string includeProperties = "");
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string includeProperties = "");
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AddAsync(T entity);
    Task<bool> AddRangeAsync(IEnumerable<T> entities);
    Task<bool> UpdateAsync(T entity);
    Task<bool> RemoveAsync(T entity);
    Task<bool> RemoveByIdAsync(Guid id);
    Task<bool> RemoveRangeAsync(IEnumerable<T> entities);
    Task<bool> SaveAsync();
}
