using ApiIngesol.Models;
using ApiIngesol.Repository.IRepository;
using AutoMapper;

namespace ApiIngesol.Repository
{
    public class BackgroundImageService
        : FileServiceBase<BackgroundImage, BackgroundImageCreateDto>,
          IFileService<BackgroundImage, BackgroundImageCreateDto>
    {
        public BackgroundImageService(
            IRepository<BackgroundImage> repository,
            IWebHostEnvironment env,
            IMapper mapper
        ) : base(repository, env, mapper)
        {
        }

        protected override async Task HandleFileAsync(BackgroundImageCreateDto dto, BackgroundImage entity, bool isUpdate)
        {
            if (dto.Image != null && dto.Image.Length > 0)
            {
                var ext = Path.GetExtension(dto.Image.FileName);
                var fileName = $"{Guid.NewGuid()}{ext}";
                var rootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                var folder = Path.Combine(rootPath, "Images", "Backgrounds");
                Directory.CreateDirectory(folder);
                var fullPath = Path.Combine(folder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await dto.Image.CopyToAsync(stream);
                }

                entity.ImagePath = $"/Images/Backgrounds/{fileName}";
                entity.LocalImagePath = fullPath;
            }
        }
    }
}
