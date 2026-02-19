using ApiIngesol.Models.Materiales;
using ApiIngesol.Repository.IRepository;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using ApiIngesol.Data; // <- Asegurate que este namespace corresponde a tu DbContext (AppDbContext)

namespace ApiIngesol.Repository
{
    public class MaterialService : IMaterialService
    {
        private readonly IRepository<Material> _repository;
        private readonly IMapper _mapper;
        private readonly IWebHostEnvironment _env;
        private readonly AppDbContext _db; // DbContext inyectado para Includes

        public MaterialService(
            IRepository<Material> repository,
            IMapper mapper,
            IWebHostEnvironment env,
            AppDbContext db) // <-- inyectar aquí
        {
            _repository = repository;
            _mapper = mapper;
            _env = env;
            _db = db;
        }

        public async Task<Material?> GetByIdAsync(Guid id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<bool> CreateAsync(MaterialCreateDto dto)
        {
            if (await ExistsWithSameDescriptionAsync(dto.Descripcion))
                throw new ArgumentException("Ya existe un material con esa descripción.");

            // =====================================================
            // 🔹 1. Obtener la Familia desde el Tipo
            // =====================================================
            var tipo = await _db.Tipos
                .Include(t => t.Clase)
                    .ThenInclude(c => c.Familia)
                .FirstOrDefaultAsync(t => t.Id == dto.TipoId);

            if (tipo?.Clase?.Familia == null)
                throw new Exception("No se pudo determinar la familia del material.");

            var familia = tipo.Clase.Familia;
            var codigoBaseFamilia = familia.Codigo; // int

            // =====================================================
            // 🔹 2. Buscar el mayor código de barra de esa familia
            // =====================================================
            var ultimoCodigo = await _db.Materiales
                .Where(m => m.Tipo!.Clase.FamiliaId == familia.Id)
                .MaxAsync(m => (int?)m.CodigoBarra);

            // =====================================================
            // 🔹 3. Asignar nuevo código
            // =====================================================
            dto.CodigoBarra = (ultimoCodigo ?? codigoBaseFamilia) + 1;

            // =====================================================
            // 🔹 4. Crear entidad
            // =====================================================
            var material = _mapper.Map<Material>(dto);

            await HandleImageUpload(dto, material, isUpdate: false);

            return await _repository.AddAsync(material);
        }


        public async Task<bool> UpdateAsync(Guid id, MaterialCreateDto dto)
        {
            var material = await _repository.GetByIdAsync(id);
            if (material == null)
                return false;

            if (await ExistsWithSameDescriptionAsync(dto.Descripcion, id))
                throw new ArgumentException("Ya existe otro material con esa descripción.");

            _mapper.Map(dto, material);
            await HandleImageUpload(dto, material, isUpdate: true);
            return await _repository.UpdateAsync(material);
        }

        private async Task HandleImageUpload(MaterialCreateDto dto, Material material, bool isUpdate = false)
        {
            if (dto.Image != null && dto.Image.Length > 0)
            {
                var ext = Path.GetExtension(dto.Image.FileName).ToLowerInvariant();
                var allowed = new[]
                {
                    ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".tiff", ".tif",
                    ".webp", ".svg", ".ico", ".heic", ".heif", ".jfif", ".pjpeg", ".pjp",
                    ".avif", ".apng", ".raw", ".cr2", ".nef", ".orf", ".sr2"
                };

                if (!allowed.Contains(ext))
                    throw new ArgumentException("Formato de imagen no permitido.");

                var maxSizeInMB = 2;
                var fileSizeInMB = dto.Image.Length / 1024.0 / 1024.0;

                if (fileSizeInMB > maxSizeInMB)
                    throw new ArgumentException($"La imagen no puede superar los {maxSizeInMB:F2} MB.");

                var newHash = await ComputeFileHashAsync(dto.Image);

                string? oldHash = isUpdate ? material.ImageHash : null;
                string? oldPath = isUpdate ? material.LocalImagePath : null;

                var existingMaterial = await GetExistingMaterialWithImageHashAsync(newHash);
                if (existingMaterial != null)
                {
                    material.ImagePath = existingMaterial.ImagePath;
                    material.LocalImagePath = existingMaterial.LocalImagePath;
                    material.ImageHash = existingMaterial.ImageHash;

                    if (isUpdate && oldHash != null && oldPath != null && oldHash != newHash)
                    {
                        await DeleteImageIfUnusedAsync(oldHash, oldPath);
                    }

                    return;
                }

                var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var folder = Path.Combine(rootPath, "Images", "Materials");
                Directory.CreateDirectory(folder);

                var newGuid = Guid.NewGuid();
                var fileName = $"{newGuid}{ext}";
                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                SetMaterialImage(material, newHash, fileName);

                if (isUpdate && oldHash != null && oldPath != null && oldHash != newHash)
                {
                    await DeleteImageIfUnusedAsync(oldHash, oldPath);
                }
            }
            else
            {
                if (!isUpdate)
                {
                    material.ImagePath = "/Images/Materials/SinImagen.png";
                    material.LocalImagePath = "/Images/Materials/SinImagen.png";
                    material.ImageHash = "a24f0fee8d4bcce3f2409b33ea3d87e48f24bfcff1d06decfc698304bfeb9827";
                }
            }
        }

        public async Task<bool> DeleteAsync(Guid id)
        {
            var material = await _repository.GetByIdAsync(id);
            if (material == null)
                return false;

            var result = await _repository.RemoveByIdAsync(id);

            if (result)
                await DeleteImageIfUnusedAsync(material.ImageHash, material.LocalImagePath);
            return result;
        }

        private Task<bool> ExistsWithSameDescriptionAsync(string descripcion, Guid? excludeId = null)
        {
            var descripcionNormalized = descripcion.Trim().ToLower();

            var response = _repository.ExistsAsync(m =>
                m.Descripcion != null &&
                m.Descripcion.Trim().ToLower() == descripcionNormalized &&
                (!excludeId.HasValue || m.Id != excludeId.Value));

            return response;
        }

        private async Task<Material?> GetExistingMaterialWithImageHashAsync(string imageHash)
        {
            var exists = await _repository.ExistsAsync(m =>
                m.ImageHash == imageHash &&
                !string.IsNullOrEmpty(m.LocalImagePath) &&
                !string.IsNullOrEmpty(m.ImagePath));

            if (!exists) return null;

            return await _repository.FirstOrDefaultAsync(m =>
                m.ImageHash == imageHash &&
                !string.IsNullOrEmpty(m.LocalImagePath) &&
                !string.IsNullOrEmpty(m.ImagePath));
        }

        private static void SetMaterialImage(Material material, string imageHash, string fileName)
        {
            material.ImageHash = imageHash;
            material.LocalImagePath = Path.Combine("Images", "Materials", fileName);
            material.ImagePath = $"/Images/Materials/{fileName}";
        }

        private static async Task<string> ComputeFileHashAsync(IFormFile file)
        {
            using var sha256 = SHA256.Create();
            using var stream = file.OpenReadStream();
            var hashBytes = await sha256.ComputeHashAsync(stream);
            return Convert.ToHexString(hashBytes).ToLowerInvariant();
        }

        private async Task DeleteImageIfUnusedAsync(string? imageHash, string? localImagePath)
        {
            if (string.IsNullOrEmpty(imageHash) || string.IsNullOrEmpty(localImagePath))
                return;

            var allMaterials = await _repository.GetAllAsync();
            var isUsedByOthers = allMaterials.Any(m =>
                m.ImageHash == imageHash &&
                m.LocalImagePath == localImagePath);

            if (!isUsedByOthers)
            {
                var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var fullPath = Path.Combine(rootPath, localImagePath);

                if (File.Exists(fullPath))
                    File.Delete(fullPath);
            }
        }

        public async Task<bool> ExistsAsync(string nombre, string descripcion)
        {
            nombre = nombre.ToLower();
            descripcion = descripcion.ToLower();

            return await _repository.ExistsAsync(m =>
                m.Descripcion.ToLower() == descripcion || m.Descripcion.ToLower() == nombre);
        }

        public IQueryable<MaterialReadDto> QueryReadDto()
        {
            var response = _db.Materiales
                .AsNoTracking()
                .Select(m => new MaterialReadDto
                {
                    Id = m.Id,
                    Nombre = m.Nombre,
                    Descripcion = m.Descripcion,

                    ImagePath = m.ImagePath,
                    LocalImagePath = m.LocalImagePath,

                    TipoId = m.TipoId,
                    TipoNombre = m.Tipo!.Nombre,

                    UnidadMedidaId = m.UnidadMedidaId,
                    UnidadNombre = m.UnidadMedida!.Nombre,

                    CodigoBarra = m.CodigoBarra,
                    PesoUnitario = m.PesoUnitario,
                    PrecioUnitario = m.PrecioUnitario,

                    ClaseNombre = m.Tipo.Clase.Nombre,
                    FamiliaNombre = m.Tipo.Clase.Familia.Nombre,
                    IsActive = m.IsActive,

                    IsFavorite = m.IsFavorite
                });
            return response;
        }
    
    }
}
