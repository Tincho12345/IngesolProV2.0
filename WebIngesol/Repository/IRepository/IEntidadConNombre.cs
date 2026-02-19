namespace WebIngesol.Repository.IRepository;

public interface IEntidadConNombre
{
    public Guid? Id { get; set; }
    public string? Nombre { get; set; }
}
