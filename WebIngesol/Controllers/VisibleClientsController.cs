using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using WebIngesol.Models;
using Microsoft.AspNetCore.SignalR;
using WebIngesol.Hubs;

namespace WebIngesol.Controllers;

public class VisibleClientsController : Controller
{
    // 🔴 SIGNALR: contexto para poder enviar eventos a los navegadores
    private readonly IHubContext<ClientsHub> _hub;
    private readonly IWebHostEnvironment _env;

    // 🔴 SIGNALR: constructor actualizado para inyectar Hub + Environment
    public VisibleClientsController(IWebHostEnvironment env, IHubContext<ClientsHub> hub)
    {
        _env = env;
        _hub = hub;
    }

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true
    };

    private static readonly string[] _allowedExtensions =
        [".jpg", ".jpeg", ".png", ".webp", ".jfif"];

    private string GetMasterJsonPath() =>
        Path.Combine(_env.WebRootPath, "clients", "clientes.json");

    private async Task<List<VisibleClient>> ReadClientsAsync()
    {
        var path = GetMasterJsonPath();

        if (!System.IO.File.Exists(path))
            return [];

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

        var folderPath = Path.Combine(_env.WebRootPath, "clients");

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

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
            Latitud = latitud,
            Longitud = longitud
        };

        var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

        if (!_allowedExtensions.Contains(extension))
            return BadRequest(new { message = "Extensión no permitida." });

        client.Imagen = $"{client.Id}{extension}";

        var imagePath = Path.Combine(folderPath, client.Imagen);

        await using (var stream = new FileStream(imagePath, FileMode.Create))
            await imageFile.CopyToAsync(stream);

        var allClients = await ReadClientsAsync();

        allClients.Add(client);

        await SaveClientsAsync(allClients);

        // 🔴 SIGNALR: avisar a TODOS los navegadores que se actualizó la lista
        await _hub.Clients.All.SendAsync("ClientesActualizados");

        return Ok(new { success = true });
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

        // 🔴 SIGNALR: avisar a todos que se editó un cliente
        await _hub.Clients.All.SendAsync("ClientesActualizados");

        return Ok(new { success = true });
    }

    // =============================
    // POST: Eliminar cliente
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

        // 🔴 SIGNALR: avisar a todos que se eliminó un cliente
        await _hub.Clients.All.SendAsync("ClientesActualizados");

        return Ok(new { success = true });
    }

    // 🎬 Permitir solo estos formatos de video
    private static readonly string[] _allowedVideoExtensions = { ".mp4", ".webm", ".ogg" };

    // =============================
    // POST: Subir video
    // =============================
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequestSizeLimit(500_000_000)]
    public async Task<IActionResult> UploadVideo(IFormFile? videoFile)
    {
        if (videoFile == null || videoFile.Length == 0)
            return BadRequest(new { message = "Video inválido." });

        var extension = Path.GetExtension(videoFile.FileName).ToLowerInvariant();
        if (!_allowedVideoExtensions.Contains(extension))
            return BadRequest(new { message = "Formato de video no permitido." });

        var folderPath = Path.Combine(_env.WebRootPath, "videos");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        // 🔴 BORRAR TODO LO EXISTENTE EN LA CARPETA
        var existingVideos = Directory.GetFiles(folderPath);
        foreach (var file in existingVideos)
        {
            System.IO.File.Delete(file);
        }

        // 🔹 GUARDAR NUEVO VIDEO
        var fileName = $"hero{extension}";
        var videoPath = Path.Combine(folderPath, fileName);

        await using (var stream = new FileStream(videoPath, FileMode.Create))
            await videoFile.CopyToAsync(stream);

        // 🔹 AVISAR A LOS NAVEGADORES QUE CARGUEN EL NUEVO VIDEO
        await _hub.Clients.All.SendAsync("VideoActualizado", fileName);

        return Redirect("/#hero");
    }

}