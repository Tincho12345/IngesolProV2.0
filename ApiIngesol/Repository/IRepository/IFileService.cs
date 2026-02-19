namespace ApiIngesol.Repository.IRepository;

public interface IFileService<TEntity, TCreateDto>
    where TEntity : class
    where TCreateDto : class
{
    Task<bool> CreateAsync(TCreateDto dto);
    Task<bool> UpdateAsync(Guid id, TCreateDto dto);
}
