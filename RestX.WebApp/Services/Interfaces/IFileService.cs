using RestX.WebApp.Models;
using System;
using System.Threading.Tasks;

namespace RestX.WebApp.Services.Interfaces
{
    public interface IFileService
    {
        Task DeleteFileAsync(Guid fileId);
        Task<string> UploadDishImageAsync(IFormFile file, string ownerName, string dishName);
        Task<Models.File> CreateFileFromUploadAsync(string filePath, string fileName, Guid userId);
    }
}