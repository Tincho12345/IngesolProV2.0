using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.Repository.IRepository;
using System.Security.Claims;

namespace WebIngesol.Controllers.Base;

/// <summary>
/// 🧱 Controlador base genérico para operaciones CRUD simples con vistas.
/// </summary>
//[Authorize(Roles = "🛡️ Admin, ✏️ Data Entry")]
public class BaseController<TModel, TCreateDto>(IRepository<TModel> repository,
    IMapper mapper,
    string apiUrl) : Controller where TModel : class
{
    private readonly IRepository<TModel> _repository = repository;
    private readonly IMapper _mapper = mapper;
    private readonly string _apiUrl = apiUrl;

    [HttpGet]
    public virtual IActionResult Index() => View();

    [HttpGet]
    public virtual async Task<IActionResult> GetAll(string? termino = null)
    {
        var resultados = await _repository.GetAllAsync(_apiUrl, termino);
        return Json(resultados);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Create([FromForm] TCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequestModelState();

        try
        {
            var entity = _mapper.Map<TModel>(dto);
            AsignarUserIdSiAplica(entity);

            var resultado = await _repository.CreateAsync(_apiUrl, entity);

            if (!resultado)
                return StatusCode(500, new { success = false, message = "Error al guardar el recurso en el servidor." });

            return Ok(new { success = true, message = "Recurso creado correctamente." });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Ocurrió un error inesperado al crear el recurso.",
                detail = ex.Message
            });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> SaveChanges(Guid id, [FromForm] TCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var entity = _mapper.Map<TModel>(dto);
        AsignarUserIdSiAplica(entity);

        var success = await _repository.UpdateAsync(_apiUrl, id, entity);

        if (!success)
            return BadRequest("No se pudo actualizar el recurso.");

        return NoContent();
    }

    [HttpGet]
    public virtual async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _repository.GetByIdAsync(_apiUrl, id);
        if (entity == null) return NotFound();

        return Json(entity);
    }

    [HttpDelete]
    public virtual async Task<IActionResult> Delete(Guid id)
    {
        var success = await _repository.DeleteAsync(_apiUrl, id);
        return Json(new
        {
            success,
            message = success ? "Recurso eliminado correctamente." : "Error al eliminar el recurso."
        });
    }

    [HttpGet]
    public virtual async Task<IActionResult> GetByPropertyGuid(string propertyName, Guid guid)
    {
        try
        {
            var result = await _repository.GetAllByPropertyGuidAsync(_apiUrl, propertyName, guid);
            return Json(result);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = "Ocurrió un error al obtener los datos.",
                detail = ex.Message
            });
        }
    }

    protected BadRequestObjectResult BadRequestModelState()
    {
        var errores = ModelState
            .Where(x => x.Value?.Errors?.Count > 0)
            .SelectMany(x => x.Value!.Errors!.Select(e => e.ErrorMessage))
            .ToList();

        string mensajeErrores = string.Join("; ", errores);

        return BadRequest(new
        {
            success = false,
            message = mensajeErrores
        });
    }

    private void AsignarUserIdSiAplica(object entity)
    {
        // 🌍 Usuario anónimo → no hacer nada
        if (!User.Identity?.IsAuthenticated ?? true)
            return;

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
            return;

        switch (entity)
        {
            case WebIngesol.Models.Viatico.RegistroViatico rv:
                rv.UserId = userId;
                break;

            case WebIngesol.Models.Movilidad.RegistroMovilidad rm:
                rm.UserId = userId;
                break;

            case WebIngesol.Models.RegistroAsistencia ra:
                ra.EmpleadoId = userId;
                break;
        }
    }
}
