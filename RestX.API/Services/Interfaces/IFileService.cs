namespace RestX.API.Services.Interfaces
{
    public interface IFileService
    {
        Task DeleteFileAsync(Guid fileId);
        Task<string> UploadDishImageAsync(IFormFile file, string ownerName, string dishName);
        Task<Models.Entities.File> CreateFileFromUploadAsync(string filePath, string fileName, Guid userId);
    }
}