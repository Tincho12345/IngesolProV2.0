using ApiIngesol.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

namespace ApiIngesol.Controllers.Base;

/// <summary>
/// 🧱 Controlador genérico base para operaciones CRUD.
/// </summary>
//[Authorize(Roles = "🛡️ Admin,✏️ Data Entry")]
[ApiController]
[Route("api/[controller]")]
public abstract class GenericController<TEntity, TDto, TReadto>(IService<TEntity> service,
                                        IMapper mapper) : ControllerBase
                                        where TEntity : class
                                        where TDto : class
                                        where TReadto : class
{
    protected readonly IService<TEntity> _service = service;
    protected readonly IMapper _mapper = mapper;

    /// <summary>
    /// 📄 Obtiene todos los registros.
    /// </summary>
    [HttpGet]
    public virtual async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        var list = await _service.GetAllAsync(filter: filter ?? "");
        return Ok(list);
    }

    /// <summary>
    /// 🔍 Obtiene un recurso por su ID.
    /// </summary>
    [HttpGet("{id}")]
    public virtual async Task<IActionResult> GetById(Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        if (item == null) return NotFound();
        return Ok(item);
    }

    /// <summary>
    /// ➕ Crea un nuevo recurso BackEnd.
    /// </summary>
    /// <summary>
    /// ➕ Crea un nuevo recurso BackEnd.
    /// </summary>
    [HttpPost]
    public virtual async Task<IActionResult> Create([FromForm] TDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1️⃣ Mapear DTO → Entidad
        var entity = _mapper.Map<TEntity>(dto);

        // 2️⃣ Autocompletar Nombre SOLO si existe y está vacío
        var propNombre = typeof(TEntity).GetProperty("Nombre");
        if (propNombre != null && propNombre.PropertyType == typeof(string))
        {
            var valor = propNombre.GetValue(entity) as string;
            if (string.IsNullOrWhiteSpace(valor))
            {
                propNombre.SetValue(
                    entity,
                    $"{typeof(TEntity).Name} - {DateTime.Now:MMMM yyyy}"
                );
            }
        }

        // 3️⃣ Persistir
        var result = await _service.CreateAsync(entity);

        if (!result)
            return StatusCode(500, "Error al crear el recurso.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = GetEntityId(entity) },
            entity
        );
    }

    /// <summary>
    /// ✏️ Actualiza un recurso existente.
    /// </summary>
    [HttpPut("{id}")]
    public virtual async Task<IActionResult> Update(Guid id, [FromForm] TDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);
        
        var existing = await _service.GetByIdAsync(id);
        if (existing == null) return NotFound();
        _mapper.Map(dto, existing);

        // Guardar cambios
        var result = await _service.UpdateAsync(existing);

        if (!result)
            return StatusCode(500, "Error al actualizar.");

        return NoContent();
    }

    /// <summary>
    /// 🗑️ Elimina un recurso por su ID.
    /// </summary>
    [HttpDelete("{id}")]
    public virtual async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { message = "Recurso no encontrado." });

        var result = await _service.DeleteAsync(id);
        if (!result)
            return StatusCode(500, new { message = "Error al eliminar." });

        return NoContent();
    }

    /// <summary>
    /// 🔎 Filtra recursos por una propiedad Guid.
    /// </summary>
    [HttpGet("filter-by-guid")]
    public virtual async Task<IActionResult> GetByGuid([FromQuery] string propertyName, [FromQuery] Guid value)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "x");
        var property = Expression.PropertyOrField(parameter, propertyName);

        if (property.Type != typeof(Guid))
            return BadRequest($"La propiedad '{propertyName}' no es de tipo Guid.");

        var constant = Expression.Constant(value);
        var equality = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(equality, parameter);

        var results = await _service.FindAsync(lambda);
        return Ok(results);
    }

    /// <summary>
    /// 🔑 Obtiene el ID de la entidad.
    /// </summary>
    protected abstract Guid GetEntityId(TEntity entity);
}
