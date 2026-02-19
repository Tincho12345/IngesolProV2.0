using ApiIngesol.Models.Materiales;
using ApiIngesol.Repository.IRepository;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;

namespace ApiIngesol.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MaterialesController : ControllerBase
{
    private readonly IMaterialService _materialService;
    private readonly IMapper _mapper;

    public MaterialesController(IMaterialService materialService, IMapper mapper)
    {
        ArgumentNullException.ThrowIfNull(materialService, nameof(materialService));
        ArgumentNullException.ThrowIfNull(mapper, nameof(mapper));

        _materialService = materialService;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        IQueryable<MaterialReadDto> query = _materialService.QueryReadDto();

        if (!string.IsNullOrWhiteSpace(filter))
        {
            var palabras = filter
                .Trim()
                .ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            foreach (var p in palabras)
            {
                query = query.Where(x =>
                    // 🔢 Código de barra
                    (x.CodigoBarra != null && x.CodigoBarra.Value.ToString().Contains(p))

                    // 🏷️ Nombre (heredado de IdentityAuditable)
                    || x.Nombre.ToLower().Contains(p)

                    // 📝 Descripción
                    || x.Descripcion.ToLower().Contains(p)

                    // 🧩 Clasificaciones
                    || (x.TipoNombre != null && x.TipoNombre.ToLower().Contains(p))
                    || (x.ClaseNombre != null && x.ClaseNombre.ToLower().Contains(p))
                    || (x.FamiliaNombre != null && x.FamiliaNombre.ToLower().Contains(p))

                    // 📏 Unidad de medida
                    || (x.UnidadNombre != null && x.UnidadNombre.ToLower().Contains(p))
                );
            }
        }

        query = query.OrderByDescending(x => x.CodigoBarra);

        var dtos = await query.ToListAsync();
        return Ok(dtos);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var entity = await _materialService.GetByIdAsync(id);
        if (entity == null) return NotFound();
        var dto = _mapper.Map<MaterialReadDto>(entity);
        return Ok(dto);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromForm] MaterialCreateDto dto)
    {
        var success = await _materialService.CreateAsync(dto);
        if (!success) return StatusCode(500, "Error al crear el material");
        return Ok("Material creado correctamente");
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromForm] MaterialCreateDto dto)
    {
        var success = await _materialService.UpdateAsync(id, dto);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _materialService.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpPost("importar-excel")]
    public async Task<IActionResult> ImportarDesdeExcel(IFormFile archivoExcel)
    {
        if (archivoExcel == null || archivoExcel.Length == 0)
            return BadRequest("No se proporcionó un archivo válido.");

        try
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using var stream = new MemoryStream();
            await archivoExcel.CopyToAsync(stream);
            stream.Position = 0;

            using var package = new ExcelPackage(stream);
            var worksheet = package.Workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                return BadRequest("El archivo Excel no contiene hojas.");

            int contador = 0;
            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var codigoBarra = 0;
                var nombre = worksheet.Cells[row, 2]?.Text?.Trim();
                var tipoIdStr = worksheet.Cells[row, 3]?.Text?.Trim();
                var unidadMedidaIdStr = worksheet.Cells[row, 4]?.Text?.Trim();
                var descripcion = worksheet.Cells[row, 5]?.Text?.Trim();

                if (string.IsNullOrWhiteSpace(nombre))
                    nombre = "Cargar luego...";

                if (string.IsNullOrWhiteSpace(descripcion))
                    descripcion = "Cargar luego...";

                if (!Guid.TryParse(tipoIdStr, out var tipoId))
                    continue;

                if (!Guid.TryParse(unidadMedidaIdStr, out var unidadMedidaId))
                    continue;

                var existe = await _materialService.ExistsAsync(nombre, descripcion);
                if (existe) continue;

                var dto = new MaterialCreateDto
                {
                    CodigoBarra = codigoBarra,
                    Nombre = nombre,
                    Descripcion = descripcion,
                    TipoId = tipoId,
                    UnidadMedidaId = unidadMedidaId
                };

                var result = await _materialService.CreateAsync(dto);
                if (result) contador++;
            }

            return Ok($"{contador} materiales fueron importados correctamente.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al procesar el archivo Excel: {ex.Message}");
        }
    }
}
