using Microsoft.AspNetCore.Http;
using RestX.WebApp.Helper;
using RestX.WebApp.Models;
using RestX.WebApp.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RestX.WebApp.Services.Services
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