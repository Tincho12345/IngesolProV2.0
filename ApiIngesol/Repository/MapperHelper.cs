using AutoMapper;
namespace ApiIngesol.Repository;
public static class MapperHelper
{
    // 🔹 Mapeo genérico + filtro (para entidades simples)
    public static async Task<IEnumerable<TDto>> MapToDtoListAsync<TEntity, TDto>(
        IMapper mapper,
        IEnumerable<TEntity> entities,
        string? filter = null)
    {
        var dtos = await Task.Run(() => mapper.Map<IEnumerable<TDto>>(entities));

        // Si hay filtro, aplicarlo automáticamente
        if (!string.IsNullOrWhiteSpace(filter))
            dtos = FilterList(dtos, filter);

        return dtos;
    }

    // 🔹 Versión especial para Proyectos (sin filtro, porque se filtra después)
    public static async Task<IEnumerable<TDto>> MapToDtoProyectosListAsync<TEntity, TDto>(
        IMapper mapper,
        IEnumerable<TEntity> entities)
    {
        return await Task.Run(() => mapper.Map<IEnumerable<TDto>>(entities));
    }

    // 🔹 Filtro genérico por texto
    public static IEnumerable<T> FilterList<T>(IEnumerable<T> list, string filter)
    {
        if (string.IsNullOrWhiteSpace(filter))
            return list;

        var lowerFilter = filter.ToLowerInvariant();

        return list.Where(item =>
        {
            foreach (var prop in typeof(T).GetProperties())
            {
                var value = prop.GetValue(item)?.ToString();
                if (value != null && value.ToLowerInvariant().Contains(lowerFilter))
                    return true;
            }
            return false;
        });
    }
}
