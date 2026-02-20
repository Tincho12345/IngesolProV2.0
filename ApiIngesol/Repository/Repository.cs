using ApiIngesol.Data;
using ApiIngesol.Repository.IRepository;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq.Dynamic.Core;

namespace ApiIngesol.Repository
{
    /// <summary>
    /// Repository genérico con:
    /// - Includes dinámicos
    /// - Filtro global dinámico (opcional)
    /// - Orden automático por "Codigo"
    /// </summary>
    public class Repository<T> : IRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly DbSet<T> _dbSet;

        public Repository(AppDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }

        // =========================================================
        // 🔎 GET ALL (INTERFAZ)
        // =========================================================
        public async Task<IEnumerable<T>> GetAllAsync(string includeProperties = "")
        {
            return await BuildQuery(includeProperties, null)
                .ToListAsync();
        }

        //// =========================================================
        //// 🔎 GET ALL WITH INCLUDES (INTERFAZ)
        //// =========================================================
        //public async Task<IEnumerable<T>> GetAllWithIncludesAsync(params string[] includeProperties)
        //{
        //    var includes = string.Join(",", includeProperties);
        //    return await BuildQuery(includes, null)
        //        .ToListAsync();
        //}

        // =========================================================
        // 🔎 GET ALL CON FILTRO (USADO POR SERVICE)
        // =========================================================
        //public async Task<IEnumerable<T>> GetAllFilteredAsync(
        //    string includeProperties,
        //    string filter)
        //{
        //    return await BuildQuery(includeProperties, filter)
        //        .ToListAsync();
        //}

        // =========================================================
        // 🔎 GET BY ID
        // =========================================================
        public async Task<T?> GetByIdAsync(Guid id)
            => await _dbSet.FindAsync(id);

        public async Task<IEnumerable<T>> FindAsync(
            Expression<Func<T, bool>> predicate,
            Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            IQueryable<T> query = _dbSet;

            if (include != null)
                query = include(query);

            return await query.Where(predicate).ToListAsync();
        }

        // =========================================================
        // 🔎 EXISTS
        // =========================================================
        public async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.AnyAsync(predicate);

        // =========================================================
        // 🔎 FIRST OR DEFAULT
        // =========================================================
        public async Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
            => await _dbSet.FirstOrDefaultAsync(predicate);

        // =========================================================
        // ➕ ADD
        // =========================================================
        public async Task<bool> AddAsync(T entity)
        {
            await _dbSet.AddAsync(entity);
            return await SaveAsync();
        }

        // =========================================================
        // ✏️ UPDATE
        // =========================================================
        public async Task<bool> UpdateAsync(T entity)
        {
            _dbSet.Update(entity);
            return await SaveAsync();
        }

        // =========================================================
        // 🗑 REMOVE
        // =========================================================
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

        // =========================================================
        // 💾 SAVE
        // =========================================================
        public async Task<bool> SaveAsync()
            => await _context.SaveChangesAsync() > 0;

        // =========================================================
        // 🔧 QUERY BUILDER CENTRAL
        // =========================================================
        private IQueryable<T> BuildQuery(
            string includeProperties,
            string? filter)
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            // 🔹 Includes
            if (!string.IsNullOrWhiteSpace(includeProperties))
            {
                foreach (var include in includeProperties
                    .Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(include.Trim());
                }
            }

            // 🔹 Filtro global dinámico
            if (!string.IsNullOrWhiteSpace(filter))
            {
                var expression = BuildGlobalFilter();
                if (!string.IsNullOrWhiteSpace(expression))
                {
                    query = query.Where(expression, filter.ToLower());
                }
            }

            // 🔹 Orden por Codigo si existe
            if (typeof(T).GetProperty("Codigo") != null)
            {
                query = query.OrderBy("Codigo");
            }

            return query;
        }

        // =========================================================
        // 🔎 GENERADOR FILTRO GLOBAL
        // =========================================================
        private static string BuildGlobalFilter()
        {
            var filters = new List<string>();

            foreach (var prop in typeof(T).GetProperties())
            {
                if (prop.PropertyType == typeof(string))
                {
                    filters.Add($"{prop.Name}.ToLower().Contains(@0)");
                }

                if (!prop.PropertyType.IsValueType &&
                    prop.PropertyType != typeof(string))
                {
                    foreach (var nested in prop.PropertyType
                        .GetProperties()
                        .Where(p => p.PropertyType == typeof(string)))
                    {
                        filters.Add($"{prop.Name}.{nested.Name}.ToLower().Contains(@0)");
                    }
                }
            }

            return filters.Count > 0
                ? string.Join(" OR ", filters)
                : string.Empty;
        }
    }
}
