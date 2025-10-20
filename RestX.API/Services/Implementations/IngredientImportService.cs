using RestX.API.Data.Repository.Interfaces;
using RestX.API.Extensions;
using RestX.API.Models.Entities;
using RestX.API.Services.Interfaces;

namespace RestX.API.Services.Implementations
{
    public class IngredientImportService : BaseService, IIngredientImportService
    {
        public IngredientImportService(IRepository repo, IHttpContextAccessor httpContextAccessor)
            : base(repo, httpContextAccessor)
        {
        }

        public async Task<List<IngredientImport>> GetIngredientImportsByOwnerIdAsync(CancellationToken cancellationToken = default)
        {
            var ownerId = UserHelper.GetCurrentOwnerId();
            var imports = await Repo.GetAsync<IngredientImport>(i => i.Ingredient.OwnerId == ownerId, includeProperties: "Ingredient");
            return imports.ToList();
        }
    }
}