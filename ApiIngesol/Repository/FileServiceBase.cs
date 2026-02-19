using ApiIngesol.Repository.IRepository;
using AutoMapper;

namespace ApiIngesol.Repository;

public abstract class FileServiceBase<TEntity, TCreateDto>
    : IFileService<TEntity, TCreateDto>
    where TEntity : class
    where TCreateDto : class
{
    protected readonly IRepository<TEntity> _repository;
    protected readonly IWebHostEnvironment _env;
    protected readonly IMapper _mapper;

    protected FileServiceBase(
        IRepository<TEntity> repository,
        IWebHostEnvironment env,
        IMapper mapper)
    {
        _repository = repository;
        _env = env;
        _mapper = mapper;
    }

    public virtual async Task<bool> CreateAsync(TCreateDto dto)
    {
        var entity = _mapper.Map<TEntity>(dto);
        await HandleFileAsync(dto, entity, isUpdate: false);
        return await _repository.AddAsync(entity);
    }

    public virtual async Task<bool> UpdateAsync(Guid id, TCreateDto dto)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity == null) return false;

        _mapper.Map(dto, entity);
        await HandleFileAsync(dto, entity, isUpdate: true);
        return await _repository.UpdateAsync(entity);
    }

    protected abstract Task HandleFileAsync(
        TCreateDto dto,
        TEntity entity,
        bool isUpdate);
}
