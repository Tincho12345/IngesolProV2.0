namespace WebIngesol.Models;

public class VisibleClientsViewModel
{
    public string ImageFolder { get; set; } = string.Empty;

    public string[] Images { get; set; } = [];

    public bool PuedeVer => Images.Length > 0;
}
