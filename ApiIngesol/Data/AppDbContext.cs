using ApiIngesol.Models;
using ApiIngesol.Models.Auditorias;
using ApiIngesol.Models.Materiales;
using ApiIngesol.Models.Movilidad;
using ApiIngesol.Models.Ubicacion;
using ApiIngesol.Models.Users;
using ApiIngesol.Models.Viatico;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace ApiIngesol.Data;

public partial class AppDbContext(
    DbContextOptions<AppDbContext> options,
    IHttpContextAccessor httpContextAccessor)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    #region DbSets
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<SolicitudProyecto> SolicitudesProyecto => Set<SolicitudProyecto>();

    public DbSet<AreaTecnica> AreasTecnicas => Set<AreaTecnica>();
    public DbSet<Clase> Clases => Set<Clase>();
    public DbSet<Familia> Familias => Set<Familia>();
    public DbSet<Material> Materiales => Set<Material>();
    public DbSet<Puesto> Puestos => Set<Puesto>();
    public DbSet<Tipo> Tipos => Set<Tipo>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<Pais> Paises => Set<Pais>();
    public DbSet<Provincia> Provincias => Set<Provincia>();
    public DbSet<Ciudad> Ciudades => Set<Ciudad>();
    public DbSet<Planta> Plantas => Set<Planta>();
    public DbSet<Area> Areas => Set<Area>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Contacto> Contactos => Set<Contacto>();
    public DbSet<Proyecto> Proyectos => Set<Proyecto>();
    public DbSet<Orden> Ordenes => Set<Orden>();
    public DbSet<Presupuesto> Presupuestos => Set<Presupuesto>();
    public DbSet<ItemPresupuesto> ItemPresupuestos => Set<ItemPresupuesto>();
    public DbSet<TipoViatico> TiposViaticos => Set<TipoViatico>();
    public DbSet<ValorViatico> ValoresViaticos => Set<ValorViatico>();
    public DbSet<RegistroViatico> RegistroViaticos => Set<RegistroViatico>();
    public DbSet<TipoMovilidad> TiposMovilidad => Set<TipoMovilidad>();
    public DbSet<ValorMovilidad> ValoresMovilidad => Set<ValorMovilidad>();
    public DbSet<RegistroMovilidad> RegistrosMovilidad => Set<RegistroMovilidad>();
    public DbSet<RegistroAsistencia> RegistrosAsistencias => Set<RegistroAsistencia>();
    public DbSet<BackgroundImage> BackgroundImages => Set<BackgroundImage>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<SolicitudProyecto>(entity =>
        {
            entity.Property(s => s.Email)
                  .HasMaxLength(150)
                  .IsRequired();

            entity.Property(s => s.Telefono)
                  .HasMaxLength(50);

            entity.Property(s => s.TipoProyecto)
                  .HasMaxLength(100);

            entity.Property(s => s.Ubicacion)
                  .HasMaxLength(150);

            entity.Property(s => s.EtapaProyecto)
                  .HasMaxLength(100);

            // ⚠️ NO default en base de datos para enums
            entity.Property(s => s.Estado)
                  .IsRequired();
        });

        modelBuilder.Entity<ItemPresupuesto>(entity =>
        {
            entity.Property(e => e.Cantidad).HasPrecision(18, 4);
            entity.Property(e => e.PesoUnitario).HasPrecision(18, 4);
            entity.Property(e => e.PrecioUnitario).HasPrecision(18, 4);
        });

        // ⓬ BackgroundImage (UI)
        modelBuilder.Entity<BackgroundImage>(entity =>
        {
            entity.Property(b => b.ImageHash)
                  .HasMaxLength(300);

            entity.Property(b => b.ImagePath)
                  .HasMaxLength(300)
                  .IsRequired();

            entity.Property(b => b.LocalImagePath)
                  .HasMaxLength(300);

            entity.Property(b => b.IsActiveBackground)
                  .HasDefaultValue(false);

            // ⚠️ Opcional pero RECOMENDADO:
            // garantiza que solo haya un fondo activo a la vez
            entity.HasIndex(b => b.IsActiveBackground)
                  .HasFilter("[IsActiveBackground] = 1")
                  .IsUnique();
        });


        // ❷ ValorViatico
        modelBuilder.Entity<ValorViatico>(entity =>
        {
            entity.Property(vv => vv.Valor).HasPrecision(18, 2);
        });

        // ❸ ItemPresupuesto
        modelBuilder.Entity<ItemPresupuesto>()
            .HasOne(pi => pi.Presupuesto)
            .WithMany(p => p.Items)
            .HasForeignKey(pi => pi.PresupuestoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ItemPresupuesto>()
            .HasOne(pi => pi.Material)
            .WithMany()
            .HasForeignKey(pi => pi.MaterialId)
            .OnDelete(DeleteBehavior.Restrict);

        // ❹ Orden → Proyecto
        modelBuilder.Entity<Orden>()
            .HasOne(o => o.Responsable)
            .WithMany() 
            .HasForeignKey(o => o.ResponsableId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // ❺ Proyecto → Area
        modelBuilder.Entity<Proyecto>()
            .HasOne(p => p.Area)
            .WithMany(a => a.Proyectos)
            .HasForeignKey(p => p.AreaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ❻ ApplicationRole
        modelBuilder.Entity<ApplicationRole>()
            .Property(r => r.IsActive)
            .HasDefaultValue(true);

        // ❼ Material
        modelBuilder.Entity<Material>(entity =>
        {
            entity.Property(m => m.ImageHash)
                  .HasDefaultValue("a24f0fee8d4bcce3f2409b33ea3d87e48f24bfcff1d06decfc698304bfeb9827");

            entity.Property(m => m.ImagePath)
                  .HasDefaultValue("/Images/Materials/SinImagen.png");

            entity.Property(m => m.LocalImagePath)
                  .HasDefaultValue("/Images/Materials/SinImagen.png");

            entity.Property(m => m.PesoUnitario)
                  .HasPrecision(18, 4);

            entity.Property(m => m.PrecioUnitario)
                  .HasPrecision(18, 4);
        });

        // ❽ Area → Planta
        modelBuilder.Entity<Area>()
            .HasOne(a => a.Planta)
            .WithMany(p => p.Areas)
            .HasForeignKey(a => a.PlantaId)
            .OnDelete(DeleteBehavior.Restrict);

        // ❾ Relaciones Material
        modelBuilder.Entity<Clase>()
            .HasOne(c => c.Familia)
            .WithMany(f => f.Clases)
            .HasForeignKey(c => c.FamiliaId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Tipo>()
            .HasOne(t => t.Clase)
            .WithMany(c => c.Tipos)
            .HasForeignKey(t => t.ClaseId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Material>()
            .HasOne(m => m.Tipo)
            .WithMany(t => t.Materiales)
            .HasForeignKey(m => m.TipoId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.HasOne(u => u.Company)
                  .WithMany() // o WithMany(c => c.Users) si luego lo agregas
                  .HasForeignKey(u => u.CompanyId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        // ➓ Ubicaciones
        modelBuilder.Entity<Pais>(entity =>
        {
            entity.Property(p => p.Codigo).HasMaxLength(3).IsRequired();
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.HasIndex(p => p.Nombre).IsUnique();
            entity.Property(p => p.Descripcion).HasMaxLength(250);
        });

        modelBuilder.Entity<Provincia>(entity =>
        {
            entity.Property(p => p.Codigo).HasMaxLength(2).IsRequired();
            entity.HasIndex(p => p.Codigo).IsUnique();
            entity.Property(p => p.Descripcion).HasMaxLength(250);
        });

        modelBuilder.Entity<Ciudad>(entity =>
        {
            entity.Property(c => c.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Codigo).HasMaxLength(4).IsRequired();
            entity.HasIndex(c => c.Codigo);
            entity.Property(c => c.Descripcion).HasMaxLength(250);
        });

        // ⓫ Cliente
        modelBuilder.Entity<Cliente>(entity =>
        {
            entity.Property(c => c.Codigo).HasMaxLength(20).IsRequired();
            entity.Property(c => c.NombreFantasia).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Domicilio).HasMaxLength(200);
            entity.Property(c => c.Email).HasMaxLength(100);
            entity.Property(c => c.CUIT).HasMaxLength(13);
            // ✅ Cbu ahora es opcional
            entity.Property(c => c.Cbu).HasMaxLength(22).IsRequired(false);

            entity.Property(c => c.Tipo)
                .HasConversion<string>()
                .IsRequired();
        });

        // 🔹 RegistroAsistencia → Empleado
        modelBuilder.Entity<RegistroAsistencia>()
            .HasOne(r => r.Empleado)
            .WithMany() // No necesitamos colección en ApplicationUser, si querés podés agregarla
            .HasForeignKey(r => r.EmpleadoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // 🔹 RegistroAsistencia → Presupuesto
        modelBuilder.Entity<RegistroAsistencia>()
            .HasOne(r => r.Presupuesto)
            .WithMany() // No necesitamos colección de registros en Presupuesto, opcional
            .HasForeignKey(r => r.PresupuestoId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        // 🔹 Observaciones opcionales: max 500
        modelBuilder.Entity<RegistroAsistencia>()
            .Property(r => r.Observaciones)
            .HasMaxLength(500);
    }

    // SaveChangesAsync con auditoría y limpieza de espacios
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified)
            .ToList();

        var currentUser = _httpContextAccessor.HttpContext?.User?.Identity?.Name ?? "Sistema";

        var argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Argentina Standard Time");
        var now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentinaTimeZone);

        foreach (var entry in entries)
        {
            if (entry.Entity is AuditableEntity entity)
            {
                if (entry.State == EntityState.Added)
                {
                    entity.CreatedDate = now;
                    entity.CreatedBy = currentUser;
                    entity.IsActive = true;
                }

                entity.ModifiedDate = now;
                entity.ModifiedBy = currentUser;
            }
        }

        foreach (var entry in entries)
        {
            var properties = entry.Entity.GetType().GetProperties()
                .Where(p => p.PropertyType == typeof(string) && p.CanRead && p.CanWrite);

            foreach (var prop in properties)
            {
                var currentValue = (string?)prop.GetValue(entry.Entity);
                if (!string.IsNullOrWhiteSpace(currentValue))
                {
                    var trimmed = RegexUtils.MultipleSpacesRegex().Replace(currentValue.Trim(), " ");
                    if (trimmed != currentValue)
                    {
                        prop.SetValue(entry.Entity, trimmed);
                    }
                }
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    private static partial class RegexUtils
    {
        [GeneratedRegex(@"\s{2,}")]
        internal static partial Regex MultipleSpacesRegex();
    }
}
