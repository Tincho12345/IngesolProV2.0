namespace ApiIngesol.Repository.IRepository;

public interface IAuditableEntity
{
    string Nombre { get; set; }
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
}
