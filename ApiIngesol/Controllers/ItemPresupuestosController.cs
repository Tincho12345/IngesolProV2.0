using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using AutoMapper;
using ApiIngesol.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using ApiIngesol.Models.Materiales;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ItemPresupuestosController(
    IService<ItemPresupuesto> service,
    IMapper mapper,
    IRepository<ItemPresupuesto> itemPresupuestoRepository,
    IRepository<Presupuesto> presupuestoRepository,
    IRepository<Material> materialRepository
) : GenericController<ItemPresupuesto, ItemPresupuestoDto, ItemPresupuestoReadDto>(service, mapper)
{
    private readonly new IMapper _mapper = mapper;
    private readonly new IService<ItemPresupuesto> _service = service;
    private readonly IRepository<ItemPresupuesto> _itemRepository = itemPresupuestoRepository;
    private readonly IRepository<Presupuesto> _presupuestoRepository = presupuestoRepository;
    private readonly IRepository<Material> _materialRepository = materialRepository;

    protected override Guid GetEntityId(ItemPresupuesto entity) => entity.Id;

    // ===============================
    // 🔥 ACTUALIZAR TOTAL PRESUPUESTO
    // ===============================
    private async Task ActualizarTotalPresupuesto(Guid presupuestoId)
    {
        var items = await _itemRepository
            .FindAsync(x => x.PresupuestoId == presupuestoId);

        var total = items.Sum(x => x.Total);

        var presupuesto = await _presupuestoRepository.GetByIdAsync(presupuestoId);

        if (presupuesto is null) return;

        presupuesto.Total = total;
        await _presupuestoRepository.UpdateAsync(presupuesto);
    }

    // ===============================
    // CREATE
    // ===============================
    public override async Task<IActionResult> Create([FromForm] ItemPresupuestoDto dto)
    {
        var existing = await _itemRepository.FirstOrDefaultAsync(x =>
            x.PresupuestoId == dto.PresupuestoId &&
            x.MaterialId == dto.MaterialId
        );

        if (existing is not null)
        {
            existing.Cantidad += dto.Cantidad;
            existing.PrecioUnitario = dto.PrecioUnitario;
            existing.PesoUnitario = dto.PesoUnitario;

            await _itemRepository.UpdateAsync(existing);
            await ActualizarMaterial(existing.MaterialId, dto);
            await ActualizarTotalPresupuesto(existing.PresupuestoId);

            return Ok(existing);
        }

        var result = await base.Create(dto);

        if (result is CreatedAtActionResult created &&
            created.Value is ItemPresupuesto item)
        {
            await ActualizarMaterial(item.MaterialId, dto);
            await ActualizarTotalPresupuesto(item.PresupuestoId);
        }

        return result;
    }

    // ===============================
    // UPDATE
    // ===============================
    public override async Task<IActionResult> Update(Guid id, [FromForm] ItemPresupuestoDto dto)
    {
        dto.Id = id;

        var existing = await _service.GetByIdAsync(id);
        var result = await base.Update(id, dto);

        if (existing is null) return result;

        await ActualizarMaterial(existing.MaterialId, dto);
        await ActualizarTotalPresupuesto(existing.PresupuestoId);

        return result;
    }

    // ===============================
    // DELETE
    // ===============================
    public override async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _service.GetByIdAsync(id);
        var result = await base.Delete(id);

        if (existing is not null)
            await ActualizarTotalPresupuesto(existing.PresupuestoId);

        return result;
    }

    // ===============================
    // GET ALL (INCLUDE TIPADO)
    // ===============================
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        var entities = await _service.FindAsync(
            x => true,
            q => q
                .Include(x => x.Material)
                    .ThenInclude(m => m.UnidadMedida)
                .Include(x => x.Presupuesto)
        );

        var dtos = _mapper.Map<IEnumerable<ItemPresupuestoReadDto>>(entities);
        return Ok(dtos);
    }

    // ===============================
    // FILTER BY GUID (INCLUDE TIPADO)
    // ===============================
    [HttpGet("filter-by-guid")]
    public override async Task<IActionResult> GetByGuid(
        [FromQuery] string propertyName,
        [FromQuery] Guid value)
    {
        var parameter = Expression.Parameter(typeof(ItemPresupuesto), "x");
        var property = Expression.PropertyOrField(parameter, propertyName);

        if (property.Type != typeof(Guid))
            return BadRequest($"La propiedad '{propertyName}' no es de tipo Guid.");

        var constant = Expression.Constant(value);
        var equality = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<ItemPresupuesto, bool>>(equality, parameter);

        var entities = await _service.FindAsync(
            lambda,
            q => q
                .Include(x => x.Material)
                    .ThenInclude(m => m.UnidadMedida)
                .Include(x => x.Presupuesto)
        );

        var dtos = _mapper.Map<IEnumerable<ItemPresupuestoReadDto>>(entities);
        return Ok(dtos);
    }

    // ===============================
    // 🔥 ACTUALIZAR MATERIAL
    // ===============================
    private async Task ActualizarMaterial(Guid materialId, ItemPresupuestoDto dto)
    {
        var material = await _materialRepository.GetByIdAsync(materialId);

        if (material is null) return;

        material.PrecioUnitario = dto.PrecioUnitario;
        material.PesoUnitario = dto.PesoUnitario;

        await _materialRepository.UpdateAsync(material);
    }
}