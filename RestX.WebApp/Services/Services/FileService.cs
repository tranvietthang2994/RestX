using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services.Interfaces;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace RestX.WebApp.Services.Services
{
    public class FileService : BaseService, IFileService
    {
        private readonly IWebHostEnvironment environment;
        private readonly IOwnerService ownerService;

        public FileService(IRepository repo, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment environment, IOwnerService ownerService) 
            : base(repo, httpContextAccessor) 
        {
            this.environment = environment;
            this.ownerService = ownerService;
        }

        public async Task UpdateFileAsync(Guid fileId, string name, string url, string userId)
        {
            var file = await Repo.GetByIdAsync<Models.File>(fileId);
            if (file != null)
            {
                file.Name = name;
                file.Url = url;
                Repo.Update(file, userId);
                await Repo.SaveAsync();
            }
        }

        public async Task DeleteFileAsync(Guid fileId)
        {
            var file = await Repo.GetByIdAsync<Models.File>(fileId);
            if (file != null)
            {
                // Delete physical file if it exists
                if (!string.IsNullOrEmpty(file.Url) && file.Url.StartsWith("~/"))
                {
                    var physicalPath = Path.Combine(environment.WebRootPath, file.Url.Replace("~/", "").Replace("/", Path.DirectorySeparatorChar.ToString()));
                    if (System.IO.File.Exists(physicalPath))
                    {
                        System.IO.File.Delete(physicalPath);
                    }
                }

                Repo.Delete(file);
            }
        }

        public async Task<string> UploadDishImageAsync(IFormFile file, string ownerName, string dishName)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            
            if (!allowedExtensions.Contains(extension))
                throw new ArgumentException("Invalid file type. Only image files are allowed.");

            var fileName = CreateDishImagePath(ownerName, dishName, extension);
            var uploadsFolder = Path.Combine(environment.WebRootPath, "Uploads", "DishesImage");
            var filePath = Path.Combine(uploadsFolder, fileName);

            Directory.CreateDirectory(uploadsFolder);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(fileStream);
            }

            return $"~/Uploads/DishesImage/{fileName}";
        }
        #region private
        private string CreateDishImagePath(string ownerName, string dishName, string extension)
        {
            var sanitizedOwnerName = SanitizeFileName(ownerName);
            var sanitizedDishName = SanitizeFileName(dishName);
            
            return $"Dish-{sanitizedOwnerName}-{sanitizedDishName}{extension}";
        }

        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return "Unknown";

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new string(fileName.Where(c => !invalidChars.Contains(c)).ToArray());
            return sanitized.Replace(" ", "_").Replace("-", "_");
        }

        #endregion
        public async Task<Models.File> CreateFileFromUploadAsync(string filePath, string fileName, Guid ownerId)
        {
            var ownerName = (await ownerService.GetOwnerByIdAsync(ownerId)).Name;
            var file = new Models.File
            {
                Id = Guid.NewGuid(),
                Name = fileName,
                Url = filePath
            };

            await Repo.CreateAsync(file, ownerName);
            return file;
        }
    }
}