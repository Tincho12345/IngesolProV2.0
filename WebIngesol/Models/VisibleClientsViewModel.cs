namespace WebIngesol.Models;

public class VisibleClientsViewModel
{
    public bool PuedeVer { get; set; } = true;
    public string ImageFolder { get; set; } = "clients";
    public string[] Images { get; set; } = Array.Empty<string>();
}