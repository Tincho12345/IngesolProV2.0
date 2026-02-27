using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebIngesol.Models;

namespace WebIngesol.Controllers;

public class VisibleClientsController(IWebHostEnvironment env) : Controller
{
    private readonly IWebHostEnvironment _env = env;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly string[] _allowedExtensions =
        [".jpg", ".jpeg", ".png", ".webp"];

    // =============================
    // POST: Subir cliente
    // =============================
    [HttpPost]
    public async Task<IActionResult> Upload(
        IFormFile imageFile,
        string nombre,
        string? facebook,
        string? twitter,
        string? instagram,
        string? telegram)
    {
        if (imageFile == null || imageFile.Length == 0)
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");

        var folderPath = Path.Combine(_env.WebRootPath, "clients");
        Directory.CreateDirectory(folderPath);

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

        if (!_allowedExtensions.Contains(extension))
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");

        var fileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(folderPath, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        var client = new VisibleClient
        {
            Nombre = nombre?.Trim() ?? string.Empty,
            Imagen = fileName,
            Facebook = facebook?.Trim(),
            Twitter = twitter?.Trim(),
            Instagram = instagram?.Trim(),
            Telegram = telegram?.Trim()
        };

        var jsonPath = Path.Combine(folderPath, Path.ChangeExtension(fileName, ".json"));
        var json = JsonSerializer.Serialize(client, _jsonOptions);

        await System.IO.File.WriteAllTextAsync(jsonPath, json);

        return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");
    }

    // =============================
    // POST: Eliminar cliente
    // =============================
    [HttpPost]
    public IActionResult Delete(string fileName)
    {
        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var folderPath = Path.Combine(_env.WebRootPath, "clients");

            var safeFileName = Path.GetFileName(fileName);

            var imagePath = Path.Combine(folderPath, safeFileName);
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            var jsonPath = Path.Combine(folderPath, Path.ChangeExtension(safeFileName, ".json"));
            if (System.IO.File.Exists(jsonPath))
            {
                System.IO.File.Delete(jsonPath);
            }
        }

        return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");
    }
}