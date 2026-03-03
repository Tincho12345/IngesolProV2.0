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
        new[] { ".jpg", ".jpeg", ".png", ".webp", ".jfif" };

    private string GetMasterJsonPath() => Path.Combine(_env.WebRootPath, "clients", "clientes.json");

    private async Task<List<VisibleClient>> ReadClientsAsync()
    {
        var path = GetMasterJsonPath();
        if (!System.IO.File.Exists(path)) return new List<VisibleClient>();
        var content = await System.IO.File.ReadAllTextAsync(path);
        return JsonSerializer.Deserialize<List<VisibleClient>>(content) ?? new List<VisibleClient>();
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
        IFormFile? imageFile)
    {
        if (string.IsNullOrWhiteSpace(nombre) || imageFile == null || imageFile.Length == 0)
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");

        var folderPath = Path.Combine(_env.WebRootPath, "clients");
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

        var client = new VisibleClient
        {
            Nombre = nombre.Trim(),
            Facebook = facebook?.Trim(),
            Twitter = twitter?.Trim(),
            Instagram = instagram?.Trim(),
            Telegram = telegram?.Trim(),
            Website = website?.Trim(),
            LinkedIn = linkedin?.Trim(),
            WhatsApp = !string.IsNullOrWhiteSpace(whatsapp) ? new string(whatsapp.Where(char.IsDigit).ToArray()) : null,
            Activo = true,
            Orden = 0
        };

        // Guardar imagen
        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");

        client.Imagen = $"{client.Id}{extension}"; // solo nombre + extensión
        var imagePath = Path.Combine(folderPath, client.Imagen);

        await using (var stream = new FileStream(imagePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        // Actualizar clientes.json
        var allClients = await ReadClientsAsync();
        allClients.Add(client);
        await SaveClientsAsync(allClients);

        return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");
    }

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
        IFormFile? imageFile)
    {
        if (!Guid.TryParse(id, out var clientId))
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");

        var allClients = await ReadClientsAsync();
        var client = allClients.FirstOrDefault(c => c.Id == clientId);
        if (client == null)
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");

        client.Nombre = nombre?.Trim() ?? string.Empty;
        client.Facebook = facebook?.Trim();
        client.Twitter = twitter?.Trim();
        client.Instagram = instagram?.Trim();
        client.Telegram = telegram?.Trim();
        client.Website = website?.Trim();
        client.LinkedIn = linkedin?.Trim();
        client.WhatsApp = !string.IsNullOrWhiteSpace(whatsapp) ? new string(whatsapp.Where(char.IsDigit).ToArray()) : null;

        var folderPath = Path.Combine(_env.WebRootPath, "clients");

        if (imageFile != null && imageFile.Length > 0)
        {
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
            if (_allowedExtensions.Contains(extension))
            {
                var newImagePath = Path.Combine(folderPath, $"{client.Id}{extension}");

                // Eliminar imagen vieja si cambia extensión
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
        return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");
    }

    // =============================
    // POST: Eliminar cliente (soft delete)
    // =============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string id)
    {
        if (!Guid.TryParse(id, out var clientId))
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");

        var allClients = await ReadClientsAsync();
        var client = allClients.FirstOrDefault(c => c.Id == clientId);
        if (client == null)
            return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");

        // Eliminar imagen física si existe
        var imagePath = Path.Combine(_env.WebRootPath, "clients", client.Imagen ?? "");
        if (!string.IsNullOrWhiteSpace(client.Imagen) && System.IO.File.Exists(imagePath))
            System.IO.File.Delete(imagePath);

        // REMOVER DEL JSON
        allClients.Remove(client);

        await SaveClientsAsync(allClients);
        return Redirect(Request.GetTypedHeaders().Referer?.ToString() ?? "/");
    }
}