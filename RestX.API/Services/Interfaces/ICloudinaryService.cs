using RestX.API.Models.DTOs.Response;

namespace RestX.API.Services.Interfaces
{
    public interface ICloudinaryService
    {
        Task<FileUploadResult> UploadImageAsync(IFormFile imageFile, string folder = "items");
        Task<bool> DeleteImageAsync(string publicId);
        Task<Models.Entities.File> SaveFileInfoAsync(string name, string url, Guid uploadedBy);
    }
}