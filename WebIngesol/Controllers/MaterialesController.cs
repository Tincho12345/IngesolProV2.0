using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebIngesol.ConstantsRoute;
using WebIngesol.Controllers.Base;
using WebIngesol.Models.Materiales;
using WebIngesol.Repository.IRepository;

namespace WebIngesol.Controllers;

[Authorize(Roles = "🛡️ Admin, ✏️ Data Entry")]
public class MaterialesController
    : BaseController<ReadMaterialDto, CreateMaterialDto>
{
    private readonly IRepository<Tipo> _tipoRepository;
    private readonly IRepository<UnidadMedida> _unidadMedidaRepository;
    private readonly IRepository<ReadMaterialDto> _materialRepository;

    public MaterialesController(
        IRepository<ReadMaterialDto> repository,
        IRepository<Tipo> tipoRepository,
        IRepository<UnidadMedida> unidadMedidaRepository,
        IMapper mapper
    ) : base(repository, mapper, CT.Materiales)
    {
        _materialRepository = repository;
        _tipoRepository = tipoRepository;
        _unidadMedidaRepository = unidadMedidaRepository;
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerTipo()
    {
        var tipos = await _tipoRepository.GetAllAsync(CT.Tipos);
        return Json(tipos
            .OrderBy(f => f.Nombre)
            .Select(f => new { id = f.Id, nombre = f.Nombre }));
    }

    [HttpGet]
    public async Task<IActionResult> ObtenerUnidadMedida()
    {
        var unidades = await _unidadMedidaRepository.GetAllAsync(CT.UnidadesMedida);
        return Json(unidades
            .OrderBy(f => f.Nombre)
            .Select(f => new { id = f.Id, nombre = f.Nombre }));
    }

    [HttpPatch]
    public async Task<IActionResult> ToggleFavorite(Guid id, [FromBody] bool isFavorite)
    {
        if (id == Guid.Empty)
            return BadRequest();

        var material = await _materialRepository.GetByIdAsync(CT.Materiales, id);
        if (material == null)
            return NotFound();

        material.IsFavorite = isFavorite;

        var success = await _materialRepository.UpdateAsync(CT.Materiales, id, material);
        if (!success)
            return StatusCode(500);

        return Ok();
    }
}
