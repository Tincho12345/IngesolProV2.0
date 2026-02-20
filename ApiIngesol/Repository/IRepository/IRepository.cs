using System.Linq.Expressions;
namespace ApiIngesol.Repository.IRepository;

public interface IRepository<T> where T : class
{
    //Task<IEnumerable<T>> GetAllWithIncludesAsync(params string[] includeProperties);
    //Task<bool> AddRangeAsync(IEnumerable<T> entities);
    Task<IEnumerable<T>> GetAllAsync(string includeProperties = "");
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<T?> GetByIdAsync(Guid id);
    Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null
    );
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
    Task<bool> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> RemoveAsync(T entity);
    Task<bool> RemoveByIdAsync(Guid id);
    Task<bool> RemoveRangeAsync(IEnumerable<T> entities);
    Task<bool> SaveAsync();
}