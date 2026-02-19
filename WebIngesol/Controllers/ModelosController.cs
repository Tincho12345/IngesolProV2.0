using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Models;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;
[Authorize(Roles = "🛡️ Admin")]
public class ModelosController(IRepository<Puesto> repo) : Controller
{
    private readonly IRepository<Puesto> _repo = repo;

    // 🔗 Ruta base para la API de Áreas Técnicas
    private readonly string _urlApi = CT.AreasTecnicas;

    // 📄 Vista principal (Index)
    [HttpGet]
    public IActionResult Index()
    {
        return View(new Puesto { });
    }

    // 📦 Obtener todos los registros (JSON)
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var entity = await _repo.GetAllAsync(_urlApi);
        return Json(entity);
    }

    // 🔍 Ver detalles de un recurso por ID
    public async Task<IActionResult> Details(Guid id)
    {
        var entity = await _repo.GetByIdAsync(_urlApi, id);
        if (entity == null) return NotFound();

        return View(entity);
    }

    // ➕ Crear un nuevo recurso
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Puesto entity)
    {
        if (!ModelState.IsValid)
        {
            // ❌ Datos inválidos: devuelve errores
            return BadRequest(new
            {
                success = false,
                message = "Modelo inválido",
                errors = ModelState
            });
        }

        var success = await _repo.CreateAsync(_urlApi, entity);
        if (!success)
        {
            // ❌ Fallo en la creación
            return BadRequest(new { success = false, message = "Error al guardar" });
        }

        // ✅ Recurso creado correctamente
        return Json(new { success = true, message = "Recurso creado correctamente" });
    }

    // 💾 Guardar cambios de un recurso existente (actualización)
    [HttpPost]
    public async Task<IActionResult> SaveChanges(Guid id, [FromBody] Puesto entity)
    {
        if (!ModelState.IsValid)
        {
            // ❌ Datos inválidos: devuelve errores
            return BadRequest(ModelState);
        }

        // 🔄 Actualiza el recurso
        var success = await _repo.UpdateAsync(_urlApi, id, entity);

        if (!success)
        {
            // ❌ Fallo en la actualización
            return BadRequest("No se pudo actualizar el área técnica.");
        }

        // ✅ Actualización exitosa, sin cuerpo
        return NoContent();
    }

    // ✏️ Obtener datos para editar un recurso
    public async Task<IActionResult> Edit(Guid id)
    {
        var entity = await _repo.GetByIdAsync(_urlApi, id);
        if (entity == null) return NotFound();

        return Json(entity);
    }

    // 🗑️ Eliminar un recurso
    [HttpDelete]
    public async Task<IActionResult> Delete(Guid id)
    {
        var registro = await _repo.DeleteAsync(_urlApi, id);

        if (!registro)
        {
            // ❌ Fallo al eliminar
            return Json(new { success = false, message = "Error al eliminar el recurso." });
        }

        // ✅ Eliminación exitosa
        return Json(new { success = true, message = "Recurso eliminado correctamente." });
    }
}
