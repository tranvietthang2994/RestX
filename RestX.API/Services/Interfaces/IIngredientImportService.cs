using RestX.API.Models.Entities;

namespace RestX.API.Services.Interfaces
{
    public interface IIngredientImportService
    {
        Task<List<IngredientImport>> GetIngredientImportsByOwnerIdAsync(CancellationToken cancellationToken = default);
    }
}