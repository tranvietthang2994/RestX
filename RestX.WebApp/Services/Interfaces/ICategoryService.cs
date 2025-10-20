using RestX.WebApp.Models;
using RestX.WebApp.Services;

namespace RestX.WebApp.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<Category>> GetCategoriesAsync();
        Task<int> CreateCategoryAsync(DataTransferObjects.Category request);
        Task<Category?> GetCategoryByNameAsync(string name);
    }
}
