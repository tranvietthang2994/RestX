using RestX.API.Models.DTOs.Request;
using RestX.API.Models.Entities;

namespace RestX.API.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetCategoriesAsync();
        Task<int> CreateCategoryAsync(CategoryRequest request);
        Task<Category?> GetCategoryByNameAsync(string name);
    }
}
