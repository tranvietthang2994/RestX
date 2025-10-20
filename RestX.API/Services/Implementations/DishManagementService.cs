using AutoMapper;
using RestX.API.Data.Repository.Interfaces;
using RestX.API.Models.ViewModels;
using RestX.API.Services.Interfaces;

namespace RestX.API.Services.Implementations
{
    public class DishManagementService : BaseService, IDishManagementService
    {
        private readonly IDishService dishService;
        private readonly ICategoryService categoryService;
        private readonly IMapper mapper;

        public DishManagementService(
            IRepository repo,
            IDishService dishService,
            ICategoryService categoryService,
            IHttpContextAccessor httpContextAccessor,
            IMapper mapper)
            : base(repo, httpContextAccessor)
        {
            this.dishService = dishService;
            this.categoryService = categoryService;
            this.mapper = mapper;
        }

        public async Task<DishesManagementViewModel> GetDishesAsync()
        {
            var dishes = await dishService.GetDishesByOwnerIdAsync();
            var categories = await categoryService.GetCategoriesAsync();

            return new DishesManagementViewModel
            {
                Dishes = mapper.Map<List<DishViewModel>>(dishes),
                Categories = mapper.Map<List<RestX.API.Models.DTOs.Response.CategoryDto>>(categories)
            };
        }
    }
}