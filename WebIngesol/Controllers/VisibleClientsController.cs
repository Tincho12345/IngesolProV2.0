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

    private string GetMasterJsonPath() => Path.Combine(_env.WebRootPath, "clients", "clientes.json");

    private async Task<List<VisibleClient>> ReadClientsAsync()
    {
        var path = GetMasterJsonPath();
        if (!System.IO.File.Exists(path)) return [];
        var content = await System.IO.File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<List<VisibleClient>>(content) ?? [];
    }

    private async Task SaveClientsAsync(List<VisibleClient> clients)
    {
        var path = GetMasterJsonPath();
        var json = JsonSerializer.Serialize(clients, _jsonOptions);
        await System.IO.File.WriteAllTextAsync(path, json);
    }

    // =============================
    // POST: Subir cliente
    // =============================
    // =============================
    // POST: Subir cliente con ubicación
    // =============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(
        string nombre,
        string? facebook,
        string? twitter,
        string? instagram,
        string? telegram,
        string? website,
        string? linkedin,
        string? whatsapp,
        double? latitud,
        double? longitud,
        IFormFile? imageFile)
    {
        if (string.IsNullOrWhiteSpace(nombre) || imageFile == null || imageFile.Length == 0)
            return BadRequest(new { message = "Datos inválidos." });

        // Crear carpeta si no existe
        var folderPath = Path.Combine(_env.WebRootPath, "clients");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // Crear objeto cliente
        var client = new VisibleClient
        {
            Nombre = nombre.Trim(),
            Facebook = facebook?.Trim(),
            Twitter = twitter?.Trim(),
            Instagram = instagram?.Trim(),
            Telegram = telegram?.Trim(),
            Website = website?.Trim(),
            LinkedIn = linkedin?.Trim(),
            WhatsApp = !string.IsNullOrWhiteSpace(whatsapp)
                ? new string(whatsapp.Where(char.IsDigit).ToArray())
                : null,
            Activo = true,
            Orden = 0,
            Latitud = latitud,     // <-- nuevo
            Longitud = longitud    // <-- nuevo
        };

        // Validar extensión de imagen
        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return BadRequest(new { message = "Extensión no permitida." });

        // Guardar imagen
        client.Imagen = $"{client.Id}{extension}";
        var imagePath = Path.Combine(folderPath, client.Imagen);
        await using (var stream = new FileStream(imagePath, FileMode.Create))
            await imageFile.CopyToAsync(stream);

        // Guardar en JSON
        var allClients = await ReadClientsAsync();
        allClients.Add(client);
        await SaveClientsAsync(allClients);

        return Ok(new { success = true });
    }


    // =============================
    // POST: Editar cliente
    // =============================
    // =============================
    // POST: Editar cliente
    // =============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        string id,
        string nombre,
        string? facebook,
        string? twitter,
        string? instagram,
        string? telegram,
        string? website,
        string? linkedin,
        string? whatsapp,
        double? latitud,
        double? longitud,
        IFormFile? imageFile)
    {
        if (!Guid.TryParse(id, out var clientId))
            return BadRequest(new { message = "Id inválido." });

        var allClients = await ReadClientsAsync();
        var client = allClients.FirstOrDefault(c => c.Id == clientId);

        if (client == null)
            return NotFound(new { message = "Cliente no encontrado." });

        client.Nombre = nombre?.Trim() ?? string.Empty;
        client.Facebook = facebook?.Trim();
        client.Twitter = twitter?.Trim();
        client.Instagram = instagram?.Trim();
        client.Telegram = telegram?.Trim();
        client.Website = website?.Trim();
        client.LinkedIn = linkedin?.Trim();
        client.WhatsApp = !string.IsNullOrWhiteSpace(whatsapp)
            ? new string(whatsapp.Where(char.IsDigit).ToArray())
            : null;

        // ---- NUEVO: guardar coordenadas ----
        client.Latitud = latitud;
        client.Longitud = longitud;

        var folderPath = Path.Combine(_env.WebRootPath, "clients");

        if (imageFile != null && imageFile.Length > 0)
        {
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (_allowedExtensions.Contains(extension))
            {
                var newImagePath = Path.Combine(folderPath, $"{client.Id}{extension}");

                if (!string.Equals(client.Imagen, $"{client.Id}{extension}", StringComparison.OrdinalIgnoreCase))
                {
                    var oldImagePath = Path.Combine(folderPath, client.Imagen);
                    if (System.IO.File.Exists(oldImagePath))
                        System.IO.File.Delete(oldImagePath);

                    client.Imagen = $"{client.Id}{extension}";
                }

                await using var stream = new FileStream(newImagePath, FileMode.Create);
                await imageFile.CopyToAsync(stream);
            }
        }

        await SaveClientsAsync(allClients);

        return Ok(new { success = true });
    }

    // =============================
    // POST: Eliminar cliente (soft delete)
    // =============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (!Guid.TryParse(id, out var clientId))
            return BadRequest(new { message = "Id inválido." });

        var allClients = await ReadClientsAsync();
        var client = allClients.FirstOrDefault(c => c.Id == clientId);

        if (client == null)
            return NotFound(new { message = "Cliente no encontrado." });

        var imagePath = Path.Combine(_env.WebRootPath, "clients", client.Imagen ?? "");
        if (!string.IsNullOrWhiteSpace(client.Imagen) && System.IO.File.Exists(imagePath))
            System.IO.File.Delete(imagePath);

        allClients.Remove(client);
        await SaveClientsAsync(allClients);

        return Ok(new { success = true });
    }
}