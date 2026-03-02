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
        [".jpg", ".jpeg", ".png", ".webp", ".jfif"];

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
        string? telegram,
        string? website,
        string? linkedin,      // NUEVO
        string? whatsapp)      // NUEVO
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

        // Limpieza básica WhatsApp (solo números)
        string? cleanedWhatsapp = null;
        if (!string.IsNullOrWhiteSpace(whatsapp))
        {
            cleanedWhatsapp = new string(
                whatsapp.Where(char.IsDigit).ToArray()
            );
        }

        var client = new VisibleClient
        {
            Nombre = nombre?.Trim() ?? string.Empty,
            Imagen = fileName,
            Facebook = facebook?.Trim(),
            Twitter = twitter?.Trim(),
            Instagram = instagram?.Trim(),
            Telegram = telegram?.Trim(),
            Website = website?.Trim(),
            LinkedIn = linkedin?.Trim(),          
            WhatsApp = cleanedWhatsapp           
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

    // =============================
    // POST: Editar cliente
    // =============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string fileName,
        string nombre,
        string? facebook,
        string? twitter,
        string? instagram,
        string? telegram,
        string? website,
        string? linkedin,
        string? whatsapp,
        IFormFile? imageFile)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");

        var folderPath = Path.Combine(_env.WebRootPath, "clients");
        var safeFileName = Path.GetFileName(fileName);

        var jsonPath = Path.Combine(folderPath, Path.ChangeExtension(safeFileName, ".json"));
        var imagePath = Path.Combine(folderPath, safeFileName);

        if (!System.IO.File.Exists(jsonPath))
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");

        // Leer JSON actual
        var jsonContent = await System.IO.File.ReadAllTextAsync(jsonPath);
        var client = JsonSerializer.Deserialize<VisibleClient>(jsonContent);

        if (client == null)
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");

        // Actualizar datos
        client.Nombre = nombre?.Trim() ?? string.Empty;
        client.Facebook = facebook?.Trim();
        client.Twitter = twitter?.Trim();
        client.Instagram = instagram?.Trim();
        client.Telegram = telegram?.Trim();
        client.Website = website?.Trim();
        client.LinkedIn = linkedin?.Trim();

        if (!string.IsNullOrWhiteSpace(whatsapp))
        {
            client.WhatsApp = new string(
                whatsapp.Where(char.IsDigit).ToArray()
            );
        }
        else
        {
            client.WhatsApp = null;
        }

        // Si sube nueva imagen
        if (imageFile != null && imageFile.Length > 0)
        {
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (_allowedExtensions.Contains(extension))
            {
                // Eliminar imagen vieja
                if (System.IO.File.Exists(imagePath))
                    System.IO.File.Delete(imagePath);

                var newFileName = $"{Guid.NewGuid()}{extension}";
                var newImagePath = Path.Combine(folderPath, newFileName);

                await using var stream = new FileStream(newImagePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);

                // Eliminar JSON viejo
                System.IO.File.Delete(jsonPath);

                // Actualizar nombre de imagen
                client.Imagen = newFileName;

                // Nuevo JSON
                jsonPath = Path.Combine(folderPath, Path.ChangeExtension(newFileName, ".json"));
            }
        }

        var updatedJson = JsonSerializer.Serialize(client, _jsonOptions);
        await System.IO.File.WriteAllTextAsync(jsonPath, updatedJson);

        return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");
    }
}