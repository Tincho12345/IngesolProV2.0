using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models;
using ApiIngesol.Models.Users;
using Microsoft.AspNetCore.Identity;
using ApiIngesol.Repository;

namespace ApiIngesol.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdenesController(
        IService<Orden> service,
        IMapper mapper,
        UserManager<ApplicationUser> userManager
    ) : GenericController<Orden, OrdenDto, OrdenReadDto>(service, mapper)
    {
        [HttpGet]
        public override async Task<IActionResult> GetAll([FromQuery] string? filter)
        {
            var entities = await _service.GetAllAsync("Proyecto.Area.Planta");

            var dtos = await MapperHelper.MapToDtoListAsync<Orden, OrdenReadDto>(_mapper, entities, filter);

            var userIds = dtos.Select(d => d.ResponsableId.ToString()).Distinct().ToList();

            var users = userManager.Users
                .Where(u => userIds.Contains(u.Id))
                .Select(u => new { u.Id, u.LastName, u.FirstName })
                .ToList();

            foreach (var dto in dtos)
            {
                var responsable = users.FirstOrDefault(u => u.Id == dto.ResponsableId.ToString());
                dto.ResponsableNombre = responsable != null
                    ? $"{responsable.LastName.ToUpperInvariant()} {ToTitleCase(responsable.FirstName)}"
                    : "Desconocido";
            }

            // 🔥 ORDENAR POR NumeroOrden DESC
            dtos = dtos.OrderByDescending(d => d.NumeroOrden);

            return Ok(dtos);
        }

        protected override Guid GetEntityId(Orden entity) => entity.Id;

        private static string ToTitleCase(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return string.Empty;

            var words = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < words.Length; i++)
            {
                var word = words[i].ToLowerInvariant();
                words[i] = char.ToUpperInvariant(word[0]) + word[1..];
            }

            return string.Join(' ', words);
        }

        // 🗑️ DELETE personalizado (marca la orden como "Anulada" en lugar de eliminarla)
        [HttpDelete("{id}")]
        public override async Task<IActionResult> Delete(Guid id)
        {
            var orden = await _service.GetByIdAsync(id);
            if (orden == null)
                return NotFound(new { message = "Orden no encontrada." });

            var propEstado = orden.GetType().GetProperty(nameof(Orden.Estado));
            if (propEstado == null)
                return BadRequest(new { message = "La entidad no tiene una propiedad Estado." });

            var estadoActual = (EstadoOrden)propEstado.GetValue(orden)!;
            if (estadoActual == EstadoOrden.Anulada)
                return BadRequest(new { message = "La orden ya está anulada." });

            propEstado.SetValue(orden, EstadoOrden.Anulada);

            var result = await _service.UpdateAsync(orden);

            if (!result)
                return StatusCode(500, new { message = "Error al anular la orden." });

            return Ok(new { message = "Orden anulada correctamente." });
        }
    }
}
