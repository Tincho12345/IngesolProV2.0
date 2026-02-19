using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using AutoMapper;
using ApiIngesol.Models;
using Microsoft.AspNetCore.Mvc;
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
    private readonly IRepository<ItemPresupuesto> _itemPresupuestoRepository = itemPresupuestoRepository;
    private readonly IRepository<Presupuesto> _presupuestoRepository = presupuestoRepository;

    // 🔥 Repositorio de materiales
    private readonly IRepository<Material> _materialRepository = materialRepository;

    protected override Guid GetEntityId(ItemPresupuesto entity) => entity.Id;

    private async Task ActualizarTotalPresupuesto(Guid presupuestoId)
    {
        var items = await _itemPresupuestoRepository.FindAsync(x => x.PresupuestoId == presupuestoId);
        var total = items.Sum(x => x.Total);

        var presupuesto = await _presupuestoRepository.GetByIdAsync(presupuestoId);
        if (presupuesto != null)
        {
            presupuesto.Total = total;
            await _presupuestoRepository.UpdateAsync(presupuesto);
        }
    }

    public override async Task<IActionResult> Create([FromForm] ItemPresupuestoDto dto)
    {
        // Verificar si el item ya existe
        var existingItem = await _itemPresupuestoRepository.FirstOrDefaultAsync(x =>
            x.PresupuestoId == dto.PresupuestoId &&
            x.MaterialId == dto.MaterialId
        );

        if (existingItem != null)
        {
            existingItem.Cantidad += dto.Cantidad;
            existingItem.PrecioUnitario = dto.PrecioUnitario;
            existingItem.PesoUnitario = dto.PesoUnitario;

            await _itemPresupuestoRepository.UpdateAsync(existingItem);

            // 🔥 Actualizar precio del material
            var material = await _materialRepository.GetByIdAsync(existingItem.MaterialId);
            if (material != null)
            {
                material.PrecioUnitario = dto.PrecioUnitario;
                material.PesoUnitario = dto.PesoUnitario;
                await _materialRepository.UpdateAsync(material);
            }

            await ActualizarTotalPresupuesto(existingItem.PresupuestoId);

            return Ok(existingItem);
        }
        else
        {
            var result = await base.Create(dto);

            if (result is CreatedAtActionResult created && created.Value is ItemPresupuesto item)
            {
                // 🔥 Actualizar precio del material
                var material = await _materialRepository.GetByIdAsync(item.MaterialId);
                if (material != null)
                {
                    material.PrecioUnitario = dto.PrecioUnitario;
                    material.PesoUnitario = dto.PesoUnitario;
                    await _materialRepository.UpdateAsync(material);
                }

                await ActualizarTotalPresupuesto(item.PresupuestoId);
            }

            return result;
        }
    }

    public override async Task<IActionResult> Update(Guid id, [FromForm] ItemPresupuestoDto dto)
    {
        dto.Id = id;

        var existing = await _service.GetByIdAsync(id);
        var result = await base.Update(id, dto);

        if (existing != null)
        {
            // 🔥 Actualizar precio del material
            var material = await _materialRepository.GetByIdAsync(existing.MaterialId);
            if (material != null)
            {
                material.PrecioUnitario = dto.PrecioUnitario;
                material.PesoUnitario = dto.PesoUnitario;
                await _materialRepository.UpdateAsync(material);
            }

            await ActualizarTotalPresupuesto(existing.PresupuestoId);
        }

        return result;
    }

    public override async Task<IActionResult> Delete(Guid id)
    {
        var existing = await _service.GetByIdAsync(id);
        var result = await base.Delete(id);

        if (existing != null)
            await ActualizarTotalPresupuesto(existing.PresupuestoId);

        return result;
    }

    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        var entities = await _service.GetAllAsync("Material,Material.UnidadMedida,Presupuesto", filter ?? "");
        var dtos = _mapper.Map<IEnumerable<ItemPresupuestoReadDto>>(entities);
        return Ok(dtos);
    }

    [HttpGet("filter-by-guid")]
    public override async Task<IActionResult> GetByGuid([FromQuery] string propertyName, [FromQuery] Guid value)
    {
        var parameter = Expression.Parameter(typeof(ItemPresupuesto), "x");
        var property = Expression.PropertyOrField(parameter, propertyName);

        if (property.Type != typeof(Guid))
            return BadRequest($"La propiedad '{propertyName}' no es de tipo Guid.");

        var constant = Expression.Constant(value);
        var equality = Expression.Equal(property, constant);
        var lambda = Expression.Lambda<Func<ItemPresupuesto, bool>>(equality, parameter);

        var entities = await _service.FindAsync(lambda);
        var dtos = _mapper.Map<IEnumerable<ItemPresupuestoReadDto>>(entities);
        return Ok(dtos);
    }
}
