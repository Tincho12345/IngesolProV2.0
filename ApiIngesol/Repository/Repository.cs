using ApiIngesol.Data;
using ApiIngesol.Models;
using ApiIngesol.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace ApiIngesol.Repository
{
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        public async Task<IEnumerable<T>> GetAllAsync(string includeProperties = "")
        {
            IQueryable<T> query = _dbSet;

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            // Detectar si existe la propiedad "Codigo"
            var codigoProp = typeof(T).GetProperty("Codigo");
            if (codigoProp != null)
            {
                // Si es int (o nullable int) -> usar EF.Property<int>
                var propType = codigoProp.PropertyType;
                if (propType == typeof(int) || propType == typeof(int?))
                {
                    query = query.OrderBy(e => EF.Property<int?>(e, "Codigo")); // nullable para ser seguro
                    return await query.ToListAsync();
                }

                // Si es string -> tratar nulos y ordenar correctamente
                if (propType == typeof(string))
                {
                    query = query
                        .OrderBy(e => EF.Property<string>(e, "Codigo") == null)
                        .ThenBy(e => EF.Property<string>(e, "Codigo"));
                    return await query.ToListAsync();
                }

                // Otros tipos -> intentar ordenar por su valor convertido a string (fallback)
                query = query
                    .OrderBy(e => EF.Property<object>(e, "Codigo").ToString());
                return await query.ToListAsync();
            }

            // Si no tiene "Codigo", devolvemos la lista ordenada por la primera propiedad pública legible (como antes)
            var resultList = await query.ToListAsync();

            var firstProp = typeof(T).GetProperties()
                                     .FirstOrDefault(p => p.CanRead && p.GetMethod?.IsPublic == true);

            if (firstProp != null)
            {
                var resultado = resultList.OrderBy(x => firstProp.GetValue(x));
                return resultado;
            }
            else
            {
                return resultList;
            }
        }

        public async Task<T?> GetByIdAsync(Guid id)
        {
            try
            {
                // Intenta buscar con Guid directamente
                // EF automáticamente trackea la entidad devuelta por FindAsync
                return await _dbSet.FindAsync(id);
            }
            catch (ArgumentException)
            {
                // Si falla, busca usando el Guid convertido a string
                string idString = id.ToString();
                return await _dbSet.FindAsync(idString);
            }
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, string includeProperties = "")
        {
            IQueryable<T> query = _dbSet.Where(predicate);

            if (typeof(T) == typeof(ItemPresupuesto))
            {
                query = query
                    .Include(x => ((ItemPresupuesto)(object)x).Material)
                        .ThenInclude(m => m.UnidadMedida)
                    .Include(x => ((ItemPresupuesto)(object)x).Presupuesto);
            }

            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var includeProperty in includeProperties.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProperty.Trim());
                }
            }

            return await query.ToListAsync();
        }

        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.AnyAsync(predicate);
        }

        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.FirstOrDefaultAsync(predicate);
        }

        public async Task<bool> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return await SaveAsync();
        }

        public async Task<bool> AddRangeAsync(IEnumerable<T> entities)
        {
            await _dbSet.AddRangeAsync(entities);
            return await SaveAsync();
        }

        public async Task<bool> UpdateAsync(T entity)
        {
            try
            {
                _dbSet.Update(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveAsync(T entity)
        {
            _dbSet.Remove(entity);
            return await SaveAsync();
        }

        public async Task<bool> RemoveByIdAsync(Guid id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null) return false;

            _dbSet.Remove(entity);
            return await SaveAsync();
        }

        public async Task<bool> RemoveRangeAsync(IEnumerable<T> entities)
        {
            _dbSet.RemoveRange(entities);
            return await SaveAsync();
        }

        public async Task<bool> SaveAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<T>> GetAllWithIncludesAsync(params string[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            foreach (var include in includeProperties)
                query = query.Include(include);

            return await query.ToListAsync();
        }

    }
}
