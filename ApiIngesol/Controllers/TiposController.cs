using ApiIngesol.Repository.IRepository;
using ApiIngesol.Controllers.Base;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using ApiIngesol.Models.Materiales;
using OfficeOpenXml;
using ApiIngesol.Data;
using Microsoft.EntityFrameworkCore;

namespace ApiIngesol.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TiposController(IService<Tipo> service, IMapper mapper) : GenericController<Tipo, TipoDto, TipoReadDto>(service, mapper)
{
    [HttpGet]
    public override async Task<IActionResult> GetAll([FromQuery] string? filter)
    {
        var entities = await _service.GetAllAsync("Clase", filter ?? "");
        var dtos = _mapper.Map<IEnumerable<TipoReadDto>>(entities);
        return Ok(dtos);
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

            var tipos = new List<TipoDto>();

            for (int row = 2; row <= worksheet.Dimension.End.Row; row++)
            {
                var claseIdStr = worksheet.Cells[row, 1]?.Text?.Trim();
                var codigoStr = worksheet.Cells[row, 2]?.Text?.Trim();
                var nombre = worksheet.Cells[row, 3]?.Text?.Trim();

                if (string.IsNullOrWhiteSpace(nombre) ||
                    !Guid.TryParse(claseIdStr, out var claseId) ||
                    !int.TryParse(codigoStr, out var codigo))
                    continue;

                tipos.Add(new TipoDto
                {
                    ClaseId = claseId,
                    Codigo = codigo,
                    Nombre = nombre
                });
            }

            int contador = 0;
            foreach (var dto in tipos)
            {
                var entity = _mapper.Map<Tipo>(dto);
                var result = await _service.CreateAsync(entity);
                if (result)
                    contador++;
            }

            return Ok($"{contador} tipos fueron importados correctamente.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al procesar el archivo Excel: {ex.Message}");
        }
    }

    [HttpDelete("eliminar-todos-sql")]
    public async Task<IActionResult> EliminarTodosConSql(
    [FromServices] AppDbContext context)
    {
        try
        {
            var cantidad = await context.Database.ExecuteSqlRawAsync("DELETE FROM Tipos");
            return Ok($"Se eliminaron {cantidad} registros de la tabla Tipos.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error al eliminar los registros: {ex.Message}");
        }
    }

    protected override Guid GetEntityId(Tipo entity) => entity.Id;
}
