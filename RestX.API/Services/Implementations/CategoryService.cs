using AutoMapper;
using RestX.API.Data.Repository.Interfaces;
using RestX.API.Extensions;
using RestX.API.Models.DTOs.Request;
using RestX.API.Models.Entities;
using RestX.API.Services.Interfaces;

namespace RestX.API.Services.Implementations
{
    public class CategoryService : BaseService, ICategoryService
    {
        private readonly IMapper mapper;
        private readonly IOwnerService ownerService;
        public CategoryService(IRepository repo,  IHttpContextAccessor httpContextAccessor, IMapper mapper, IOwnerService ownerService) : base(repo, httpContextAccessor)
        {
            this.mapper = mapper;
            this.ownerService = ownerService;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            var categories = await Repo.GetAllAsync<Category>();
            return categories.ToList();
        }

        public async Task<int> CreateCategoryAsync(CategoryRequest request)
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            var userName = (await ownerService.GetOwnerByIdAsync(ownerId))?.Name;
            var existingCategory = await GetCategoryByNameAsync(request.Name);
            if (existingCategory != null)
                throw new InvalidOperationException("Category with this name already exists.");

            var category = mapper.Map<Category>(request);
            var result = await Repo.CreateAsync(category, userName);
            await Repo.SaveAsync();
            return (int)result;
        }

        public async Task<Category?> GetCategoryByNameAsync(string name)
        {
            var categories = await Repo.GetAsync<Category>(c => c.Name.ToLower() == name.ToLower().Trim());
            return categories.FirstOrDefault();
        }
    }
}