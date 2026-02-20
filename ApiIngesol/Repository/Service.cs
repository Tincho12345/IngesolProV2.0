using ApiIngesol.Repository.IRepository;
using System.Linq.Expressions;

namespace ApiIngesol.Repository;

public class Service<T>(IRepository<T> repository) : IService<T> where T : class
{
    private readonly IRepository<T> _repository = repository;

    public async Task<T?> GetByIdAsync(Guid id) =>  await _repository.GetByIdAsync(id);

    public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await _repository.FindAsync(predicate, null!);

    public async Task<bool> CreateAsync(T entity) => await _repository.AddAsync(entity);
    
    public async Task<bool> UpdateAsync(T entity) => await _repository.UpdateAsync(entity);
  
    public async Task<bool> DeleteAsync(Guid id) => await _repository.RemoveByIdAsync(id);

    public async Task<bool> DeleteRangeAsync(IEnumerable<T> entities) => await _repository.RemoveRangeAsync(entities);

    public async Task<IEnumerable<T>> GetAllAsync(string includeProperties = "", string filter = "")
    {
        var entities = await _repository.GetAllAsync(includeProperties);
        return entities;
    }

    public async Task<IEnumerable<T>> FindAsync(
        Expression<Func<T, bool>> predicate,
        Func<IQueryable<T>, IQueryable<T>>? include = null)
    {
        return await _repository.FindAsync(predicate, include);
    }
}
