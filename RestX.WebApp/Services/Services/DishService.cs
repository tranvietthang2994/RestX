using AutoMapper;
using RestX.WebApp.Helper;
using RestX.WebApp.Models;
using RestX.WebApp.Models.ViewModels;
using RestX.WebApp.Services;
using RestX.WebApp.Services.Interfaces;

namespace RestX.WebApp.Services.Services
{
    public class DishService : BaseService, IDishService
    {
        private readonly IOwnerService ownerService;
        private readonly IFileService fileService;
        private readonly IMapper mapper;

        public DishService(IRepository repo, IHttpContextAccessor httpContextAccessor, IOwnerService ownerService, IFileService fileService, IMapper mapper) : base(repo, httpContextAccessor)
        {
            this.ownerService = ownerService;
            this.fileService = fileService;
            this.mapper = mapper;
        }

        public async Task<List<Dish>> GetDishesByOwnerIdAsync()
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            var dishes = await Repo.GetAsync<Dish>(
                filter: d => d.OwnerId == ownerId && d.IsActive == true,
                includeProperties: "Category,File"
            );
            return dishes.OrderBy(d => d.Name).ToList();
        }

        public async Task<Dish> GetDishByIdAsync(int id)
        {
            var dishes = await Repo.GetAsync<Dish>(
                filter: d => d.Id == id,
                includeProperties: "Category,File"
            );
            return dishes.FirstOrDefault();
        }

        public async Task<DishViewModel> GetDishViewModelByIdAsync(int id)
        {
            var dish = await GetDishByIdAsync(id);
            if (dish == null) return null;

            return mapper.Map<DishViewModel>(dish);
        }

        public async Task<int> UpsertDishAsync(DataTransferObjects.Dish request)
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            Dish dish;
            bool isEdit = request.Id.HasValue && request.Id.Value > 0;

            if (isEdit)
            {
                dish = await GetDishByIdAsync(request.Id.Value);
                mapper.Map(request, dish);
                dish.OwnerId = ownerId;
            }
            else
            {
                dish = mapper.Map<Dish>(request);
                dish.OwnerId = ownerId;
            }

            if (request.ImageFile != null && request.ImageFile.Length > 0)
            {
                var owner = await ownerService.GetOwnerByIdAsync(ownerId);
                var imageUrl = await fileService.UploadDishImageAsync(request.ImageFile, owner.Name, request.Name);
                var file = await fileService.CreateFileFromUploadAsync(imageUrl, request.ImageFile.FileName, ownerId);
                dish.FileId = file.Id;
            }

            var ownerName = (await ownerService.GetOwnerByIdAsync(ownerId))?.Name;
            if (dish.Id == 0)
            {
                var result = await Repo.CreateAsync(dish, ownerName);
                await Repo.SaveAsync();
                return (int)result;
            }
            else
            {
                Repo.Update(dish, ownerName);
                await Repo.SaveAsync();
                return dish.Id;
            }
        }

        public async Task DeleteDishAsync(int id)
        {
            var dish = await GetDishByIdAsync(id);

            if (dish?.FileId.HasValue == true)
            {
                await fileService.DeleteFileAsync(dish.FileId.Value);
            }

            Repo.Delete<Dish>(id);
            await Repo.SaveAsync();
        }

        public async Task<bool> UpdateDishAvailabilityAsync(int dishId, bool isActive)
        {
            try
            {
                var ownerId = UserHelper.GetCurrentOwnerId();
                Console.WriteLine($"DishService: OwnerId={ownerId}, DishId={dishId}, IsActive={isActive}");
                
                var dish = await GetDishByIdAsync(dishId);
                
                if (dish == null)
                {
                    Console.WriteLine($"DishService: Dish not found for ID {dishId}");
                    return false;
                }
                
                Console.WriteLine($"DishService: Found dish '{dish.Name}', OwnerId={dish.OwnerId}");
                
                if (dish.OwnerId != ownerId)
                {
                    Console.WriteLine($"DishService: Owner mismatch. Dish OwnerId={dish.OwnerId}, Current OwnerId={ownerId}");
                    return false;
                }

                dish.IsActive = isActive;
                var ownerName = (await ownerService.GetOwnerByIdAsync(ownerId))?.Name;
                Console.WriteLine($"DishService: Updating dish with owner name: {ownerName}");
                
                Repo.Update(dish, ownerName);
                await Repo.SaveAsync();
                
                Console.WriteLine($"DishService: Successfully updated dish availability");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DishService Exception: {ex.Message}");
                Console.WriteLine($"DishService Stack Trace: {ex.StackTrace}");
                return false;
            }
        }
    }
}