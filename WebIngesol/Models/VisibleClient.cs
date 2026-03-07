namespace WebIngesol.Models;

public class VisibleClient
{
    public Guid Id { get; set; } = Guid.NewGuid(); // ID único
    public string Nombre { get; set; } = string.Empty;
    public string Imagen { get; set; } = string.Empty; // ahora será Id + extensión
    public string? Facebook { get; set; }
    public string? Twitter { get; set; }
    public string? Instagram { get; set; }
    public string? Telegram { get; set; }
    public string? Website { get; set; }
    public string? LinkedIn { get; set; }
    public string? WhatsApp { get; set; }
    public bool Activo { get; set; } = true; // para soft delete
    public int Orden { get; set; } = 0; // opcional: para ordenar manualmente

    // ==========================
    // NUEVAS PROPIEDADES UBICACIÓN
    // ==========================
    public double? Latitud { get; set; }   // opcional
    public double? Longitud { get; set; }  // opcional
}